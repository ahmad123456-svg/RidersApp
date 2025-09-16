using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RidersApp.Data;
using RidersApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using RidersApp.DbModels;
using RidersApp.ViewModels;
using RidersApp.IServices;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CitiesController : Controller
    {
        private readonly ICityRepository _cityRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityService _cityService;
        private readonly ApplicationDbContext _context;

        public CitiesController(
            ICityRepository cityRepository,
            ICountryRepository countryRepository,
            ICityService cityService,
            ApplicationDbContext context)
        {
            _cityRepository = cityRepository;
            _countryRepository = countryRepository;
            _cityService = cityService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> GetCities()
        {
            var cities = await _cityRepository.GetAllAsync();
            var vm = cities.Select(c => new CityVM
            {
                CityId = c.CityId,
                CityName = c.CityName,
                PostalCode = c.PostalCode,
                CountryId = c.CountryId,
                CountryName = c.Country != null ? c.Country.Name : null
            }).ToList();

            return PartialView("_ViewAll", vm);
        }

        [HttpPost]
        public async Task<IActionResult> GetCitiesData()
        {
            // DataTables parameters
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault(); // asc/desc

            int sortColumnIndex = 0;
            int.TryParse(sortColumnIndexString, out sortColumnIndex);

            // Column mapping in the table order
            string[] columnNames = new[] { "CityName", "PostalCode", "CountryName" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var query = (await _cityRepository.GetAllAsync()).AsQueryable();

            // Project to VM for consistent sorting/searching
            var dataQuery = query.Select(c => new CityVM
            {
                CityId = c.CityId,
                CityName = c.CityName,
                PostalCode = c.PostalCode,
                CountryId = c.CountryId,
                CountryName = c.Country != null ? c.Country.Name : null
            });

            var recordsTotal = dataQuery.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                dataQuery = dataQuery.Where(x =>
                    (x.CityName ?? string.Empty).ToLower().Contains(lower) ||
                    (x.PostalCode ?? string.Empty).ToLower().Contains(lower) ||
                    (x.CountryName ?? string.Empty).ToLower().Contains(lower)
                );
            }

            var recordsFiltered = dataQuery.Count();

            // Sorting
            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            dataQuery = sortColumn switch
            {
                "CityName" => ascending ? dataQuery.OrderBy(x => x.CityName) : dataQuery.OrderByDescending(x => x.CityName),
                "PostalCode" => ascending ? dataQuery.OrderBy(x => x.PostalCode) : dataQuery.OrderByDescending(x => x.PostalCode),
                "CountryName" => ascending ? dataQuery.OrderBy(x => x.CountryName) : dataQuery.OrderByDescending(x => x.CountryName),
                _ => ascending ? dataQuery.OrderBy(x => x.CityName) : dataQuery.OrderByDescending(x => x.CityName)
            };

            var pageData = dataQuery
                .Skip(start)
                .Take(length)
                .ToList();

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
            ViewBag.Countries = new SelectList(await _countryRepository.GetAllAsync(), "CountryId", "Name");

            if (id == 0)
                return View(new CityVM());

            var city = await _cityRepository.GetByIdAsync(id);
            if (city == null) return NotFound();

            var vm = new CityVM
            {
                CityId = city.CityId,
                CityName = city.CityName,
                PostalCode = city.PostalCode,
                CountryId = city.CountryId,
                CountryName = city.Country?.Name
            };

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

                if (id == 0)
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

            // Repopulate dropdown on error
            ViewBag.Countries = new SelectList(await _countryRepository.GetAllAsync(), "CountryId", "Name");
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var city = await _context.Cities
                    .Include(c => c.Employees) // check if employees exist
                    .FirstOrDefaultAsync(c => c.CityId == id);

                if (city == null)
                    return Json(new { success = false, message = "City not found" });

                if (city.Employees != null && city.Employees.Any())
                {
                    return Json(new
                    {
                        success = false,
                        message = "Cannot delete city because employees are linked to it."
                    });
                }

                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();

                // reload updated city list
                var cities = await _context.Cities.Include(c => c.Country).ToListAsync();
                var vm = cities.Select(c => new CityVM
                {
                    CityId = c.CityId,
                    CityName = c.CityName,
                    PostalCode = c.PostalCode,
                    CountryId = c.CountryId,
                    CountryName = c.Country?.Name
                }).ToList();

                return Json(new
                {
                    success = true,
                    message = "City deleted successfully",
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", vm)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Failed to delete city: {ex.InnerException?.Message ?? ex.Message}"
                });
            }
        }
    }
}
