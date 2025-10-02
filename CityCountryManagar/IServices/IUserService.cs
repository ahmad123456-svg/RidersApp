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
        
        // New methods for validation and processing
        void CleanModelStateForUser(ModelStateDictionary modelState, bool isNewUser);
        void ValidateUserPassword(ModelStateDictionary modelState, UserVM vm, bool isNewUser);
        void PreparePasswordForReturn(UserVM vm, string originalPassword, bool isNewUser);
    }
}
