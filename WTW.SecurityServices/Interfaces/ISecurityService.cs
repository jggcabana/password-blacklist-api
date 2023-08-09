using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTW.SecurityServices.Interfaces
{
    public interface ISecurityService
    {
        Task<bool> IsValidPassword(string password);

        Task<bool> RebuildBloomFilter();

        Task<bool> UpdatePasswordBlacklist();

        Task<string> Test();
    }
}
