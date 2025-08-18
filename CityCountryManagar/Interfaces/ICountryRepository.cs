using System.Collections.Generic;
using System.Threading.Tasks;
using RidersApp.DbModels;

namespace RidersApp.Interfaces
{
    public interface ICountryRepository
    {
        Task<IEnumerable<Country>> GetAllAsync();
        Task<Country> GetByIdAsync(int id);
        Task AddAsync(Country country);
        Task UpdateAsync(Country country);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
        Task<bool> HasRelatedCitiesAsync(int countryId);
        Task<bool> HasRelatedEmployeesAsync(int countryId);
    }
}
