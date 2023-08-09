namespace WTW.SecurityRepository.Interfaces
{
    public interface IBaseRepository
    {
        Task<T> GetAsync<T, U>(string sql, U parameters, string connectionId = "DefaultConnection");
        Task<IEnumerable<T>> GetAllAsync<T>(string sql, string connectionId = "DefaultConnection");
    }
}