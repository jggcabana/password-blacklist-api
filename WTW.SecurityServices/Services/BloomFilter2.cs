using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Security.Cryptography;
using WTW.SecurityRepository.Interfaces;

// TODO: this should be a singleton class
public class BloomFilter2 : IBloomFilter
{
    private readonly int _hashFunctionCount; // Number of hash functions
    private readonly BitArray _bitArray;     // Bit array to store data
    private readonly ILogger<BloomFilter> _logger;
    private readonly ISecurityRepository _securityRepository;

    public bool HasData => throw new NotImplementedException();

    //public BloomFilter(int expectedItemCount, double falsePositiveRate)
    public BloomFilter2(IConfiguration config, ILogger<BloomFilter> logger, ISecurityRepository securityRepository)
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
                return false; // If any bit is false, the item is definitely not in the set
            }
        }
        return true; // All bits are true, indicating the item may be in the set (potential false positive)
    }

    private int[] GetHashIndexes(string item)
    {
        int[] indexes = new int[_hashFunctionCount];

        // Compute hash values using MD5 hash function
        using (var md5 = MD5.Create())
        {
            byte[] inputBytes = System.Text.Encoding.UTF8.GetBytes(item);
            byte[] hashBytes = md5.ComputeHash(inputBytes);

            try
            {
                // Map hash values to indexes within the bit array
                for (int i = 0; i < _hashFunctionCount; i++)
                {
                    int hash = BitConverter.ToInt32(hashBytes, i * 4);
                    indexes[i] = Math.Abs(hash) % _bitArray.Length;
                }
            }
            catch (Exception e)
            {
                _logger.LogError("xd", e);
                throw;
            }
        }
        return indexes;
    }

    private int GetBitArraySize(int expectedItemCount, double falsePositiveRate)
    {
        // Calculate the optimal bit array size to achieve the desired false positive rate
        int size = (int)Math.Ceiling((-expectedItemCount * Math.Log(falsePositiveRate)) / Math.Pow(Math.Log(2), 2));
        return size;
    }

    private int GetHashFunctionCount(int expectedItemCount, int bitArraySize)
    {
        // Calculate the optimal number of hash functions based on bit array size and expected item count
        int count = (int)Math.Ceiling((bitArraySize / (double)expectedItemCount) * Math.Log(2));
        return count;
    }

    public async Task LoadData()
    {
        try
        {
            _logger.LogInformation($"Populating bloom filter...");

            string[] blacklist = (await _securityRepository.GetAllBlacklistedPasswords()).ToArray<string>();
            foreach (var password in blacklist)
            {
                Add(password);
            }
        }
        catch (Exception e)
        {
            _logger.LogError("Populating bloom filter failed.", e.Message);
            throw;
        }
    }
}
