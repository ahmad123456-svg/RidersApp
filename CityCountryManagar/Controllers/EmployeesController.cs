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

        [HttpPost]
        public async Task<IActionResult> GetEmployeesData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            int sortColumnIndex = 0;
            int.TryParse(sortColumnIndexString, out sortColumnIndex);

            string[] columnNames = new[] { "Name", "FatherName", "PhoneNo", "Address", "CountryName", "CityName", "Salary", "Vehicle", "VehicleNumber" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var all = await _employeeService.GetAll();
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

            var pageData = query.Skip(start).Take(length).ToList();

            return Json(new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = pageData
            });
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
                // Prefer model ID; fall back to route id
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
