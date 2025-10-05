using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace RidersApp.IServices
{
    public interface IUserService
    {
        Task<List<UserVM>> GetAll();
        Task<UserVM> GetById(string id);
        Task<List<UserVM>> Add(UserVM vm);
        Task<List<UserVM>> Edit(UserVM vm);
        Task<List<UserVM>> Delete(string id);
        Task<object> GetUsersData(IFormCollection form);
        
        // ? Enhanced Password update methods for admin
        Task<UpdatePasswordVM> GetUserForPasswordUpdate(string id);
        Task<(bool success, string message, string html)> ProcessUpdatePassword(UpdatePasswordVM vm, ModelStateDictionary modelState);
        
        // ? Enhanced Password change methods for user self-service
        Task<ChangePasswordVM> GetCurrentUserForPasswordChange();
        Task<(bool success, string message, string html)> ProcessChangePassword(ChangePasswordVM vm, ModelStateDictionary modelState);
        
        // Existing validation methods
        void CleanModelStateForUser(ModelStateDictionary modelState, bool isNewUser);
        void ValidateUserPassword(ModelStateDictionary modelState, UserVM vm, bool isNewUser);
        void PreparePasswordForReturn(UserVM vm, string originalPassword, bool isNewUser);
        
        // ? New methods for rendering views
        string RenderUpdatePasswordView(UpdatePasswordVM vm);
        string RenderChangePasswordView(ChangePasswordVM vm);
    }
}
