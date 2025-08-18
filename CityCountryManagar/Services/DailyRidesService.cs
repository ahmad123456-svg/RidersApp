using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System;

namespace RidersApp.Services
{
    public class DailyRidesService : IDailyRidesService
    {
        private readonly IDailyRidesRepository _dailyRidesRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public DailyRidesService(IDailyRidesRepository dailyRidesRepository, IEmployeeRepository employeeRepository)
        {
            _dailyRidesRepository = dailyRidesRepository;
            _employeeRepository = employeeRepository;
        }

        public async Task<List<DailyRidesVM>> GetAll()
        {
            var dailyRides = await _dailyRidesRepository.GetAllAsync();
            return dailyRides.Select(d => new DailyRidesVM
            {
                Id = d.Id,
                CreditAmount = d.CreditAmount,
                CreditWAT = d.CreditWAT,
                CashAmount = d.CashAmount,
                CashWAT = d.CashWAT,
                Expense = d.Expense,
                EntryDate = d.EntryDate,
                EmployeeId = d.EmployeeId,
                EmployeeName = d.Employee != null ? d.Employee.Name : null,
                TodayRides = d.TodayRides,
                OverRides = d.OverRides,
                OverRidesAmount = d.OverRidesAmount,
                InsertDate = d.InsertDate,
                UpdateDate = d.UpdateDate,
                InsertedBy = d.InsertedBy,
                UpdatedBy = d.UpdatedBy,
                TotalRides = d.TotalRides,
                LessAmount = d.LessAmount
            }).ToList();
        }

        public async Task<DailyRidesVM> GetById(int id)
        {
            var dailyRide = await _dailyRidesRepository.GetByIdAsync(id);
            if (dailyRide == null) return null;

            return new DailyRidesVM
            {
                Id = dailyRide.Id,
                CreditAmount = dailyRide.CreditAmount,
                CreditWAT = dailyRide.CreditWAT,
                CashAmount = dailyRide.CashAmount,
                CashWAT = dailyRide.CashWAT,
                Expense = dailyRide.Expense,
                EntryDate = dailyRide.EntryDate,
                EmployeeId = dailyRide.EmployeeId,
                EmployeeName = dailyRide.Employee != null ? dailyRide.Employee.Name : null,
                TodayRides = dailyRide.TodayRides,
                OverRides = dailyRide.OverRides,
                OverRidesAmount = dailyRide.OverRidesAmount,
                InsertDate = dailyRide.InsertDate,
                UpdateDate = dailyRide.UpdateDate,
                InsertedBy = dailyRide.InsertedBy,
                UpdatedBy = dailyRide.UpdatedBy,
                TotalRides = dailyRide.TotalRides,
                LessAmount = dailyRide.LessAmount
            };
        }

        public async Task<List<DailyRidesVM>> Add(DailyRidesVM vm)
        {
            var entity = new DailyRides
            {
                CreditAmount = vm.CreditAmount,
                CreditWAT = vm.CreditWAT,
                CashAmount = vm.CashAmount,
                CashWAT = vm.CashWAT,
                Expense = vm.Expense,
                EntryDate = vm.EntryDate,
                EmployeeId = vm.EmployeeId,
                TodayRides = vm.TodayRides,
                OverRides = vm.OverRides,
                OverRidesAmount = vm.OverRidesAmount,
                InsertDate = DateTime.Now,
                InsertedBy = "System",
                TotalRides = vm.TotalRides,
                LessAmount = vm.LessAmount
            };

            await _dailyRidesRepository.AddAsync(entity);
            return await GetAll();
        }

        public async Task<List<DailyRidesVM>> Edit(DailyRidesVM vm)
        {
            var entity = await _dailyRidesRepository.GetByIdAsync(vm.Id);
            if (entity != null)
            {
                entity.CreditAmount = vm.CreditAmount;
                entity.CreditWAT = vm.CreditWAT;
                entity.CashAmount = vm.CashAmount;
                entity.CashWAT = vm.CashWAT;
                entity.Expense = vm.Expense;
                entity.EntryDate = vm.EntryDate;
                entity.EmployeeId = vm.EmployeeId;
                entity.TodayRides = vm.TodayRides;
                entity.OverRides = vm.OverRides;
                entity.OverRidesAmount = vm.OverRidesAmount;
                entity.UpdateDate = DateTime.Now;
                entity.UpdatedBy = "System";
                entity.TotalRides = vm.TotalRides;
                entity.LessAmount = vm.LessAmount;

                await _dailyRidesRepository.UpdateAsync(entity);
            }

            return await GetAll();
        }

        public async Task<List<DailyRidesVM>> Delete(int id)
        {
            try
            {
                // Check if daily ride record exists
                var dailyRide = await _dailyRidesRepository.GetByIdAsync(id);
                if (dailyRide == null)
                {
                    throw new InvalidOperationException($"Daily ride record with ID {id} not found.");
                }

                // Delete the daily ride record
                await _dailyRidesRepository.DeleteAsync(id);
                
                // Return updated list
                var result = await GetAll();
                return result;
            }
            catch (Exception ex)
            {
                // Log the error (you can implement proper logging here)
                var errorMessage = $"Failed to delete daily ride record with ID {id}. Error: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $" Inner error: {ex.InnerException.Message}";
                }
                throw new InvalidOperationException(errorMessage, ex);
            }
        }
    }
}
