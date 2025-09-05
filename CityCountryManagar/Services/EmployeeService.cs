using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RidersApp.IServices;
using RidersApp.ViewModels;
using RidersApp.DbModels;
using Microsoft.EntityFrameworkCore;
using RidersApp.Interfaces;
using System;

namespace RidersApp.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICityRepository _cityRepository;
        private readonly ICountryRepository _countryRepository;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            ICityRepository cityRepository,
            ICountryRepository countryRepository)
        {
            _employeeRepository = employeeRepository;
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
        }

        public async Task<List<EmployeeVM>> GetAll()
        {
            var employees = await _employeeRepository.GetAllEmployees()
                .Include(e => e.Country)
                .Include(e => e.City)
                .ToListAsync();

            return employees
                .OrderBy(e => (e.Name ?? string.Empty).ToLowerInvariant())
                .Select(e => new EmployeeVM
                {
                    EmployeeId = e.EmployeeId,
                    Name = e.Name,
                    FatherName = e.FatherName,
                    PhoneNo = e.PhoneNo,
                    Address = e.Address,
                    CountryId = e.CountryId,
                    CityId = e.CityId,
                    CountryName = e.Country != null ? e.Country.Name : null,
                    CityName = e.City != null ? e.City.CityName : null
                }).ToList();
        }

        public async Task<EmployeeVM> GetById(int id)
        {
            var employee = await _employeeRepository.GetAllEmployees()
                .Include(e => e.Country)
                .Include(e => e.City)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);

            if (employee == null) return null;

            return new EmployeeVM
            {
                EmployeeId = employee.EmployeeId,
                Name = employee.Name,
                FatherName = employee.FatherName,
                PhoneNo = employee.PhoneNo,
                Address = employee.Address,
                CountryId = employee.CountryId,
                CityId = employee.CityId,
                CountryName = employee.Country != null ? employee.Country.Name : null,
                CityName = employee.City != null ? employee.City.CityName : null
            };
        }

        public async Task<List<EmployeeVM>> Add(EmployeeVM vm)
        {
            var employee = new Employee
            {
                Name = vm.Name,
                FatherName = vm.FatherName,
                PhoneNo = vm.PhoneNo,
                Address = vm.Address,
                CountryId = vm.CountryId,
                CityId = vm.CityId
            };

            await _employeeRepository.AddEmployee(employee);
            // Return employees ordered alphabetically
            return await GetAll();
        }

        public async Task<List<EmployeeVM>> Edit(EmployeeVM vm)
        {
            var employee = await _employeeRepository.GetEmployeeById(vm.EmployeeId);
            if (employee != null)
            {
                employee.Name = vm.Name;
                employee.FatherName = vm.FatherName;
                employee.PhoneNo = vm.PhoneNo;
                employee.Address = vm.Address;
                employee.CountryId = vm.CountryId;
                employee.CityId = vm.CityId;

                await _employeeRepository.UpdateEmployee(employee);
            }
            // Return employees ordered alphabetically
            return await GetAll();
        }

        public async Task<List<EmployeeVM>> Delete(int id)
        {
            try
            {
                // Simple delete without complex validation for now
                var employee = await _employeeRepository.GetEmployeeById(id);
                if (employee == null)
                {
                    throw new InvalidOperationException($"Employee with ID {id} not found.");
                }

                // Delete the employee
                await _employeeRepository.DeleteEmployee(id);

                // Return updated list
                var result = await GetAll();
                return result;
            }
            catch (Exception ex)
            {
                var errorMessage = $"Failed to delete employee with ID {id}. Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner error: {ex.InnerException.Message}";
                }
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
    }
}
