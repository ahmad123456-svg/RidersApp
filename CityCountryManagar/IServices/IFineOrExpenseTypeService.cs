using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RidersApp.IServices
{
    public interface IFineOrExpenseTypeService
    {
        Task<List<FineOrExpenseTypeVM>> GetAll();
        Task<FineOrExpenseTypeVM> GetById(int id);
        Task<List<FineOrExpenseTypeVM>> Add(FineOrExpenseTypeVM vm);
        Task<List<FineOrExpenseTypeVM>> Edit(FineOrExpenseTypeVM vm);
        Task Delete(int id);
        Task<object> GetFineOrExpenseTypesData(IFormCollection form);
    }
}