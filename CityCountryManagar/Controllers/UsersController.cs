using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System.Threading.Tasks;
using System;
using System.Linq;

namespace RidersApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        public IActionResult Index()
        {
            // For server-side processing, we don't need to load all users initially
            return View();
        }

        public async Task<IActionResult> GetUsers()
        {
            var users = await _userService.GetAll();
            return PartialView("_ViewAll", users);
        }

        [HttpPost]
        public async Task<IActionResult> GetUsersData()
        {
            var result = await _userService.GetUsersData(Request.Form);
            return Json(result);
        }

        public async Task<IActionResult> AddOrEdit(string id = null)
        {
            if (string.IsNullOrEmpty(id))
                return PartialView(new UserVM());
                
            var vm = await _userService.GetById(id);
            if (vm == null) return NotFound();
            
            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrEdit(string id, UserVM vm)
        {
            bool isNewUser = string.IsNullOrEmpty(id);
            string originalPassword = vm.Password;
            string originalConfirmPassword = vm.ConfirmPassword;
            
            // Delegate validation to service
            _userService.CleanModelStateForUser(ModelState, isNewUser);
            _userService.ValidateUserPassword(ModelState, vm, isNewUser);
            
            if (ModelState.IsValid)
            {
                try
                {
                    string message;
                    if (isNewUser)
                    {
                        await _userService.Add(vm);
                        message = "User added successfully";
                    }
                    else
                    {
                        vm.Id = id;
                        await _userService.Edit(vm);
                        message = "User updated successfully";
                    }
                    
                    return Json(new { isValid = true, message });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            
           
            _userService.PreparePasswordForReturn(vm, originalPassword, isNewUser);
            
            // Store confirm password in ViewBag for preservation on validation errors (new users only)
            if (isNewUser)
            {
                ViewBag.PreservedConfirmPassword = originalConfirmPassword;
            }
            
            return Json(new
            {
                isValid = false,
                html = Helper.RenderRazorViewToString(this, "AddOrEdit", vm)
            });
        }

        public async Task<IActionResult> UpdatePassword(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();
                
            try
            {
                var vm = await _userService.GetUserForPasswordUpdate(id);
                return PartialView(vm);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePassword(UpdatePasswordVM vm)
        {
            var result = await _userService.ProcessUpdatePassword(vm, ModelState);
            
            if (result.success)
            {
                return Json(new { isValid = true, message = result.message });
            }
           
            var html = result.html ?? Helper.RenderRazorViewToString(this, "UpdatePassword", vm);
            return Json(new { isValid = false, html });
        }

        
        [Authorize] 
        [AllowAnonymous] // Override the class-level Admin restriction
        public async Task<IActionResult> ChangePassword()
        {
            
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            
            try
            {
                var vm = await _userService.GetCurrentUserForPasswordChange();
                return PartialView(vm);
            }
            catch (Exception ex)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize] 
        [AllowAnonymous] 
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM vm)
        {
          
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }
            
            var result = await _userService.ProcessChangePassword(vm, ModelState);
            
            if (result.success)
            {
                return Json(new { isValid = true, message = result.message });
            }
            
            // Handle validation errors - render view if service didn't provide HTML
            var html = result.html ?? Helper.RenderRazorViewToString(this, "ChangePassword", vm);
            return Json(new { isValid = false, html });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            try
            {
                await _userService.Delete(id);
                return Json(new { success = true, message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
