using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Security.Cryptography;
using WTW.SecurityRepository.Interfaces;

// TODO: this should be a singleton class
public class BloomFilter : IBloomFilter
{
    private readonly int _hashFunctionCount; // Number of hash functions
    private readonly BitArray _bitArray;     // Bit array to store data
    private readonly ILogger<BloomFilter> _logger;
    private readonly ISecurityRepository _securityRepository;

    public bool HasData { get; private set; } = false;

    public BloomFilter(IConfiguration config, ILogger<BloomFilter> logger, ISecurityRepository securityRepository)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        int expectedItemCount = Convert.ToInt32(config.GetSection("BloomFilterSettings")["ExpectedItemCount"]);
        double falsePositiveRate = Convert.ToDouble(config.GetSection("BloomFilterSettings")["FalsePositiveRate"]);

        _securityRepository = securityRepository ?? throw new ArgumentNullException(nameof(securityRepository));

        try
        {
            _logger.LogInformation($"Creating bloom filter with expected item count of {expectedItemCount} and false positive rate of {falsePositiveRate}.");

            // Calculate the optimal size of the bit array based on the expected item count and false positive rate
            int bitArraySize = GetBitArraySize(expectedItemCount, falsePositiveRate);

            // Initialize the bit array with the calculated size
            _bitArray = new BitArray(bitArraySize);

            // Calculate the optimal number of hash functions based on the bit array size and expected item count
            _hashFunctionCount = GetHashFunctionCount(expectedItemCount, bitArraySize);

            _logger.LogInformation($"Bloom filter successfully created");
            _logger.LogInformation($"Bit array size      : {bitArraySize}");
            _logger.LogInformation($"Hash function count : {_hashFunctionCount}");

        }
        catch (Exception e)
        {
            _logger.LogError($"Bloom filter instantation failed.", e.Message);
            throw;
        }
    }

    public void Add(string item)
    {
        // Get the indexes produced by hash functions and set the corresponding bits to true in the bit array
        foreach (int index in GetHashIndexes(item))
        {
            _bitArray[index] = true;
        }
    }

    public bool Contains(string item)
    {
        // Check if all corresponding bits are true in the bit array
        foreach (int index in GetHashIndexes(item))
        {
            if (!_bitArray[index])
            {
                _logger.LogInformation("Password is not in the hash.");
                return false; // If any bit is false, the item is definitely not in the set
            }
        }
        _logger.LogInformation("Password is in the hash, indicating that the password might be blacklisted.");
        return true; // All bits are true, indicating the item may be in the set (potential false positive)
    }

    private int[] GetHashIndexes(string item)
    {
        var hash1 = item.GetHashCode();
        var hash2 = hash1 >> 16;

        var hashes = new int[_hashFunctionCount];
        for (int i = 0; i < _hashFunctionCount; i++)
        {
            hashes[i] = Math.Abs((hash1 + i * hash2) % _bitArray.Count);
        }

        return hashes;
    }

    private int GetBitArraySize(int expectedItemCount, double falsePositiveRate)
    {
        // Calculate the optimal bit array size to achieve the desired false positive rate
        int size = (int)Math.Ceiling(expectedItemCount * Math.Log(falsePositiveRate) / Math.Log(1.0 / (Math.Pow(2.0, Math.Log(2.0)))));
        return size;
    }

    private int GetHashFunctionCount(int expectedItemCount, int bitArraySize)
    {
        // Calculate the optimal number of hash functions based on bit array size and expected item count
        int count = (int)Math.Round((bitArraySize / expectedItemCount) * Math.Log(2.0));
        return count;
    }

    public async Task LoadData()
    {
        try
        {
            _logger.LogInformation($"Populating bloom filter...");

            string[] blacklist = (await _securityRepository.GetAllBlacklistedPasswords()).ToArray<string>();

            int count = 0;
            foreach (var password in blacklist)
            {
                Add(password);
                _logger.LogInformation($"Adding entry {count++}");
            }

            _logger.LogInformation($"Populating bloom filter succeeded.");

            HasData = true;
        }
        catch (Exception e)
        {
            _logger.LogError("Populating bloom filter failed.", e.Message);
            throw;
        }
    }
}
