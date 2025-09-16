using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RidersApp.Data;
using RidersApp.Interfaces;
using Microsoft.AspNetCore.Authorization;
using RidersApp.DbModels;
using RidersApp.ViewModels;
using RidersApp.IServices;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class DailyRidesController : Controller
    {
        private readonly IDailyRidesService _dailyRidesService;
        private readonly IEmployeeService _employeeService;

        public DailyRidesController(IDailyRidesService dailyRidesService, IEmployeeService employeeService)
        {
            _dailyRidesService = dailyRidesService;
            _employeeService = employeeService;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> GetDailyRides()
        {
            var dailyRidesVM = await _dailyRidesService.GetAll();
            return PartialView("_ViewAll", dailyRidesVM);
        }

        [HttpPost]
        public async Task<IActionResult> GetDailyRidesData()
        {
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            int sortColumnIndex = 0;
            int.TryParse(sortColumnIndexString, out sortColumnIndex);

            string[] columnNames = new[] { "EmployeeName", "EntryDate", "CreditAmount", "CreditWAT", "CashAmount", "CashWAT", "Expense", "TodayRides", "TotalRides" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var all = await _dailyRidesService.GetAll();
            var query = all.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.EmployeeName ?? string.Empty).ToLower().Contains(lower)
                    || x.EntryDate.ToString("yyyy-MM-dd").Contains(lower)
                    || x.TodayRides.ToString().Contains(lower)
                    || x.TotalRides.ToString().Contains(lower)
                );
            }

            var recordsFiltered = query.Count();

            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = sortColumn switch
            {
                "EmployeeName" => ascending ? query.OrderBy(x => x.EmployeeName) : query.OrderByDescending(x => x.EmployeeName),
                "EntryDate" => ascending ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate),
                "CreditAmount" => ascending ? query.OrderBy(x => x.CreditAmount) : query.OrderByDescending(x => x.CreditAmount),
                "CreditWAT" => ascending ? query.OrderBy(x => x.CreditWAT) : query.OrderByDescending(x => x.CreditWAT),
                "CashAmount" => ascending ? query.OrderBy(x => x.CashAmount) : query.OrderByDescending(x => x.CashAmount),
                "CashWAT" => ascending ? query.OrderBy(x => x.CashWAT) : query.OrderByDescending(x => x.CashWAT),
                "Expense" => ascending ? query.OrderBy(x => x.Expense) : query.OrderByDescending(x => x.Expense),
                "TodayRides" => ascending ? query.OrderBy(x => x.TodayRides) : query.OrderByDescending(x => x.TodayRides),
                "TotalRides" => ascending ? query.OrderBy(x => x.TotalRides) : query.OrderByDescending(x => x.TotalRides),
                _ => ascending ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate)
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
            var employees = await _employeeService.GetAll();
            ViewBag.Employees = new SelectList(employees, "EmployeeId", "Name");

            if (id == 0)
            {
                var newRide = new DailyRidesVM
                {
                    EntryDate = DateTime.Now.Date // Set current date for new records
                };
                return View(newRide);
            }

            var dailyRideVM = await _dailyRidesService.GetById(id);
            if (dailyRideVM == null) return NotFound();

            return View(dailyRideVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, DailyRidesVM dailyRideVM)
        {
            ModelState.Remove("EmployeeName");
            ModelState.Remove("UpdateDate");
            ModelState.Remove("InsertedBy");
            ModelState.Remove("UpdatedBy");
            
            // Ensure EntryDate has both date and time components
            if (dailyRideVM.EntryDate.TimeOfDay.TotalSeconds == 0)
            {
                dailyRideVM.EntryDate = dailyRideVM.EntryDate.Date.Add(DateTime.Now.TimeOfDay);
            }
            if (ModelState.IsValid)
            {
                string message;
                List<DailyRidesVM> allDailyRides;

                if (id == 0)
                {
                    allDailyRides = await _dailyRidesService.Add(dailyRideVM);
                    message = "Data saved successfully";
                }
                else
                {
                    allDailyRides = await _dailyRidesService.Edit(dailyRideVM);
                    message = "Data updated successfully";
                }

                return Json(new { isValid = true, message, html = Helper.RenderRazorViewToString(this, "_ViewAll", allDailyRides) });
            }

            var employees = await _employeeService.GetAll();
            ViewBag.Employees = new SelectList(employees, "EmployeeId", "Name");
            return PartialView(dailyRideVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Simple delete without complex validation for now
                var dailyRide = await _dailyRidesService.GetById(id);
                if (dailyRide == null)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"Daily ride record with ID {id} not found."
                    });
                }

                // Direct delete from repository
                await _dailyRidesService.Delete(id);

                // Get updated list
                var dailyRides = await _dailyRidesService.GetAll();

                return Json(new
                {
                    success = true,
                    message = "Daily ride record deleted successfully",
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", dailyRides)
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Failed to delete daily ride record: {ex.Message}"
                });
            }
        }
    }
}
