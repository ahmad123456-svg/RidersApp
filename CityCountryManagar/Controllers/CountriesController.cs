using Microsoft.AspNetCore.Mvc;
using RidersApp.ViewModels;
using RidersApp.IServices;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CountriesController : Controller
    {
        private readonly ICountryService _countryService;

        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> GetCountries()
        {
            var countries = await _countryService.GetAll();
            return PartialView("_ViewAll", countries);
        }

        [HttpPost]
        public async Task<IActionResult> GetCountriesData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);

            string[] columnNames = new[] { "Name" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var all = await _countryService.GetAll();
            var query = all.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                query = query.Where(x => (x.Name ?? string.Empty).ToLower().Contains(searchValue.ToLower()));
            }

            var recordsFiltered = query.Count();

            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = sortColumn switch
            {
                "Name" => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
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
            if (id == 0)
                return PartialView(new CountryVM());

            var vm = await _countryService.GetById(id);
            if (vm == null) return NotFound();

            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, CountryVM vm)
        {
            ModelState.Remove("Cities");
            if (ModelState.IsValid)
            {
                string message;
                var effectiveId = vm.CountryId != 0 ? vm.CountryId : id;
                if (effectiveId == 0)
                {
                    await _countryService.Add(vm);
                    message = "Country added successfully";
                }
                else
                {
                    await _countryService.Edit(vm);
                    message = "Country updated successfully";
                }

                var countries = await _countryService.GetAll();
                return Json(new
                {
                    isValid = true,
                    message,
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", countries)
                });
            }
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _countryService.Delete(id);
                var countries = await _countryService.GetAll();

                return Json(new
                {
                    success = true,
                    message = "Country deleted successfully",
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", countries)
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
