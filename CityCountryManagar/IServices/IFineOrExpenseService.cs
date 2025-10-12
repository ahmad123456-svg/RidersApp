using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RidersApp.IServices
{
    public interface IFineOrExpenseService
    {
        Task<List<FineOrExpenseVM>> GetAll();
        Task<FineOrExpenseVM?> GetById(int id);
        Task<(bool success, string message, List<FineOrExpenseVM>? data)> Add(FineOrExpenseVM vm);
        Task<(bool success, string message, List<FineOrExpenseVM>? data)> Edit(FineOrExpenseVM vm);
        Task<(bool success, string message)> Delete(int id);
        Task<object> GetFineOrExpensesData(IFormCollection form);
        Task<(bool isValid, List<string> errors)> ValidateModel(FineOrExpenseVM vm);
        Task<(bool isValid, List<string> errors)> ValidateBusinessRules(FineOrExpenseVM vm);
    }
}