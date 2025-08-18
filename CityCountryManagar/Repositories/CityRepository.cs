using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RidersApp.Data;
using RidersApp.Interfaces;
using RidersApp.DbModels;
using System.Linq;
using System;

namespace RidersApp.Repositories
{
    public class CityRepository : ICityRepository
    {
        private readonly ApplicationDbContext _context;

        public CityRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<City>> GetAllAsync()
        {
            return await _context.Cities.Include(c => c.Country).ToListAsync();
        }

        public async Task<City> GetByIdAsync(int id)
        {
            return await _context.Cities.Include(c => c.Country).FirstOrDefaultAsync(c => c.CityId == id);
        }

        public async Task AddAsync(City city)
        {
            _context.Cities.Add(city);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(City city)
        {
            _context.Cities.Update(city);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var city = await _context.Cities.FindAsync(id);
            if (city != null)
            {
                _context.Cities.Remove(city);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Cities.AnyAsync(e => e.CityId == id);
        }

        public async Task<bool> HasRelatedEmployeesAsync(int cityId)
        {
            return await _context.Employees.AnyAsync(e => e.CityId == cityId);
        }
    }
}
