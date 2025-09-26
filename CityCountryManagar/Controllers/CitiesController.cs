using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using RidersApp.ViewModels;
using RidersApp.IServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CitiesController : Controller
    {
        private readonly ICityService _cityService;
        private readonly ICountryService _countryService;

        public CitiesController(ICityService cityService, ICountryService countryService)
        {
            _cityService = cityService;
            _countryService = countryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetCities()
        {
            var cities = await _cityService.GetAll();
            return PartialView("_ViewAll", cities);
        }

        /// <summary>
        /// API for dependent dropdown (returns cities of a country in JSON).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetCitiesByCountry(int countryId)
        {
            if (countryId <= 0)
                return Json(Enumerable.Empty<object>());

            // Fix: Cast to the correct type instead of object
            var cities = await _cityService.GetByCountry(countryId);
            var result = cities.Cast<dynamic>().Select(c => new
            {
                cityId = c.CityId,
                cityName = c.CityName
            });

            return Json(result);
        }

        // DataTables
        [HttpPost]
        public async Task<IActionResult> GetCitiesData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);
            string[] columnNames = { "CityName", "PostalCode", "CountryName" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : "CityName";

            var cities = await _cityService.GetAll();
            var query = cities.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.CityName ?? "").ToLower().Contains(lower) ||
                    (x.PostalCode ?? "").ToLower().Contains(lower) ||
                    (x.CountryName ?? "").ToLower().Contains(lower));
            }

            var recordsFiltered = query.Count();
            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            query = sortColumn switch
            {
                "CityName" => ascending ? query.OrderBy(x => x.CityName) : query.OrderByDescending(x => x.CityName),
                "PostalCode" => ascending ? query.OrderBy(x => x.PostalCode) : query.OrderByDescending(x => x.PostalCode),
                "CountryName" => ascending ? query.OrderBy(x => x.CountryName) : query.OrderByDescending(x => x.CountryName),
                _ => ascending ? query.OrderBy(x => x.CityName) : query.OrderByDescending(x => x.CityName)
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

        // Add or Edit
        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            ViewBag.Countries = new SelectList(await _countryService.GetAll(), "CountryId", "Name");

            if (id == 0)
                return View(new CityVM());

            var vm = await _cityService.GetById(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, CityVM vm)
        {
            ModelState.Remove("CountryName");
            if (ModelState.IsValid)
            {
                List<CityVM> cities;
                string message;

                if (vm.CityId == 0)
                {
                    cities = await _cityService.Add(vm);
                    message = "City added successfully";
                }
                else
                {
                    cities = await _cityService.Edit(vm);
                    message = "City updated successfully";
                }

                return Json(new
                {
                    isValid = true,
                    message,
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", cities)
                });
            }

            ViewBag.Countries = new SelectList(await _countryService.GetAll(), "CountryId", "Name");
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cities = await _cityService.Delete(id);
                return Json(new
                {
                    success = true,
                    message = "City deleted successfully",
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", cities)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = ex.Message
                });
            }
        }
    }
}
