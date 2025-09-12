using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

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

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            ViewBag.Countries = new SelectList(await _countryService.GetAll(), "CountryId", "Name");
            ViewBag.Cities = new SelectList(await _cityService.GetAll(), "CityId", "CityName");

            if (id == 0)
                return View(new EmployeeVM());

            var vm = await _employeeService.GetById(id);
            if (vm == null) return NotFound();
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
                if (id == 0)
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
            ViewBag.Cities = new SelectList(await _cityService.GetAll(), "CityId", "CityName");
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
                // Log the error (implement proper logging here if needed)
                return Json(new
                {
                    success = false,
                    message = $"Failed to delete employee: {ex.Message}"
                });
            }
        }
    }
}
