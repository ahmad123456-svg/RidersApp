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
        private readonly RidersApp.IServices.IConfigurationService _configurationService;

        public DailyRidesService(IDailyRidesRepository dailyRidesRepository, IEmployeeRepository employeeRepository, RidersApp.IServices.IConfigurationService configurationService)
        {
            _dailyRidesRepository = dailyRidesRepository;
            _employeeRepository = employeeRepository;
            _configurationService = configurationService;
        }

        public async Task<List<DailyRidesVM>> GetAll()
        {
            var dailyRides = await _dailyRidesRepository.GetAllAsync();
            return dailyRides
                .OrderBy(d => (d.Employee != null ? d.Employee.Name : string.Empty).ToLowerInvariant())
                .Select(d => new DailyRidesVM
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
            // Calculate WAT values from configuration
            var configs = await _configurationService.GetAll();
            decimal cashPercent = 0;
            decimal creditPercent = 0;
            var cashCfg = configs.FirstOrDefault(c => string.Equals(c.KeyName, "CashWAT", StringComparison.OrdinalIgnoreCase));
            var creditCfg = configs.FirstOrDefault(c => string.Equals(c.KeyName, "CreditWAT", StringComparison.OrdinalIgnoreCase));

            // parse percentage values from configurations so WAT is computed correctly
            if (cashCfg != null && decimal.TryParse(cashCfg.Value, out var cashp)) cashPercent = cashp;
            if (creditCfg != null && decimal.TryParse(creditCfg.Value, out var cp)) creditPercent = cp;

            var entity = new DailyRides
            {
                CashAmount = vm.CashAmount,
                // CashWAT = CashAmount * (cashPercent / 100)
                CashWAT = Math.Round(vm.CashAmount * (cashPercent / 100), 2),
                CreditAmount = vm.CreditAmount,
                // Use simpler formula: CreditWAT = CreditAmount * (creditPercent / 100)
                CreditWAT = Math.Round(vm.CreditAmount * (creditPercent / 100), 2),
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
                // Recalculate WAT values using configuration
                var configs = await _configurationService.GetAll();
                decimal creditPercent = 0;
                decimal cashPercent = 0;
                var creditCfg = configs.FirstOrDefault(c => string.Equals(c.KeyName, "CreditWAT", StringComparison.OrdinalIgnoreCase));
                var cashCfg = configs.FirstOrDefault(c => string.Equals(c.KeyName, "CashWAT", StringComparison.OrdinalIgnoreCase));
                // parse percentage values from configurations so WAT is computed correctly
                if (creditCfg != null && decimal.TryParse(creditCfg.Value, out var cp)) creditPercent = cp;
                if (cashCfg != null && decimal.TryParse(cashCfg.Value, out var cashp)) cashPercent = cashp;

                entity.CreditAmount = vm.CreditAmount;
                // Recalculate using the simpler formula form
                entity.CreditWAT = Math.Round(vm.CreditAmount * (creditPercent / 100), 2);
                entity.CashAmount = vm.CashAmount;
                entity.CashWAT = Math.Round(vm.CashAmount * (cashPercent / 100), 2);
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
