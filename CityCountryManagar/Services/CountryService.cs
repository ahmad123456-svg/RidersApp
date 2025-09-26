using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

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
    }
}
