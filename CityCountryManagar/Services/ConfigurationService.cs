using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RidersApp.Services
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfigurationRepository _repo;

        public ConfigurationService(IConfigurationRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<ConfigurationVM>> Add(ConfigurationVM vm)
        {
            var entity = new Configuration
            {
                KeyName = vm.KeyName,
                Value = vm.Value
            };

            await _repo.AddAsync(entity);

            // Return configurations ordered alphabetically
            return await GetAll();
        }

        public async Task<List<ConfigurationVM>> Edit(ConfigurationVM vm)
        {
            var entity = await _repo.GetByIdAsync(vm.ConfigurationId);
            if (entity != null)
            {
                // Do not allow changing KeyName on edit — only update the Value
                entity.Value = vm.Value;
                await _repo.UpdateAsync(entity);
            }

            // Return configurations ordered alphabetically
            return await GetAll();
        }

        public async Task<List<ConfigurationVM>> GetAll()
        {
            var items = await _repo.GetAllAsync();
            return items
                .OrderBy(i => (i.KeyName ?? string.Empty).ToLowerInvariant())
                .Select(i => new ConfigurationVM
                {
                    ConfigurationId = i.ConfigurationId,
                    KeyName = i.KeyName,
                    Value = i.Value
                }).ToList();
        }

        public async Task<ConfigurationVM> GetById(int id)
        {
            var entity = await _repo.GetByIdAsync(id);
            if (entity == null) return null;
            return new ConfigurationVM
            {
                ConfigurationId = entity.ConfigurationId,
                KeyName = entity.KeyName,
                Value = entity.Value
            };
        }

        public async Task<List<ConfigurationVM>> Delete(int id)
        {
            await _repo.DeleteAsync(id);
            
            // Return configurations ordered alphabetically
            return await GetAll();
        }
    }
}
