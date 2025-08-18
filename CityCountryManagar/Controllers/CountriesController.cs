using Microsoft.AspNetCore.Mvc;
using RidersApp.ViewModels;
using RidersApp.Interfaces; // Make sure ICountryService exists here
using System.Threading.Tasks;
using System.Collections.Generic;
using RidersApp.IServices;
using System;

namespace RidersApp.Controllers
{
    public class CountriesController : Controller
    {
        private readonly ICountryService _countryService;

        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetCountries()
        {
            var vm = await _countryService.GetAll();
            return PartialView("_ViewAll", vm);
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
           
            if (id == 0)
                return View(new CountryVM());

            var vm = await _countryService.GetById(id);
            if (vm == null) return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, CountryVM vm)
        {
            ModelState.Remove("Cities");
            if (ModelState.IsValid)
            {
                List<CountryVM> countries;
                string message;

                if (id == 0)
                {
                    countries = await _countryService.Add(vm);
                    message = "Data saved successfully";
                }
                else
                {
                    countries = await _countryService.Edit(vm);
                    message = "Data updated successfully";
                }

                return Json(new
                {
                    isValid = true,
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", countries),
                    message
                });
            }

            return Json(new
            {
                isValid = false,
                html = Helper.RenderRazorViewToString(this, "AddOrEdit", vm)
            });
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Simple delete without complex validation for now
                var country = await _countryService.GetById(id);
                if (country == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Country with ID {id} not found."
                    });
                }
                
                // Direct delete from repository
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
            catch (Exception ex)
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
