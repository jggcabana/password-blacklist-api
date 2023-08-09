using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTW.SecurityRepository.Interfaces;
using WTW.SecurityServices.Interfaces;

namespace WTW.SecurityServices.Services
{
    public class SecurityService : ISecurityService
    {
        private readonly ISecurityRepository _securityRepository;
        private readonly IBloomFilter _bloomFilter;
        private readonly ILogger _logger;

        public SecurityService(ILogger<ISecurityService> logger, ISecurityRepository securityRepository, IBloomFilter bloomFilter)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _securityRepository = securityRepository ?? throw new ArgumentNullException(nameof(securityRepository));
            _bloomFilter = bloomFilter ?? throw new ArgumentNullException(nameof(bloomFilter));
        }

        public Task<bool> RebuildBloomFilter()
        {
            throw new NotImplementedException();
        }

        public async Task<string> Test()
        {
            return await _securityRepository.Test();
        }

        public Task<bool> UpdatePasswordBlacklist()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsValidPassword(string password)
        {
            if (_bloomFilter.Contains(password))
            {
                _logger.LogInformation("Searching DB for a possible false positive...");
                var result = await _securityRepository.GetBlacklistedPassword(password);
                if (string.IsNullOrEmpty(result))
                {
                    _logger.LogInformation("No matches found.");
                    return true;
                }
                _logger.LogInformation("Password is blacklisted.");
                return false;
            }
            return true;
        }
    }
}
