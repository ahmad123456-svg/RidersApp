using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.Data;
using System;

namespace RidersApp.Repositories
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ApplicationDbContext _context;

        public EmployeeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public IQueryable<Employee> GetAllEmployees()
        {
            return _context.Employees.AsQueryable();
        }

        public async Task<Employee> GetEmployeeById(int id)
        {
            return await _context.Employees
                .Include(e => e.Country)
                .Include(e => e.City)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
        }

        public async Task AddEmployee(Employee employee)
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployee(Employee employee)
        {
            _context.Employees.Update(employee);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
            }
        }
    }
}
