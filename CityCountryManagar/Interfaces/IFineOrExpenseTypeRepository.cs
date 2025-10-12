using RidersApp.DbModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RidersApp.Interfaces
{
    public interface IFineOrExpenseTypeRepository
    {
        Task<List<FineOrExpenseType>> GetAll();
        Task<FineOrExpenseType> GetById(int id);
        Task<FineOrExpenseType> Add(FineOrExpenseType entity);
        Task<FineOrExpenseType> Update(FineOrExpenseType entity);
        Task Delete(int id);
        Task<bool> Exists(string name, int? id = null);
    }
}