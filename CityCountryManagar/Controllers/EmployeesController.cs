using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RidersApp.IServices;
using RidersApp.Services;
using RidersApp.ViewModels;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeService _employeeService;
        private readonly ICountryService _countryService;
        private readonly ICityService _cityService;
        private readonly FileService _fileService;

        public EmployeesController(
            IEmployeeService employeeService, 
            ICountryService countryService, 
            ICityService cityService,
            FileService fileService)
        {
            _employeeService = employeeService;
            _countryService = countryService;
            _cityService = cityService;
            _fileService = fileService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetEmployees()
        {
            var vm = await _employeeService.GetAll();
            return PartialView("_ViewAll", vm);
        }

        [HttpPost]
        public async Task<IActionResult> GetEmployeesData()
        {
            var result = await _employeeService.GetEmployeesData(Request.Form);
            return Json(result);
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            ViewBag.Countries = new SelectList(await _countryService.GetAll(), "CountryId", "Name");

            if (id == 0)
            {
                // For Add: do not populate cities so the city dropdown will be disabled on the client
                ViewBag.Cities = new SelectList(Enumerable.Empty<object>(), "CityId", "CityName");
                return View(new EmployeeVM());
            }

            var vm = await _employeeService.GetById(id);
            if (vm == null) return NotFound();

            // For Edit: populate cities for the employee's country so the select shows proper options
            if (vm.CountryId > 0)
            {
                var cities = await _cityService.GetByCountry(vm.CountryId);
                ViewBag.Cities = new SelectList(cities, "CityId", "CityName", vm.CityId);
            }
            else
            {
                ViewBag.Cities = new SelectList(Enumerable.Empty<object>(), "CityId", "CityName");
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
        [RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
        public async Task<IActionResult> AddOrEdit(int id, EmployeeVM vm)
        {
            // Remove validation for properties that aren't user input or are handled separately
            ModelState.Remove("CountryName");
            ModelState.Remove("CityName");
            ModelState.Remove("PictureFile"); // We'll handle file validation in service
            
            // For new employees, remove PictureUrl validation since we'll set it after upload
            if (vm.EmployeeId == 0 && id == 0)
            {
                ModelState.Remove("PictureUrl");
            }

            if (ModelState.IsValid)
            {
                var (isValid, message, employees) = await _employeeService.AddOrEditEmployee(id, vm);
                
                if (isValid)
                {
                    return Json(new
                    {
                        isValid = true,
                        message,
                        html = Helper.RenderRazorViewToString(this, "_ViewAll", employees)
                    });
                }
                else
                {
                    // If service returned an error, treat as validation failure
                    ModelState.AddModelError("", message);
                }
            }

            // Handle validation errors
            ViewBag.Countries = new SelectList(await _countryService.GetAll(), "CountryId", "Name");

            // When validation fails, populate city list only for the selected country (if any)
            if (vm.CountryId > 0)
            {
                var cities = await _cityService.GetByCountry(vm.CountryId);
                ViewBag.Cities = new SelectList(cities, "CityId", "CityName", vm.CityId);
            }
            else
            {
                ViewBag.Cities = new SelectList(Enumerable.Empty<object>(), "CityId", "CityName");
            }

            return Json(new
            {
                isValid = false,
                html = Helper.RenderRazorViewToString(this, "AddOrEdit", vm)
            });
        }

        [HttpPost]
        public async Task<IActionResult> TestFileUpload(IFormFile testFile)
        {
            Console.WriteLine($"TestFileUpload: Received file: {(testFile != null ? $"{testFile.FileName} ({testFile.Length} bytes)" : "null")}");
            
            if (testFile != null && testFile.Length > 0)
            {
                try
                {
                    var uploadedPath = await _fileService.UploadEmployeeImage(testFile);
                    return Json(new { success = true, path = uploadedPath, message = $"File uploaded successfully to: {uploadedPath}" });
                }
                catch (Exception ex)
                {
                    return Json(new { success = false, message = $"Upload failed: {ex.Message}" });
                }
            }
            
            return Json(new { success = false, message = "No file provided" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var listVm = await _employeeService.Delete(id);

                return Json(new
                {
                    success = true,
                    message = "Employee deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Failed to delete employee: {ex.Message}"
                });
            }
        }
    }
}
