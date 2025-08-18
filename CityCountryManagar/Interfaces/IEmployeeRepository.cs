using RidersApp.DbModels;
using System.Linq;
using System.Threading.Tasks;

namespace RidersApp.Interfaces
{
    public interface IEmployeeRepository
    {
        IQueryable<Employee> GetAllEmployees();
        Task<Employee> GetEmployeeById(int id);
        Task AddEmployee(Employee employee);
        Task UpdateEmployee(Employee employee);
        Task DeleteEmployee(int id);

    }
}
