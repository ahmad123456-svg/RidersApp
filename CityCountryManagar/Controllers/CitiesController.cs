using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using RidersApp.ViewModels;
using RidersApp.IServices;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;
using Microsoft.AspNetCore.Http;

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

            var cities = await _cityService.GetByCountry(countryId);
            var result = cities.Cast<dynamic>().Select(c => new
            {
                cityId = c.CityId,
                cityName = c.CityName
            });

            return Json(result);
        }

        // DataTables - delegate processing to service
        [HttpPost]
        public async Task<IActionResult> GetCitiesData()
        {
            var result = await _cityService.GetCitiesData(Request.Form);
            return Json(result);
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
