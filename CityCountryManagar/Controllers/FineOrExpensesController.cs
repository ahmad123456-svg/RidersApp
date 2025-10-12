using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FineOrExpensesController : Controller
    {
        private readonly IFineOrExpenseService _service;
        private readonly IEmployeeService _employeeService;
        private readonly IFineOrExpenseTypeService _fineOrExpenseTypeService;

        public FineOrExpensesController(
            IFineOrExpenseService service,
            IEmployeeService employeeService,
            IFineOrExpenseTypeService fineOrExpenseTypeService)
        {
            _service = service;
            _employeeService = employeeService;
            _fineOrExpenseTypeService = fineOrExpenseTypeService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetAll()
        {
            var fineOrExpenses = await _service.GetAll();
            return PartialView("_ViewAll", fineOrExpenses);
        }

        [HttpPost]
        public async Task<IActionResult> GetFineOrExpensesData()
        {
            var result = await _service.GetFineOrExpensesData(Request.Form);
            return Json(result);
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            // Populate dropdowns
            ViewBag.Employees = new SelectList(await _employeeService.GetAll(), "EmployeeId", "Name");
            ViewBag.FineOrExpenseTypes = new SelectList(await _fineOrExpenseTypeService.GetAll(), "Id", "Name");

            if (id == 0)
            {
                var vm = new FineOrExpenseVM
                {
                    EntryDate = DateTime.Now
                };
                return PartialView(vm);
            }

            var existingVm = await _service.GetById(id);
            if (existingVm == null)
                return NotFound();

            return PartialView(existingVm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, FineOrExpenseVM vm)
        {
            try
            {
                // Perform operation based on whether it's an add or edit
                if (id != 0)
                {
                    vm.Id = id;
                }
                var result = id == 0 ? 
                    await _service.Add(vm) : 
                    await _service.Edit(vm);

                if (result.success)
                {
                    return Json(new { isValid = true, message = result.message });
                }

                // If validation failed, reload dropdowns and return the form
                ViewBag.Employees = new SelectList(await _employeeService.GetAll(), "EmployeeId", "Name", vm.EmployeeId);
                ViewBag.FineOrExpenseTypes = new SelectList(await _fineOrExpenseTypeService.GetAll(), "Id", "Name", vm.FineOrExpenseTypeId);

                return Json(new
                {
                    isValid = false,
                    message = result.message,
                    html = Helper.RenderRazorViewToString(this, "AddOrEdit", vm)
                });
            }
            catch (Exception ex)
            {
                // Handle unexpected errors
                ViewBag.Employees = new SelectList(await _employeeService.GetAll(), "EmployeeId", "Name", vm.EmployeeId);
                ViewBag.FineOrExpenseTypes = new SelectList(await _fineOrExpenseTypeService.GetAll(), "Id", "Name", vm.FineOrExpenseTypeId);

                return Json(new
                {
                    isValid = false,
                    message = $"An unexpected error occurred: {ex.Message}",
                    html = Helper.RenderRazorViewToString(this, "AddOrEdit", vm)
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.Delete(id);
            return Json(new { success = result.success, message = result.message });
        }

        private async Task<bool> ValidateModel(FineOrExpenseVM vm)
        {
            bool isValid = true;
            System.Diagnostics.Debug.WriteLine("Starting ValidateModel...");

            // Amount validation - minimum 200, maximum 20000
            if (vm.Amount < 200)
            {
                ModelState.AddModelError("Amount", "Amount must be at least Rs 200");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Amount validation failed: Amount < 200");
            }
            if (vm.Amount > 20000)
            {
                ModelState.AddModelError("Amount", "Amount cannot exceed Rs 20,000");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Amount validation failed: Amount > 20000");
            }

            // Employee validation
            if (vm.EmployeeId <= 0)
            {
                ModelState.AddModelError("EmployeeId", "Please select an employee");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Employee validation failed: EmployeeId <= 0");
            }
            else
            {
                try
                {
                    var employee = await _employeeService.GetById(vm.EmployeeId);
                    if (employee == null)
                    {
                        ModelState.AddModelError("EmployeeId", "Selected employee does not exist");
                        isValid = false;
                        System.Diagnostics.Debug.WriteLine($"Employee validation failed: Employee ID {vm.EmployeeId} not found");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"Employee validation passed: Found employee {employee.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Employee validation error: {ex.Message}");
                    ModelState.AddModelError("EmployeeId", "Error validating employee");
                    isValid = false;
                }
            }

            // FineOrExpenseType validation
            if (vm.FineOrExpenseTypeId <= 0)
            {
                ModelState.AddModelError("FineOrExpenseTypeId", "Please select a fine/expense type");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("FineOrExpenseType validation failed: FineOrExpenseTypeId <= 0");
            }
            else
            {
                try
                {
                    var type = await _fineOrExpenseTypeService.GetById(vm.FineOrExpenseTypeId);
                    if (type == null)
                    {
                        ModelState.AddModelError("FineOrExpenseTypeId", "Selected fine/expense type does not exist");
                        isValid = false;
                        System.Diagnostics.Debug.WriteLine($"FineOrExpenseType validation failed: Type ID {vm.FineOrExpenseTypeId} not found");
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"FineOrExpenseType validation passed: Found type {type.Name}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"FineOrExpenseType validation error: {ex.Message}");
                    ModelState.AddModelError("FineOrExpenseTypeId", "Error validating fine/expense type");
                    isValid = false;
                }
            }

            // Description validation
            if (string.IsNullOrWhiteSpace(vm.Description))
            {
                ModelState.AddModelError("Description", "Description is required");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("Description validation failed: Description is null or empty");
            }
            else if (vm.Description.Length < 3)
            {
                ModelState.AddModelError("Description", "Description must be at least 3 characters long");
                isValid = false;
                System.Diagnostics.Debug.WriteLine($"Description validation failed: Length {vm.Description.Length} < 3");
            }
            else if (vm.Description.Length > 500)
            {
                ModelState.AddModelError("Description", "Description cannot exceed 500 characters");
                isValid = false;
                System.Diagnostics.Debug.WriteLine($"Description validation failed: Length {vm.Description.Length} > 500");
            }

            // Entry Date validation
            if (vm.EntryDate == default(DateTime))
            {
                ModelState.AddModelError("EntryDate", "Entry date is required");
                isValid = false;
                System.Diagnostics.Debug.WriteLine("EntryDate validation failed: EntryDate is default");
            }
            else
            {
                var oneYearAgo = DateTime.Now.AddYears(-1);
                var oneYearFromNow = DateTime.Now.AddYears(1);
                
                if (vm.EntryDate < oneYearAgo || vm.EntryDate > oneYearFromNow)
                {
                    ModelState.AddModelError("EntryDate", "Entry date must be within the last year or next year");
                    isValid = false;
                    System.Diagnostics.Debug.WriteLine($"EntryDate validation failed: Date {vm.EntryDate} out of range");
                }
            }

            System.Diagnostics.Debug.WriteLine($"ValidateModel completed. IsValid: {isValid}");
            return isValid;
        }

        private async Task<bool> ValidateBusinessRules(FineOrExpenseVM vm)
        {
            bool isValid = true;

            try
            {
                // No business rules currently enforced
                // All validation is done in ValidateModel method
            }
            catch (Exception ex)
            {
                // Log the error but don't fail validation
                System.Diagnostics.Debug.WriteLine($"Business rule validation error: {ex.Message}");
            }

            return isValid;
        }
    }
}