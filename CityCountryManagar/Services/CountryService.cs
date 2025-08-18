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

        public async Task<List<CountryVM>> Add(CountryVM vm)
        {
            var entity = new Country
            {
                Name = vm.Name
            };

            await _countryRepository.AddAsync(entity);

            var countries = await _countryRepository.GetAllAsync();
            return countries.Select(c => new CountryVM
            {
                CountryId = c.CountryId,
                Name = c.Name
            }).ToList();
        }

        public async Task<List<CountryVM>> Edit(CountryVM vm)
        {
            var entity = await _countryRepository.GetByIdAsync(vm.CountryId);
            if (entity != null)
            {
                entity.Name = vm.Name;
                await _countryRepository.UpdateAsync(entity);
            }

            var countries = await _countryRepository.GetAllAsync();
            return countries.Select(c => new CountryVM
            {
                CountryId = c.CountryId,
                Name = c.Name
            }).ToList();
        }

        public async Task<List<CountryVM>> GetAll()
        {
            var countries = await _countryRepository.GetAllAsync();
            return countries.Select(c => new CountryVM
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

        public async Task<List<CountryVM>> Delete(int id)
        {
            try
            {
                // Check if country exists
                var country = await _countryRepository.GetByIdAsync(id);
                if (country == null)
                {
                    throw new InvalidOperationException($"Country with ID {id} not found.");
                }

                // Check if country has related cities
                var hasCities = await _countryRepository.HasRelatedCitiesAsync(id);
                if (hasCities)
                {
                    throw new InvalidOperationException($"Cannot delete country '{country.Name}' because it has related cities. Please delete or reassign the cities first.");
                }

                // Check if country has related employees
                var hasEmployees = await _countryRepository.HasRelatedEmployeesAsync(id);
                if (hasEmployees)
                {
                    throw new InvalidOperationException($"Cannot delete country '{country.Name}' because it has related employees. Please delete or reassign the employees first.");
                }

                // Delete the country
                await _countryRepository.DeleteAsync(id);

                // Return updated list
                var countries = await _countryRepository.GetAllAsync();
                return countries.Select(c => new CountryVM
                {
                    CountryId = c.CountryId,
                    Name = c.Name
                }).ToList();
            }
            catch (Exception ex)
            {
                // Log the error (you can implement proper logging here)
                var errorMessage = $"Failed to delete country with ID {id}. Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner error: {ex.InnerException.Message}";
                }
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
    }
}
