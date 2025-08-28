using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RidersApp.Data;
using RidersApp.Interfaces;
using RidersApp.DbModels;

namespace RidersApp.Repositories
{
    public class ConfigurationRepository : IConfigurationRepository
    {
        private readonly ApplicationDbContext _context;

        public ConfigurationRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Configuration>> GetAllAsync()
        {
            return await _context.Configurations.ToListAsync();
        }

        public async Task<Configuration> GetByIdAsync(int id)
        {
            return await _context.Configurations.FindAsync(id);
        }

        public async Task AddAsync(Configuration config)
        {
            _context.Configurations.Add(config);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Configuration config)
        {
            _context.Configurations.Update(config);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var entity = await _context.Configurations.FindAsync(id);
            if (entity != null)
            {
                _context.Configurations.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Configurations.AnyAsync(e => e.ConfigurationId == id);
        }
    }
}
