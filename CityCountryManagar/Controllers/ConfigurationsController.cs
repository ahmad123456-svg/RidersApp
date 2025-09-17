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
            var draw = Request.Form["draw"].FirstOrDefault();
            var start = Convert.ToInt32(Request.Form["start"].FirstOrDefault() ?? "0");
            var length = Convert.ToInt32(Request.Form["length"].FirstOrDefault() ?? "10");
            var searchValue = Request.Form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = Request.Form["order[0][column]"].FirstOrDefault();
            var sortDirection = Request.Form["order[0][dir]"].FirstOrDefault();

            int sortColumnIndex = 0;
            int.TryParse(sortColumnIndexString, out sortColumnIndex);

            string[] columnNames = new[] { "KeyName", "Value" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var all = await _service.GetAll();
            var query = all.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.KeyName ?? string.Empty).ToLower().Contains(lower) ||
                    (x.Value ?? string.Empty).ToLower().Contains(lower)
                );
            }

            var recordsFiltered = query.Count();

            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = sortColumn switch
            {
                "KeyName" => ascending ? query.OrderBy(x => x.KeyName) : query.OrderByDescending(x => x.KeyName),
                "Value" => ascending ? query.OrderBy(x => x.Value) : query.OrderByDescending(x => x.Value),
                _ => ascending ? query.OrderBy(x => x.KeyName) : query.OrderByDescending(x => x.KeyName)
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
