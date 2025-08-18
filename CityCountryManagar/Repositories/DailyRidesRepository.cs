using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RidersApp.Data;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using System;

namespace RidersApp.Repositories
{
    public class DailyRidesRepository : IDailyRidesRepository
    {
        private readonly ApplicationDbContext _context;

        public DailyRidesRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<DailyRides>> GetAllAsync()
        {
            return await _context.DailyRides
                .Include(d => d.Employee)
                .ToListAsync();
        }

        public async Task<DailyRides> GetByIdAsync(int id)
        {
            return await _context.DailyRides
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task AddAsync(DailyRides dailyRides)
        {
            _context.DailyRides.Add(dailyRides);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(DailyRides dailyRides)
        {

            _context.DailyRides.Update(dailyRides);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var dailyRides = await _context.DailyRides.FindAsync(id);
            if (dailyRides != null)
            {
                _context.DailyRides.Remove(dailyRides);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.DailyRides.AnyAsync(e => e.Id == id);
        }
    }
}
