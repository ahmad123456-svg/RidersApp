using Microsoft.AspNetCore.Mvc;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class FineOrExpenseTypesController : Controller
    {
        private readonly IFineOrExpenseTypeService _service;

        public FineOrExpenseTypesController(IFineOrExpenseTypeService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetAll()
        {
            var types = await _service.GetAll();
            return PartialView("_ViewAll", types);
        }

        [HttpPost]
        public async Task<IActionResult> GetFineOrExpenseTypesData()
        {
            var result = await _service.GetFineOrExpenseTypesData(Request.Form);
            return Json(result);
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new FineOrExpenseTypeVM());

            var vm = await _service.GetById(id);
            if (vm == null)
                return NotFound();

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, FineOrExpenseTypeVM vm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string message;
                    if (id == 0)
                    {
                        await _service.Add(vm);
                        message = "Fine/Expense Type added successfully";
                    }
                    else
                    {
                        vm.Id = id;
                        await _service.Edit(vm);
                        message = "Fine/Expense Type updated successfully";
                    }

                    return Json(new
                    {
                        isValid = true,
                        message = message
                    });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            return Json(new
            {
                isValid = false,
                html = Helper.RenderRazorViewToString(this, "AddOrEdit", vm)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _service.Delete(id);
                return Json(new
                {
                    success = true,
                    message = "Fine/Expense Type deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}