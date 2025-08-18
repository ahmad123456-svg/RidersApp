using RidersApp.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RidersApp.IServices
{
    public interface IEmployeeService
    {
        Task<List<EmployeeVM>> GetAll();
        Task<EmployeeVM> GetById(int id);
        Task<List<EmployeeVM>> Add(EmployeeVM vm);
        Task<List<EmployeeVM>> Edit(EmployeeVM vm);
        Task<List<EmployeeVM>> Delete(int id);
    }
}
