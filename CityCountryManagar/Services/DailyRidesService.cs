using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;

namespace RidersApp.Services
{
    public class DailyRidesService : IDailyRidesService
    {
        private readonly IDailyRidesRepository _dailyRidesRepository;
        private readonly IConfigurationService _configurationService;

        public DailyRidesService(
            IDailyRidesRepository dailyRidesRepository,
            IConfigurationService configurationService)
        {
            _dailyRidesRepository = dailyRidesRepository;
            _configurationService = configurationService;
        }

        public async Task<List<DailyRidesVM>> GetAll()
        {
            var dailyRides = await _dailyRidesRepository.GetAllAsync();
            return dailyRides
                .OrderBy(d => (d.Employee?.Name ?? string.Empty).ToLowerInvariant())
                .Select(MapToVM)
                .ToList();
        }

        public async Task<DailyRidesVM> GetById(int id)
        {
            var dailyRide = await _dailyRidesRepository.GetByIdAsync(id);
            return dailyRide == null ? null : MapToVM(dailyRide);
        }

        public async Task<List<DailyRidesVM>> Add(DailyRidesVM vm)
        {
            var entity = await MapToEntity(vm, true);
            await _dailyRidesRepository.AddAsync(entity);
            return await GetAll();
        }

        public async Task<List<DailyRidesVM>> Edit(DailyRidesVM vm)
        {
            var entity = await _dailyRidesRepository.GetByIdAsync(vm.Id);
            if (entity != null)
            {
                var updatedEntity = await MapToEntity(vm, false, entity);
                await _dailyRidesRepository.UpdateAsync(updatedEntity);
            }
            return await GetAll();
        }

        public async Task<List<DailyRidesVM>> Delete(int id)
        {
            await _dailyRidesRepository.DeleteAsync(id);
            return await GetAll();
        }

        #region Helpers

        private async Task<DailyRides> MapToEntity(DailyRidesVM vm, bool isNew, DailyRides existingEntity = null)
        {
            var configs = await _configurationService.GetAll();
            decimal cashPercent = ParsePercent(configs, "CashWAT");
            decimal creditPercent = ParsePercent(configs, "CreditWAT");

            var entity = existingEntity ?? new DailyRides();

            entity.CashAmount = vm.CashAmount;
            entity.CashWAT = Math.Round(vm.CashAmount * (cashPercent / 100), 2);
            entity.CreditAmount = vm.CreditAmount;
            entity.CreditWAT = Math.Round(vm.CreditAmount * (creditPercent / 100), 2);
            entity.Expense = vm.Expense;
            entity.EntryDate = vm.EntryDate;
            entity.EmployeeId = vm.EmployeeId;
            entity.TodayRides = vm.TodayRides;
            entity.OverRides = vm.OverRides;
            entity.OverRidesAmount = vm.OverRidesAmount;
            entity.TotalRides = vm.TotalRides;
            entity.LessAmount = vm.LessAmount;

            if (isNew)
            {
                entity.InsertDate = DateTime.Now;
                entity.InsertedBy = "System";
            }
            else
            {
                entity.UpdateDate = DateTime.Now;
                entity.UpdatedBy = "System";
            }

            return entity;
        }

        private DailyRidesVM MapToVM(DailyRides d)
        {
            return new DailyRidesVM
            {
                Id = d.Id,
                CreditAmount = d.CreditAmount,
                CreditWAT = d.CreditWAT,
                CashAmount = d.CashAmount,
                CashWAT = d.CashWAT,
                Expense = d.Expense,
                EntryDate = d.EntryDate,
                EmployeeId = d.EmployeeId,
                EmployeeName = d.Employee?.Name,
                TodayRides = d.TodayRides,
                OverRides = d.OverRides,
                OverRidesAmount = d.OverRidesAmount,
                InsertDate = d.InsertDate,
                UpdateDate = d.UpdateDate,
                InsertedBy = d.InsertedBy,
                UpdatedBy = d.UpdatedBy,
                TotalRides = d.TotalRides,
                LessAmount = d.LessAmount
            };
        }

        private decimal ParsePercent(IEnumerable<ConfigurationVM> configs, string key)
        {
            var cfg = configs.FirstOrDefault(c => string.Equals(c.KeyName, key, StringComparison.OrdinalIgnoreCase));
            return cfg != null && decimal.TryParse(cfg.Value, out var val) ? val : 0;
        }

        #endregion
    }
}
