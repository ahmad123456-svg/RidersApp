using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "User,Admin")]
    public class ConfigurationsController : Controller
    {
        private readonly IConfigurationService _service;

        public ConfigurationsController(IConfigurationService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {
            return View();
        }

        public async Task<IActionResult> AddOrEdit(int id = 0)
        {
            if (id == 0)
                return View(new ConfigurationVM());

            var vm = await _service.GetById(id);
            if (vm == null) return NotFound();
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> GetConfigurationsData()
        {
            var result = await _service.GetConfigurationsData(Request.Form);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, ConfigurationVM vm)
        {
            if (ModelState.IsValid)
            {
                var effectiveId = vm.ConfigurationId != 0 ? vm.ConfigurationId : id;
                if (effectiveId == 0)
                {
                    await _service.Add(vm);
                    var all = await _service.GetAll();
                    return Json(new
                    {
                        isValid = true,
                        message = "Configuration added successfully",
                        html = Helper.RenderRazorViewToString(this, "_ViewAll", all)
                    });
                }
                else
                {
                    await _service.Edit(vm);
                    var all = await _service.GetAll();
                    return Json(new
                    {
                        isValid = true,
                        message = "Configuration updated successfully",
                        html = Helper.RenderRazorViewToString(this, "_ViewAll", all)
                    });
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
                var config = await _service.GetById(id);
                if (config == null)
                {
                    return Json(new { success = false, message = "Configuration not found" });
                }

                await _service.Delete(id);
                
                // Get updated list
                var configurations = await _service.GetAll();
                
                return Json(new
                {
                    success = true,
                    message = "Configuration deleted successfully",
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", configurations)
                });
            }
            catch (System.Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Failed to delete configuration: {ex.Message}"
                });
            }
        }
    }
}

