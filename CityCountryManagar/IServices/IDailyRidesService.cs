using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RidersApp.IServices
{
    public interface IDailyRidesService
    {
        Task<List<DailyRidesVM>> GetAll();
        Task<DailyRidesVM> GetById(int id);
        Task<List<DailyRidesVM>> Add(DailyRidesVM vm);
        Task<List<DailyRidesVM>> Edit(DailyRidesVM vm);
        Task<List<DailyRidesVM>> Delete(int id);
        Task<object> GetDailyRidesData(IFormCollection form);
    }
}
