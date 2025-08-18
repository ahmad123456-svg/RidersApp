using System.Collections.Generic;
using System.Threading.Tasks;
using RidersApp.DbModels;

namespace RidersApp.Interfaces
{
    public interface IDailyRidesRepository
    {
        Task<IEnumerable<DailyRides>> GetAllAsync();
        Task<DailyRides> GetByIdAsync(int id);
        Task AddAsync(DailyRides dailyRides);
        Task UpdateAsync(DailyRides dailyRides);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}
