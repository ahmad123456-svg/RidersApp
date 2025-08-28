using System.Collections.Generic;
using System.Threading.Tasks;
using RidersApp.DbModels;

namespace RidersApp.Interfaces
{
    public interface IConfigurationRepository
    {
        Task<IEnumerable<Configuration>> GetAllAsync();
    Task<Configuration> GetByIdAsync(int id);
        Task AddAsync(Configuration config);
        Task UpdateAsync(Configuration config);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
    }
}
