using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RidersApp.IServices
{
    public interface ICityService
    {
        Task<List<CityVM>> GetAll();
        Task<List<CityVM>> Add(CityVM vm);
        Task<List<CityVM>> Edit(CityVM vm);
        Task<List<CityVM>> Delete(int id);
    }
}
