using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using RidersApp.IServices;
using RidersApp.ViewModels;
using RidersApp.DbModels;
using System.Linq;

namespace RidersApp.Controllers
{
    public class ConfigurationsController : Controller
    {
        private readonly IConfigurationService _service;

        public ConfigurationsController(IConfigurationService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> GetConfigurations()
        {
            var vm = await _service.GetAll();
            return PartialView("_ViewAll", vm);
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(int id, ConfigurationVM vm)
        {
            if (ModelState.IsValid)
            {
                List<ConfigurationVM> items;
                string message;

                if (id == 0)
                {
                    items = await _service.Add(vm);
                    message = "Data saved successfully";
                }
                else
                {
                    items = await _service.Edit(vm);
                    message = "Data updated successfully";
                }

                return Json(new
                {
                    isValid = true,
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", items),
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
                var config = await _service.GetById(id);
                if (config == null)
                {
                    // Return the current list so client can refresh
                    var current = await _service.GetAll();
                    return Json(new { success = false, message = $"Configuration with ID {id} not found.", html = Helper.RenderRazorViewToString(this, "_ViewAll", current) });
                }

                var items = await _service.Delete(id);

                return Json(new
                {
                    success = true,
                    message = "Configuration deleted successfully",
                    html = Helper.RenderRazorViewToString(this, "_ViewAll", items)
                });
            }
            catch (System.Exception ex)
            {
                var current = await _service.GetAll();
                return Json(new { success = false, message = $"Failed to delete: {ex.Message}", html = Helper.RenderRazorViewToString(this, "_ViewAll", current) });
            }
        }
    }
}
