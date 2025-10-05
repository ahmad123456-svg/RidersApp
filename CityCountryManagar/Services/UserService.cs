using RidersApp.ViewModels;
using RidersApp.IServices;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using RidersApp.Areas.Identity.Data;
using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RidersApp.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<RidersAppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(UserManager<RidersAppUser> userManager, RoleManager<IdentityRole> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<List<UserVM>> GetAll()
        {
            var users = _userManager.Users.ToList();
            var result = new List<UserVM>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                result.Add(new UserVM
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = roles.FirstOrDefault() ?? user.Role,
                    // Additional fields from database
                    NormalizedUserName = user.NormalizedUserName,
                    NormalizedEmail = user.NormalizedEmail,
                    EmailConfirmed = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                    TwoFactorEnabled = user.TwoFactorEnabled,
                    LockoutEnd = user.LockoutEnd,
                    LockoutEnabled = user.LockoutEnabled,
                    AccessFailedCount = user.AccessFailedCount,
                    SecurityStamp = user.SecurityStamp,
                    ConcurrencyStamp = user.ConcurrencyStamp
                });
            }
            return result;
        }

        public async Task<UserVM> GetById(string id)
        {
            Console.WriteLine($"UserService.GetById called for id: {id}");
            
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) 
            {
                Console.WriteLine($"User with id {id} not found");
                return null;
            }
            
            var roles = await _userManager.GetRolesAsync(user);
            Console.WriteLine($"Found user: {user.UserName} with roles: {string.Join(", ", roles)}");
            
            return new UserVM
            {
                Id = user.Id,
                Email = user.Email,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = roles.FirstOrDefault() ?? user.Role,
                // Additional fields from database
                NormalizedUserName = user.NormalizedUserName,
                NormalizedEmail = user.NormalizedEmail,
                EmailConfirmed = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                PhoneNumberConfirmed = user.PhoneNumberConfirmed,
                TwoFactorEnabled = user.TwoFactorEnabled,
                LockoutEnd = user.LockoutEnd,
                LockoutEnabled = user.LockoutEnabled,
                AccessFailedCount = user.AccessFailedCount,
                SecurityStamp = user.SecurityStamp,
                ConcurrencyStamp = user.ConcurrencyStamp
            };
        }

        public async Task<List<UserVM>> Add(UserVM vm)
        {
            Console.WriteLine($"UserService.Add called for user: {vm.Email}");
            Console.WriteLine($"Password provided: {!string.IsNullOrWhiteSpace(vm.Password)}");
            
            // ? Set UserName to Email automatically
            vm.UserName = vm.Email;
            
            var user = new RidersAppUser
            {
                UserName = vm.Email, // Use Email as UserName
                Email = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Role = vm.Role,
                EmailConfirmed = true, // Set to true for new users by default
                PhoneNumber = vm.PhoneNumber ?? string.Empty, // Handle null values
                LockoutEnabled = false // New users should not be locked by default
            };
            
            Console.WriteLine($"Creating user with Email as UserName: {vm.Email}");
            var result = await _userManager.CreateAsync(user, vm.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"UserManager.CreateAsync failed: {errors}");
                throw new Exception($"Failed to create user: {errors}");
            }
            
            Console.WriteLine("User created successfully, now adding to role...");
            
            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(vm.Role))
            {
                Console.WriteLine($"Creating role: {vm.Role}");
                await _roleManager.CreateAsync(new IdentityRole(vm.Role));
            }
            
            // Add user to role
            var roleResult = await _userManager.AddToRoleAsync(user, vm.Role);
            if (!roleResult.Succeeded)
            {
                var roleErrors = string.Join(", ", roleResult.Errors.Select(e => e.Description));
                Console.WriteLine($"Failed to assign role: {roleErrors}");
                throw new Exception($"User created but failed to assign role: {roleErrors}");
            }
            
            Console.WriteLine("User and role assignment completed successfully");
            return await GetAll();
        }

        public async Task<List<UserVM>> Edit(UserVM vm)
        {
            Console.WriteLine($"UserService.Edit called for user: {vm.Email} (ID: {vm.Id})");
            
            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) 
            {
                Console.WriteLine($"User with id {vm.Id} not found");
                throw new Exception("User not found");
            }
            
            Console.WriteLine($"Found existing user: {user.UserName}");
            Console.WriteLine($"Current EmailConfirmed status: {user.EmailConfirmed}");
            Console.WriteLine($"Updating user properties...");
            
            // ? Set UserName to Email for consistency
            vm.UserName = vm.Email;
            
            // Update basic properties
            user.UserName = vm.Email; // Use Email as UserName
            user.Email = vm.Email;
            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Role = vm.Role;
            user.PhoneNumber = vm.PhoneNumber ?? user.PhoneNumber;
            
            // ? CRITICAL FIX: Keep EmailConfirmed as true for existing users
            // This ensures users remain active after updates
            user.EmailConfirmed = true;
            
            // Keep LockoutEnabled as false unless specifically needed
            user.LockoutEnabled = false;
            
            Console.WriteLine("EmailConfirmed set to true to keep user active");
            
            Console.WriteLine("Updating user with UserManager...");
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"UserManager.UpdateAsync failed: {errors}");
                throw new Exception($"Failed to update user: {errors}");
            }
            
            Console.WriteLine("User updated successfully");
            
            // ? SKIP password update for edit operations since password fields are hidden
            Console.WriteLine("Password update skipped - password fields are hidden during edit");
            
            // Update roles
            Console.WriteLine($"Updating user role to: {vm.Role}");
            var currentRoles = await _userManager.GetRolesAsync(user);
            Console.WriteLine($"Current roles: {string.Join(", ", currentRoles)}");
            
            if (!currentRoles.Contains(vm.Role))
            {
                if (currentRoles.Any())
                {
                    Console.WriteLine($"Removing existing roles: {string.Join(", ", currentRoles)}");
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                }
                
                // Ensure role exists
                if (!await _roleManager.RoleExistsAsync(vm.Role))
                {
                    Console.WriteLine($"Creating role: {vm.Role}");
                    await _roleManager.CreateAsync(new IdentityRole(vm.Role));
                }
                
                Console.WriteLine($"Adding user to role: {vm.Role}");
                await _userManager.AddToRoleAsync(user, vm.Role);
            }
            else
            {
                Console.WriteLine("User already has the correct role");
            }
            
            Console.WriteLine("User edit completed successfully - user remains active, password unchanged");
            return await GetAll();
        }

        public async Task<List<UserVM>> Delete(string id)
        {
            Console.WriteLine($"UserService.Delete called for id: {id}");
            
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) 
            {
                Console.WriteLine($"User with id {id} not found");
                throw new Exception("User not found");
            }
            
            Console.WriteLine($"Deleting user: {user.UserName}");
            
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"UserManager.DeleteAsync failed: {errors}");
                throw new Exception($"Failed to delete user: {errors}");
            }
            
            Console.WriteLine("User deleted successfully");
            return await GetAll();
        }

        // ? NEW: Password update methods
        public async Task<UpdatePasswordVM> GetUserForPasswordUpdate(string id)
        {
            Console.WriteLine($"UserService.GetUserForPasswordUpdate called for id: {id}");
            
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) 
            {
                Console.WriteLine($"User with id {id} not found");
                throw new Exception("User not found");
            }
            
            Console.WriteLine($"Found user for password update: {user.UserName}");
            
            return new UpdatePasswordVM
            {
                Id = user.Id,
                Email = user.Email,
                FullName = $"{user.FirstName} {user.LastName}"
            };
        }

        public async Task UpdateUserPassword(UpdatePasswordVM vm)
        {
            Console.WriteLine($"UserService.UpdateUserPassword called for user ID: {vm.Id}");
            
            var user = await _userManager.FindByIdAsync(vm.Id);
            if (user == null) 
            {
                Console.WriteLine($"User with id {vm.Id} not found");
                throw new Exception("User not found");
            }
            
            Console.WriteLine($"Updating password for user: {user.UserName}");
            
            // Reset password using UserManager
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, vm.NewPassword);
            
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"Password update failed: {errors}");
                throw new Exception($"Failed to update password: {errors}");
            }
            
            Console.WriteLine("Password updated successfully");
        }

        // ? NEW: User self-password change methods
        public async Task<ChangePasswordVM> GetCurrentUserForPasswordChange()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                throw new Exception("User not authenticated");
            }

            var currentUser = await _userManager.GetUserAsync(httpContext.User);
            if (currentUser == null)
            {
                throw new Exception("Current user not found");
            }

            Console.WriteLine($"Getting current user for password change: {currentUser.UserName}");

            return new ChangePasswordVM
            {
                Email = currentUser.Email,
                FullName = $"{currentUser.FirstName} {currentUser.LastName}"
            };
        }

        public async Task ChangeCurrentUserPassword(ChangePasswordVM vm)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext?.User?.Identity?.IsAuthenticated != true)
            {
                throw new Exception("User not authenticated");
            }

            var currentUser = await _userManager.GetUserAsync(httpContext.User);
            if (currentUser == null)
            {
                throw new Exception("Current user not found");
            }

            Console.WriteLine($"Changing password for current user: {currentUser.UserName}");

            // Verify current password
            var isCurrentPasswordValid = await _userManager.CheckPasswordAsync(currentUser, vm.CurrentPassword);
            if (!isCurrentPasswordValid)
            {
                Console.WriteLine("Current password is incorrect");
                throw new Exception("Current password is incorrect");
            }

            // Change password
            var result = await _userManager.ChangePasswordAsync(currentUser, vm.CurrentPassword, vm.NewPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                Console.WriteLine($"Password change failed: {errors}");
                throw new Exception($"Failed to change password: {errors}");
            }

            Console.WriteLine("Password changed successfully");
        }

        // ? NEW: Enhanced business logic methods for controller
        public async Task<(bool success, string message, string html)> ProcessUpdatePassword(UpdatePasswordVM vm, ModelStateDictionary modelState)
        {
            Console.WriteLine($"ProcessUpdatePassword called for user ID: {vm.Id}");
            
            // Remove display-only fields from validation
            modelState.Remove("Email");
            modelState.Remove("FullName");
            
            Console.WriteLine($"ModelState.IsValid: {modelState.IsValid}");
            
            if (modelState.IsValid)
            {
                try
                {
                    Console.WriteLine("Attempting to update password...");
                    await UpdateUserPassword(vm);
                    Console.WriteLine("Password updated successfully in service");
                    
                    return (true, "Password updated successfully", null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating password: {ex.Message}");
                    modelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                Console.WriteLine("ModelState validation failed:");
                foreach (var modelError in modelState)
                {
                    var key = modelError.Key;
                    var errors = modelError.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  {key}: {error.ErrorMessage}");
                    }
                }
            }
            
            Console.WriteLine("Returning validation error response");
            return (false, null, RenderUpdatePasswordView(vm));
        }

        public async Task<(bool success, string message, string html)> ProcessChangePassword(ChangePasswordVM vm, ModelStateDictionary modelState)
        {
            Console.WriteLine($"ProcessChangePassword called for current user");
            
            // Remove display-only fields from validation
            modelState.Remove("Email");
            modelState.Remove("FullName");
            
            Console.WriteLine($"ModelState.IsValid: {modelState.IsValid}");
            
            if (modelState.IsValid)
            {
                try
                {
                    Console.WriteLine("Attempting to change current user password...");
                    await ChangeCurrentUserPassword(vm);
                    Console.WriteLine("Password changed successfully");
                    
                    return (true, "Password changed successfully", null);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error changing password: {ex.Message}");
                    modelState.AddModelError("", ex.Message);
                }
            }
            else
            {
                Console.WriteLine("ModelState validation failed:");
                foreach (var modelError in modelState)
                {
                    var key = modelError.Key;
                    var errors = modelError.Value.Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($"  {key}: {error.ErrorMessage}");
                    }
                }
            }
            
            Console.WriteLine("Returning validation error response");
            return (false, null, RenderChangePasswordView(vm));
        }

        // ? NEW: View rendering methods (to be implemented with proper dependency injection)
        public string RenderUpdatePasswordView(UpdatePasswordVM vm)
        {
            // This will be implemented with proper view rendering
            // For now, return null to indicate we need to handle this in controller
            // until we can inject the view rendering service
            return null;
        }

        public string RenderChangePasswordView(ChangePasswordVM vm)
        {
            // This will be implemented with proper view rendering
            // For now, return null to indicate we need to handle this in controller
            // until we can inject the view rendering service
            return null;
        }

        // DataTables server-side processing for Users
        public async Task<object> GetUsersData(IFormCollection form)
        {
            var draw = form["draw"].FirstOrDefault();
            var start = int.TryParse(form["start"].FirstOrDefault(), out int s) ? s : 0;
            var length = int.TryParse(form["length"].FirstOrDefault(), out int l) ? l : 10;
            var searchValue = form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = form["order[0][column]"].FirstOrDefault();
            var sortDirection = form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);
            // ? Updated column names to match new table structure (removed UserName)
            string[] columnNames = new[] { "Email", "FirstName", "LastName", "Role", "UserStatus" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : columnNames[0];

            var all = await GetAll();
            var query = all.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.Email ?? string.Empty).ToLower().Contains(lower) ||
                    (x.FirstName ?? string.Empty).ToLower().Contains(lower) ||
                    (x.LastName ?? string.Empty).ToLower().Contains(lower) ||
                    (x.Role ?? string.Empty).ToLower().Contains(lower)
                );
            }

            var recordsFiltered = query.Count();

            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);
            query = sortColumn switch
            {
                "Email" => ascending ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email),
                "FirstName" => ascending ? query.OrderBy(x => x.FirstName) : query.OrderByDescending(x => x.FirstName),
                "LastName" => ascending ? query.OrderBy(x => x.LastName) : query.OrderByDescending(x => x.LastName),
                "Role" => ascending ? query.OrderBy(x => x.Role) : query.OrderByDescending(x => x.Role),
                "UserStatus" => ascending ? query.OrderBy(x => x.UserStatus) : query.OrderByDescending(x => x.UserStatus),
                _ => ascending ? query.OrderBy(x => x.Email) : query.OrderByDescending(x => x.Email)
            };

            var pageData = query.Skip(start).Take(length).ToList();

            return new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = pageData
            };
        }

        // New validation and processing methods moved from controller
        public void CleanModelStateForUser(ModelStateDictionary modelState, bool isNewUser)
        {
            Console.WriteLine("UserService.CleanModelStateForUser called");
            
            // Remove all non-essential fields from ModelState validation since they're not in the form
            modelState.Remove("LockoutEnd");
            modelState.Remove("NormalizedEmail");
            modelState.Remove("NormalizedUserName");
            modelState.Remove("PhoneNumber");
            modelState.Remove("SecurityStamp");
            modelState.Remove("ConcurrencyStamp");
            modelState.Remove("EmailConfirmed");
            modelState.Remove("PhoneNumberConfirmed");
            modelState.Remove("TwoFactorEnabled");
            modelState.Remove("LockoutEnabled");
            modelState.Remove("AccessFailedCount");
            modelState.Remove("FullName");
            modelState.Remove("UserStatus");
            modelState.Remove("HasExistingPassword");
            modelState.Remove("IsLocked");
            modelState.Remove("UserName"); // ? Remove UserName from validation since it's auto-set
            
            // ? For edit users, remove password validation since fields are hidden
            if (!isNewUser)
            {
                modelState.Remove("Password");
                modelState.Remove("ConfirmPassword");
                Console.WriteLine("Password fields removed from ModelState for edit user");
            }
            
            if (isNewUser)
            {
                modelState.Remove("Id");
            }
            
            Console.WriteLine("ModelState cleaned successfully");
        }

        public void ValidateUserPassword(ModelStateDictionary modelState, UserVM vm, bool isNewUser)
        {
            Console.WriteLine($"UserService.ValidateUserPassword called for {(isNewUser ? "new" : "existing")} user");
            
            if (isNewUser)
            {
                // For new users, password is required
                if (string.IsNullOrWhiteSpace(vm.Password))
                {
                    modelState.AddModelError("Password", "Password is required for new users.");
                    Console.WriteLine("Added error: Password required for new users");
                }
                else if (vm.Password.Length < 8)
                {
                    modelState.AddModelError("Password", "Password must be at least 8 characters long.");
                    Console.WriteLine("Added error: Password too short");
                }

                // ? Validate confirm password for new users
                if (string.IsNullOrWhiteSpace(vm.ConfirmPassword))
                {
                    modelState.AddModelError("ConfirmPassword", "Please confirm your password.");
                    Console.WriteLine("Added error: Confirm password required");
                }
                else if (vm.Password != vm.ConfirmPassword)
                {
                    modelState.AddModelError("ConfirmPassword", "Password and Confirm Password do not match.");
                    Console.WriteLine("Added error: Passwords do not match");
                }
            }
            else
            {
                // ? For existing users, NO password validation since fields are hidden
                // Password will remain unchanged when editing user information
                Console.WriteLine("No password validation for edit user - password fields are hidden");
            }
        }

        public void PreparePasswordForReturn(UserVM vm, string originalPassword, bool isNewUser)
        {
            Console.WriteLine("UserService.PreparePasswordForReturn called");
            
            if (isNewUser)
            {
                // For new users, preserve password and confirm password on validation errors
                vm.Password = originalPassword;
                // Don't preserve confirm password for security - let user re-enter
                vm.ConfirmPassword = string.Empty;
                Console.WriteLine("Password preserved for new user, confirm password cleared");
            }
            else
            {
                // ? For edit operations, clear password fields completely since they're hidden
                vm.Password = string.Empty;
                vm.ConfirmPassword = string.Empty;
                Console.WriteLine("Password and confirm password cleared for existing user (fields are hidden)");
            }
        }
    }
}
