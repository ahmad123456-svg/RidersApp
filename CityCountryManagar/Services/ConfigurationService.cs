using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

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
            return await GetAll();
        }

        public async Task<List<ConfigurationVM>> Edit(ConfigurationVM vm)
        {
            var entity = await _repo.GetByIdAsync(vm.ConfigurationId);
            if (entity != null)
            {
                entity.Value = vm.Value; // only update Value
                await _repo.UpdateAsync(entity);
            }
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
            return await GetAll();
        }

        // DataTables logic moved from controller
        public async Task<object> GetConfigurationsData(IFormCollection form)
        {
            var draw = form["draw"].FirstOrDefault();
            var start = int.TryParse(form["start"].FirstOrDefault(), out int s) ? s : 0;
            var length = int.TryParse(form["length"].FirstOrDefault(), out int l) ? l : 10;
            var searchValue = form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = form["order[0][column]"].FirstOrDefault();
            var sortDirection = form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);
            string[] columnNames = new[] { "KeyName", "Value" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var all = await GetAll();
            var query = all.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.KeyName ?? string.Empty).ToLower().Contains(lower) ||
                    (x.Value ?? string.Empty).ToLower().Contains(lower)
                );
            }

            var recordsFiltered = query.Count();

            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = sortColumn switch
            {
                "KeyName" => ascending ? query.OrderBy(x => x.KeyName) : query.OrderByDescending(x => x.KeyName),
                "Value" => ascending ? query.OrderBy(x => x.Value) : query.OrderByDescending(x => x.Value),
                _ => ascending ? query.OrderBy(x => x.KeyName) : query.OrderByDescending(x => x.KeyName)
            };

            var pageData = query.Skip(start).Take(length).ToList();

            return new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = pageData
            };
        }
    }
}
