using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WTW.SecurityRepository.Interfaces
{
    public interface ISecurityRepository : IBaseRepository
    {
        Task<string> Test();

        Task<IEnumerable<string>> GetAllBlacklistedPasswords();

        Task<string> GetBlacklistedPassword(string password);
    }
}
