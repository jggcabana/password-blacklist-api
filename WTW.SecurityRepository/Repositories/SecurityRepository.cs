using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTW.SecurityRepository.Interfaces;

namespace WTW.SecurityRepository.Repositories
{
    public class SecurityRepository : BaseRepository, ISecurityRepository
    {
        public SecurityRepository(IConfiguration config) : base(config)
        {

        }

        public async Task<string> Test()
        {
            return await GetAsync<string, dynamic>("SELECT [Password] FROM PasswordBlacklist WHERE Id=@Id", new { Id = 69 });
        }

        public async Task<IEnumerable<string>> GetAllBlacklistedPasswords()
        {
            return await GetAllAsync<string>("SELECT [Password] FROM PasswordBlacklist");
        }

        public async Task<string> GetBlacklistedPassword(string password)
        {
            return await GetAsync<string, dynamic>("SELECT [Password] FROM PasswordBlacklist WHERE [Password]=@Password COLLATE SQL_Latin1_General_CP1_CS_AS", new { Password = password });
        }
    }
}
