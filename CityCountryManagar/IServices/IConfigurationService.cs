using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using RidersApp.ViewModels;

namespace RidersApp.IServices
{
    public interface IConfigurationService
    {
        Task<List<ConfigurationVM>> GetAll();
        Task<ConfigurationVM> GetById(int id);
        Task<List<ConfigurationVM>> Add(ConfigurationVM vm);
        Task<List<ConfigurationVM>> Edit(ConfigurationVM vm);
        Task<List<ConfigurationVM>> Delete(int id);
        Task<object> GetConfigurationsData(IFormCollection form);
    }
}
