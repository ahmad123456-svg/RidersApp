using System.Collections.Generic;
using System.Threading.Tasks;
using RidersApp.DbModels;

namespace RidersApp.Interfaces
{
    public interface ICityRepository
    {
        Task<IEnumerable<City>> GetAllAsync();
        Task<City> GetByIdAsync(int id);
        Task<List<City>> GetByCountryAsync(int countryId);
        Task AddAsync(City city);
        Task UpdateAsync(City city);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasRelatedEmployeesAsync(int cityId);
    }
}
