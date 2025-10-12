using Microsoft.EntityFrameworkCore;
using RidersApp.Data;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RidersApp.Repositories
{
    public class FineOrExpenseTypeRepository : IFineOrExpenseTypeRepository
    {
        private readonly ApplicationDbContext _context;

        public FineOrExpenseTypeRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FineOrExpenseType>> GetAll()
        {
            return await _context.FineOrExpenseTypes
                .OrderBy(x => x.Name)
                .ToListAsync();
        }

        public async Task<FineOrExpenseType> GetById(int id)
        {
            return await _context.FineOrExpenseTypes.FindAsync(id);
        }

        public async Task<FineOrExpenseType> Add(FineOrExpenseType entity)
        {
            _context.FineOrExpenseTypes.Add(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<FineOrExpenseType> Update(FineOrExpenseType entity)
        {
            _context.Entry(entity).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return entity;
        }

        public async Task Delete(int id)
        {
            var entity = await _context.FineOrExpenseTypes.FindAsync(id);
            if (entity != null)
            {
                _context.FineOrExpenseTypes.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Exists(string name, int? id = null)
        {
            return await _context.FineOrExpenseTypes
                .AnyAsync(x => x.Name.ToLower() == name.ToLower() && (!id.HasValue || x.Id != id.Value));
        }
    }
}