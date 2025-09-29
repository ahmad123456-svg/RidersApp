using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System;
using Microsoft.AspNetCore.Http;

namespace RidersApp.Services
{
    public class CityService : ICityService
    {
        private readonly ICityRepository _cityRepository;

        public CityService(ICityRepository cityRepository)
        {
            _cityRepository = cityRepository;
        }

        public async Task<List<CityVM>> GetAll()
        {
            var cities = await _cityRepository.GetAllAsync();
            return cities
                .OrderBy(c => (c.CityName ?? string.Empty).ToLowerInvariant())
                .Select(c => new CityVM
                {
                    CityId = c.CityId,
                    CityName = c.CityName,
                    PostalCode = c.PostalCode,
                    CountryId = c.CountryId,
                    CountryName = c.Country?.Name
                }).ToList();
        }

        // Fix GetById to match interface return type
        public async Task<string?> GetById(int id)
        {
            var city = await _cityRepository.GetByIdAsync(id);
            if (city == null) return null;
            // Return a string representation as expected by the interface
            return $"{city.CityName} ({city.PostalCode}), {city.Country?.Name}";
        }

        public async Task<List<CityVM>> Add(CityVM vm)
        {
            var entity = new City
            {
                CityName = vm.CityName,
                PostalCode = vm.PostalCode,
                CountryId = vm.CountryId
            };

            await _cityRepository.AddAsync(entity);
            return await GetAll();
        }

        public async Task<List<CityVM>> Edit(CityVM vm)
        {
            var entity = await _cityRepository.GetByIdAsync(vm.CityId);
            if (entity == null)
                throw new InvalidOperationException("City not found");

            entity.CityName = vm.CityName;
            entity.PostalCode = vm.PostalCode;
            entity.CountryId = vm.CountryId;

            await _cityRepository.UpdateAsync(entity);
            return await GetAll();
        }

        public async Task<List<CityVM>> Delete(int id)
        {
            var city = await _cityRepository.GetByIdAsync(id);
            if (city == null)
                throw new InvalidOperationException("City not found");

            if (await _cityRepository.HasRelatedEmployeesAsync(id))
                throw new InvalidOperationException($"Cannot delete city '{city.CityName}' because it has related employees.");

            await _cityRepository.DeleteAsync(id);
            return await GetAll();
        }

        // Use repository GetByCountryAsync to avoid loading all cities into memory
        public async Task<IEnumerable<object>> GetByCountry(int countryId)
        {
            var cities = await _cityRepository.GetByCountryAsync(countryId);
            return cities
                .Select(c => new
                {
                    c.CityId,
                    c.CityName,
                    c.PostalCode,
                    c.CountryId,
                    CountryName = c.Country?.Name
                })
                .Cast<object>()
                .ToList();
        }

        // Move server-side DataTables processing here
        public async Task<object> GetCitiesData(IFormCollection form)
        {
            var draw = form["draw"].FirstOrDefault();
            var start = int.TryParse(form["start"].FirstOrDefault(), out int s) ? s : 0;
            var length = int.TryParse(form["length"].FirstOrDefault(), out int l) ? l : 10;
            var searchValue = form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = form["order[0][column]"].FirstOrDefault();
            var sortDirection = form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);
            string[] columnNames = { "CityName", "PostalCode", "CountryName" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : "CityName";

            var cities = await GetAll();
            var query = cities.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.CityName ?? "").ToLower().Contains(lower) ||
                    (x.PostalCode ?? "").ToLower().Contains(lower) ||
                    (x.CountryName ?? "").ToLower().Contains(lower));
            }

            var recordsFiltered = query.Count();
            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            query = sortColumn switch
            {
                "CityName" => ascending ? query.OrderBy(x => x.CityName) : query.OrderByDescending(x => x.CityName),
                "PostalCode" => ascending ? query.OrderBy(x => x.PostalCode) : query.OrderByDescending(x => x.PostalCode),
                "CountryName" => ascending ? query.OrderBy(x => x.CountryName) : query.OrderByDescending(x => x.CountryName),
                _ => ascending ? query.OrderBy(x => x.CityName) : query.OrderByDescending(x => x.CityName)
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
