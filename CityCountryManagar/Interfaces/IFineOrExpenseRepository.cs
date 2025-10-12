using RidersApp.DbModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RidersApp.Interfaces
{
    public interface IFineOrExpenseRepository
    {
        Task<List<FineOrExpense>> GetAll();
        Task<FineOrExpense?> GetById(int id);
        Task Add(FineOrExpense fineOrExpense);
        Task Edit(FineOrExpense fineOrExpense);
        Task Delete(int id);
        Task<bool> Exists(int id);
    }
}