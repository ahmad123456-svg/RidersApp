using Microsoft.EntityFrameworkCore;
using RidersApp.Data;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RidersApp.Repositories
{
    public class FineOrExpenseRepository : IFineOrExpenseRepository
    {
        private readonly ApplicationDbContext _context;

        public FineOrExpenseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<FineOrExpense>> GetAll()
        {
            return await _context.FineOrExpenses
                .Include(f => f.Employee)
                .Include(f => f.FineOrExpenseType)
                .OrderByDescending(f => f.EntryDate)
                .ToListAsync();
        }

        public async Task<FineOrExpense?> GetById(int id)
        {
            return await _context.FineOrExpenses
                .Include(f => f.Employee)
                .Include(f => f.FineOrExpenseType)
                .FirstOrDefaultAsync(f => f.Id == id);
        }

        public async Task Add(FineOrExpense fineOrExpense)
        {
            _context.FineOrExpenses.Add(fineOrExpense);
            await _context.SaveChangesAsync();
        }

        public async Task Edit(FineOrExpense fineOrExpense)
        {
            var existingEntity = await _context.FineOrExpenses
                .Include(f => f.Employee)
                .Include(f => f.FineOrExpenseType)
                .FirstOrDefaultAsync(f => f.Id == fineOrExpense.Id);

            if (existingEntity != null)
            {
                // Detach the existing entity
                _context.Entry(existingEntity).State = EntityState.Detached;

                // Attach and mark as modified
                _context.FineOrExpenses.Attach(fineOrExpense);
                _context.Entry(fineOrExpense).State = EntityState.Modified;

                await _context.SaveChangesAsync();
            }
        }

        public async Task Delete(int id)
        {
            var fineOrExpense = await _context.FineOrExpenses.FindAsync(id);
            if (fineOrExpense != null)
            {
                _context.FineOrExpenses.Remove(fineOrExpense);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> Exists(int id)
        {
            return await _context.FineOrExpenses.AnyAsync(f => f.Id == id);
        }
    }
}