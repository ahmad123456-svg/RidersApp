using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Http;

namespace RidersApp.Services
{
    public class CountryService : ICountryService
    {
        private readonly ICountryRepository _countryRepository;

        public CountryService(ICountryRepository countryRepository)
        {
            _countryRepository = countryRepository;
        }

        public async Task<List<CountryVM>> GetAll()
        {
            var countries = await _countryRepository.GetAllAsync();
            return countries
                .OrderBy(c => (c.Name ?? string.Empty).ToLowerInvariant())
                .Select(c => new CountryVM
                {
                    CountryId = c.CountryId,
                    Name = c.Name
                }).ToList();
        }

        public async Task<CountryVM> GetById(int id)
        {
            var entity = await _countryRepository.GetByIdAsync(id);
            if (entity == null) return null;

            return new CountryVM
            {
                CountryId = entity.CountryId,
                Name = entity.Name
            };
        }

        public async Task<List<CountryVM>> Add(CountryVM vm)
        {
            var entity = new Country { Name = vm.Name };
            await _countryRepository.AddAsync(entity);
            return await GetAll();
        }

        public async Task<List<CountryVM>> Edit(CountryVM vm)
        {
            var entity = await _countryRepository.GetByIdAsync(vm.CountryId);
            if (entity != null)
            {
                entity.Name = vm.Name;
                await _countryRepository.UpdateAsync(entity);
            }
            return await GetAll();
        }

        public async Task<List<CountryVM>> Delete(int id)
        {
            var country = await _countryRepository.GetByIdAsync(id);
            if (country == null)
                throw new InvalidOperationException("Country not found");

            if (await _countryRepository.HasRelatedCitiesAsync(id))
                throw new InvalidOperationException($"Cannot delete country '{country.Name}' because it has related cities.");

            if (await _countryRepository.HasRelatedEmployeesAsync(id))
                throw new InvalidOperationException($"Cannot delete country '{country.Name}' because it has related employees.");

            await _countryRepository.DeleteAsync(id);
            return await GetAll();
        }

        // DataTables logic moved from controller
        public async Task<object> GetCountriesData(IFormCollection form)
        {
            var draw = form["draw"].FirstOrDefault();
            var start = int.TryParse(form["start"].FirstOrDefault(), out int s) ? s : 0;
            var length = int.TryParse(form["length"].FirstOrDefault(), out int l) ? l : 10;
            var searchValue = form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = form["order[0][column]"].FirstOrDefault();
            var sortDirection = form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);

            string[] columnNames = new[] { "Name" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var all = await GetAll();
            var query = all.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(x => (x.Name ?? string.Empty).ToLower().Contains(searchValue.ToLower()));
            }

            var recordsFiltered = query.Count();

            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = sortColumn switch
            {
                "Name" => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                _ => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name)
            };

            var pageData = query.Skip(start).Take(length).Select(x => new
            {
                name = x.Name,
                countryId = x.CountryId
            }).ToList();

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
