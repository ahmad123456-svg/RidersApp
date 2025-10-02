using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RidersApp.IServices;
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

        public EmployeesController(IEmployeeService employeeService, ICountryService countryService, ICityService cityService)
        {
            _employeeService = employeeService;
            _countryService = countryService;
            _cityService = cityService;
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
        public async Task<IActionResult> AddOrEdit(int id, EmployeeVM vm)
        {
            ModelState.Remove("CountryName");
            ModelState.Remove("CityName");

            if (ModelState.IsValid)
            {
                string message;
                var effectiveId = vm.EmployeeId != 0 ? vm.EmployeeId : id;
                if (effectiveId == 0)
                {
                    await _employeeService.Add(vm);
                    message = "Data saved successfully";
                }
                else
                {
                    await _employeeService.Edit(vm);
                    message = "Data updated successfully";
                }

                var listVm = await _employeeService.GetAll();
                return Json(new
                {
                    isValid = true,
                    message,
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", listVm)
                });
            }

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
