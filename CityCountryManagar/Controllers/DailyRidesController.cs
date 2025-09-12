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
