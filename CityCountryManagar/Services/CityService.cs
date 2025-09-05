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
        private readonly ICountryRepository _countryRepository;

        public CityService(ICityRepository cityRepository, ICountryRepository countryRepository)
        {
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
        }

        public async Task<List<CityVM>> GetAll()
        {
            var cities = await _cityRepository.GetAllAsync();
            // Return cities ordered alphabetically by CityName (case-insensitive)
            return cities
                .OrderBy(c => (c.CityName ?? string.Empty).ToLowerInvariant())
                .Select(c => new CityVM
                {
                    CityId = c.CityId,
                    CityName = c.CityName,
                    PostalCode = c.PostalCode,
                    CountryId = c.CountryId,
                    CountryName = c.Country != null ? c.Country.Name : null
                }).ToList();
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
            entity.CityName = vm.CityName;
            entity.PostalCode = vm.PostalCode;
            entity.CountryId = vm.CountryId;

            await _cityRepository.UpdateAsync(entity);
            return await GetAll();
        }

        public async Task<List<CityVM>> Delete(int id)
        {
            try
            {
                Console.WriteLine($"CityService.Delete: Starting delete operation for city ID: {id}");

                // Check if city exists
                var city = await _cityRepository.GetByIdAsync(id);
                if (city == null)
                {
                    Console.WriteLine($"CityService.Delete: City with ID {id} not found");
                    throw new InvalidOperationException($"City with ID {id} not found.");
                }

                Console.WriteLine($"CityService.Delete: Found city: {city.CityName}");

                // Check if city has related employees
                Console.WriteLine($"CityService.Delete: Checking for related employees...");
                var hasEmployees = await _cityRepository.HasRelatedEmployeesAsync(id);
                Console.WriteLine($"CityService.Delete: Has related employees: {hasEmployees}");

                if (hasEmployees)
                {
                    Console.WriteLine($"CityService.Delete: Cannot delete city '{city.CityName}' - has related employees");
                    throw new InvalidOperationException($"Cannot delete city '{city.CityName}' because it has related employees. Please delete or reassign the employees first.");
                }

                // Delete the city
                Console.WriteLine($"CityService.Delete: Deleting city from repository...");
                await _cityRepository.DeleteAsync(id);
                Console.WriteLine($"CityService.Delete: City deleted from repository successfully");

                // Return updated list
                Console.WriteLine($"CityService.Delete: Getting updated city list...");
                var result = await GetAll();
                Console.WriteLine($"CityService.Delete: Retrieved {result.Count} cities");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CityService.Delete: Exception occurred: {ex.Message}");
                Console.WriteLine($"CityService.Delete: Stack trace: {ex.StackTrace}");

                // Log the error (you can implement proper logging here)
                var errorMessage = $"Failed to delete city with ID {id}. Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"CityService.Delete: Inner exception: {ex.InnerException.Message}");
                    errorMessage += $" Inner error: {ex.InnerException.Message}";
                }
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
    }
}
