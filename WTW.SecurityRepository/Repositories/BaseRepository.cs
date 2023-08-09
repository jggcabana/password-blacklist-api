using Dapper;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WTW.SecurityRepository.Interfaces;

namespace WTW.SecurityRepository.Repositories
{
    public class BaseRepository : IBaseRepository
    {
        private readonly IConfiguration _config;

        public BaseRepository(IConfiguration config)
        {
            _config = config;
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string sql, string connectionId = "DefaultConnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

            return await connection.QueryAsync<T>(sql);
        }

        public async Task<T> GetAsync<T, U>(string sql, U parameters, string connectionId = "DefaultConnection")
        {
            using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

            return await connection.QueryFirstOrDefaultAsync<T>(sql, parameters);
        }
    }
}
