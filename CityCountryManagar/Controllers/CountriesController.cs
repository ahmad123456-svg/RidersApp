using Microsoft.AspNetCore.Mvc;
using RidersApp.ViewModels;
using RidersApp.Interfaces; // Make sure ICountryService exists here
using System.Threading.Tasks;
using System.Collections.Generic;
using RidersApp.IServices;
using System;
using Microsoft.AspNetCore.Authorization;

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

            int sortColumnIndex = 0;
            int.TryParse(sortColumnIndexString, out sortColumnIndex);

            string[] columnNames = new[] { "Name" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var all = await _countryService.GetAll();
            var query = all.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x => (x.Name ?? string.Empty).ToLower().Contains(lower));
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
                List<CountryVM> countries;

                if (id == 0)
                {
                    await _countryService.Add(vm);
                    message = "Country added successfully";
                }
                else
                {
                    await _countryService.Edit(vm);
                    message = "Country updated successfully";
                }

                countries = await _countryService.GetAll();
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
                var country = await _countryService.GetById(id);
                if (country == null)
                {
                    return Json(new { success = false, message = "Country not found" });
                }

                await _countryService.Delete(id);
                
                // Get updated list
                var countries = await _countryService.GetAll();
                
                return Json(new
                {
                    success = true,
                    message = "Country deleted successfully",
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", countries)
                });
            }
            catch (System.Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Failed to delete country: {ex.Message}"
                });
            }
        }
    }
}
