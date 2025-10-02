using Microsoft.AspNetCore.Mvc;
using RidersApp.ViewModels;
using RidersApp.IServices;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using Microsoft.AspNetCore.Http;

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
            var result = await _countryService.GetCountriesData(Request.Form);
            return Json(result);
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
