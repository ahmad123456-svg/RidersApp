using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RidersApp.IServices
{
    public interface ICityService
    {
        Task<List<CityVM>> GetAll();
        Task<List<CityVM>> Add(CityVM vm);
        Task<List<CityVM>> Edit(CityVM vm);
        Task<List<CityVM>> Delete(int id);
        Task<IEnumerable<object>> GetByCountry(int countryId);
        Task<string?> GetById(int id);

        // Support DataTables server-side processing moved from controller
        Task<object> GetCitiesData(IFormCollection form);
    }
}
