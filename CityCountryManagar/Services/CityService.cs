using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System;

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
    }
}
