using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RidersApp.IServices;
using RidersApp.ViewModels;
using RidersApp.DbModels;
using Microsoft.EntityFrameworkCore;
using RidersApp.Interfaces;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace RidersApp.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ICityRepository _cityRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmployeeService(
            IEmployeeRepository employeeRepository,
            ICityRepository cityRepository,
            ICountryRepository countryRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _employeeRepository = employeeRepository;
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
            _webHostEnvironment = webHostEnvironment;
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
                    CityName = e.City != null ? e.City.CityName : null,
                    Vehicle = e.Vehicle,
                    VehicleNumber = e.VehicleNumber,
                    Salary = e.Salary,
                    Picture = !string.IsNullOrEmpty(e.Picture) ? e.Picture : "/Image/download.png" // Always return a picture
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
                CityName = employee.City != null ? employee.City.CityName : null,
                Vehicle = employee.Vehicle,
                VehicleNumber = employee.VehicleNumber,
                Salary = employee.Salary,
                Picture = !string.IsNullOrEmpty(employee.Picture) ? employee.Picture : "/Image/download.png" // Always return a picture
            };
        }

        // Helper method to upload picture file
        private async Task<string?> UploadPictureAsync(IFormFile? pictureFile)
        {
            if (pictureFile == null || pictureFile.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var fileExtension = Path.GetExtension(pictureFile.FileName).ToLowerInvariant();
            
            if (!allowedExtensions.Contains(fileExtension))
                throw new ArgumentException("Invalid file type. Only JPG, PNG, and GIF files are allowed.");

            // Validate file size (5MB max)
            if (pictureFile.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size cannot exceed 5MB.");

            // Create uploads directory if it doesn't exist
            var uploadsPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "employees");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            // Save the file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pictureFile.CopyToAsync(stream);
            }

            // Return the relative path for database storage
            return $"/uploads/employees/{fileName}";
        }

        public async Task<List<EmployeeVM>> Add(EmployeeVM vm)
        {
            // Handle picture upload
            var picturePath = await UploadPictureAsync(vm.PictureFile);
            
            // If no picture was uploaded, set default picture
            if (string.IsNullOrEmpty(picturePath))
            {
                picturePath = "/Image/download.png";
            }

            var employee = new Employee
            {
                Name = vm.Name,
                FatherName = vm.FatherName,
                PhoneNo = vm.PhoneNo,
                Address = vm.Address,
                CountryId = vm.CountryId,
                CityId = vm.CityId,
                Vehicle = vm.Vehicle,
                VehicleNumber = vm.VehicleNumber,
                Salary = vm.Salary,
                Picture = picturePath // Always has a value (uploaded or default)
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
                employee.Vehicle = vm.Vehicle;
                employee.VehicleNumber = vm.VehicleNumber;
                employee.Salary = vm.Salary;

                // Handle picture upload - only update if new file is provided
                if (vm.PictureFile != null)
                {
                    // Delete old picture if it exists and is not the default
                    if (!string.IsNullOrEmpty(employee.Picture) && employee.Picture != "/Image/download.png")
                    {
                        var oldPicturePath = Path.Combine(_webHostEnvironment.WebRootPath, employee.Picture.TrimStart('/'));
                        if (File.Exists(oldPicturePath))
                        {
                            File.Delete(oldPicturePath);
                        }
                    }

                    // Upload new picture
                    employee.Picture = await UploadPictureAsync(vm.PictureFile);
                }

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

                // Delete uploaded picture file (if it exists and is not the default)
                if (!string.IsNullOrEmpty(employee.Picture) && employee.Picture != "/Image/download.png")
                {
                    var picturePath = Path.Combine(_webHostEnvironment.WebRootPath, employee.Picture.TrimStart('/'));
                    if (File.Exists(picturePath))
                    {
                        File.Delete(picturePath);
                    }
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

        // DataTables logic moved from controller
        public async Task<object> GetEmployeesData(IFormCollection form)
        {
            var draw = form["draw"].FirstOrDefault();
            var start = int.TryParse(form["start"].FirstOrDefault(), out int s) ? s : 0;
            var length = int.TryParse(form["length"].FirstOrDefault(), out int l) ? l : 10;
            var searchValue = form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = form["order[0][column]"].FirstOrDefault();
            var sortDirection = form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);
            string[] columnNames = new[] { "Picture", "Name", "FatherName", "PhoneNo", "Address", "CountryName", "CityName", "Salary", "Vehicle", "VehicleNumber" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[1]; // Default to "Name" since Picture is not sortable

            var all = await GetAll();
            var query = all.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.Name ?? string.Empty).ToLower().Contains(lower) ||
                    (x.FatherName ?? string.Empty).ToLower().Contains(lower) ||
                    (x.PhoneNo ?? string.Empty).ToLower().Contains(lower) ||
                    (x.Address ?? string.Empty).ToLower().Contains(lower) ||
                    (x.CountryName ?? string.Empty).ToLower().Contains(lower) ||
                    (x.CityName ?? string.Empty).ToLower().Contains(lower) ||
                    (x.Vehicle ?? string.Empty).ToLower().Contains(lower) ||
                    (x.VehicleNumber ?? string.Empty).ToLower().Contains(lower)
                );
            }

            var recordsFiltered = query.Count();

            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = sortColumn switch
            {
                "Name" => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                "FatherName" => ascending ? query.OrderBy(x => x.FatherName) : query.OrderByDescending(x => x.FatherName),
                "PhoneNo" => ascending ? query.OrderBy(x => x.PhoneNo) : query.OrderByDescending(x => x.PhoneNo),
                "Address" => ascending ? query.OrderBy(x => x.Address) : query.OrderByDescending(x => x.Address),
                "CountryName" => ascending ? query.OrderBy(x => x.CountryName) : query.OrderByDescending(x => x.CountryName),
                "CityName" => ascending ? query.OrderBy(x => x.CityName) : query.OrderByDescending(x => x.CityName),
                "Salary" => ascending ? query.OrderBy(x => x.Salary) : query.OrderByDescending(x => x.Salary),
                "Vehicle" => ascending ? query.OrderBy(x => x.Vehicle) : query.OrderByDescending(x => x.Vehicle),
                "VehicleNumber" => ascending ? query.OrderBy(x => x.VehicleNumber) : query.OrderByDescending(x => x.VehicleNumber),
                _ => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name)
            };

            var pageData = query.Skip(start).Take(length).Select(x => new
            {
                picture = x.Picture ?? "/Image/download.png",
                name = x.Name,
                fatherName = x.FatherName,
                phoneNo = x.PhoneNo,
                address = x.Address,
                countryName = x.CountryName,
                cityName = x.CityName,
                salary = x.Salary.ToString("C"),
                vehicle = x.Vehicle,
                vehicleNumber = x.VehicleNumber,
                employeeId = x.EmployeeId
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
