using AutoMapper;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.IServices;
using RidersApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace RidersApp.Services
{
    public class FineOrExpenseService : IFineOrExpenseService
    {
        private readonly IFineOrExpenseRepository _repository;
        private readonly IMapper _mapper;
        private readonly IEmployeeService _employeeService;
        private readonly IFineOrExpenseTypeService _fineOrExpenseTypeService;

        public FineOrExpenseService(
            IFineOrExpenseRepository repository,
            IMapper mapper,
            IEmployeeService employeeService,
            IFineOrExpenseTypeService fineOrExpenseTypeService)
        {
            _repository = repository;
            _mapper = mapper;
            _employeeService = employeeService;
            _fineOrExpenseTypeService = fineOrExpenseTypeService;
        }

        public async Task<List<FineOrExpenseVM>> GetAll()
        {
            var entities = await _repository.GetAll();
            return _mapper.Map<List<FineOrExpenseVM>>(entities);
        }

        public async Task<FineOrExpenseVM?> GetById(int id)
        {
            var entity = await _repository.GetById(id);
            return _mapper.Map<FineOrExpenseVM>(entity);
        }

        public async Task<(bool isValid, List<string> errors)> ValidateModel(FineOrExpenseVM vm)
        {
            var errors = new List<string>();

            // Amount validation - minimum 200, maximum 20000
            if (vm.Amount < 200)
            {
                errors.Add("Amount must be at least Rs 200");
            }
            if (vm.Amount > 20000)
            {
                errors.Add("Amount cannot exceed Rs 20,000");
            }

            // Employee validation
            if (vm.EmployeeId <= 0)
            {
                errors.Add("Please select an employee");
            }
            else
            {
                var employee = await _employeeService.GetById(vm.EmployeeId);
                if (employee == null)
                {
                    errors.Add("Selected employee does not exist");
                }
            }

            // FineOrExpenseType validation
            if (vm.FineOrExpenseTypeId <= 0)
            {
                errors.Add("Please select a fine/expense type");
            }
            else
            {
                var type = await _fineOrExpenseTypeService.GetById(vm.FineOrExpenseTypeId);
                if (type == null)
                {
                    errors.Add("Selected fine/expense type does not exist");
                }
            }

            // Description validation
            if (string.IsNullOrWhiteSpace(vm.Description))
            {
                errors.Add("Description is required");
            }
            else if (vm.Description.Length < 3)
            {
                errors.Add("Description must be at least 3 characters long");
            }
            else if (vm.Description.Length > 500)
            {
                errors.Add("Description cannot exceed 500 characters");
            }

            // Entry Date validation
            if (vm.EntryDate == default(DateTime))
            {
                errors.Add("Entry date is required");
            }
            else
            {
                var oneYearAgo = DateTime.Now.AddYears(-1);
                var oneYearFromNow = DateTime.Now.AddYears(1);
                
                if (vm.EntryDate < oneYearAgo || vm.EntryDate > oneYearFromNow)
                {
                    errors.Add("Entry date must be within the last year or next year");
                }
            }

            return (errors.Count == 0, errors);
        }

        public async Task<(bool isValid, List<string> errors)> ValidateBusinessRules(FineOrExpenseVM vm)
        {
            var errors = new List<string>();

            try
            {
                // No business rules currently enforced
                // All validation is done in ValidateModel method
            }
            catch (Exception ex)
            {
                errors.Add($"Error validating business rules: {ex.Message}");
            }

            return (errors.Count == 0, errors);
        }

        public async Task<(bool success, string message, List<FineOrExpenseVM>? data)> Add(FineOrExpenseVM vm)
        {
            try
            {
                // Validate model
                var (isModelValid, modelErrors) = await ValidateModel(vm);
                if (!isModelValid)
                {
                    return (false, string.Join(". ", modelErrors), null);
                }

                // Validate business rules
                var (isBusinessValid, businessErrors) = await ValidateBusinessRules(vm);
                if (!isBusinessValid)
                {
                    return (false, string.Join(". ", businessErrors), null);
                }

                var entity = _mapper.Map<FineOrExpense>(vm);
                await _repository.Add(entity);
                var updatedList = await GetAll();
                
                return (true, "Fine/Expense added successfully", updatedList);
            }
            catch (Exception ex)
            {
                return (false, $"Error adding Fine/Expense: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message, List<FineOrExpenseVM>? data)> Edit(FineOrExpenseVM vm)
        {
            try
            {
                // Validate model
                var (isModelValid, modelErrors) = await ValidateModel(vm);
                if (!isModelValid)
                {
                    return (false, string.Join(". ", modelErrors), null);
                }

                // Validate business rules
                var (isBusinessValid, businessErrors) = await ValidateBusinessRules(vm);
                if (!isBusinessValid)
                {
                    return (false, string.Join(". ", businessErrors), null);
                }

                var entity = _mapper.Map<FineOrExpense>(vm);
                await _repository.Edit(entity);
                var updatedList = await GetAll();
                
                return (true, "Fine/Expense updated successfully", updatedList);
            }
            catch (Exception ex)
            {
                return (false, $"Error updating Fine/Expense: {ex.Message}", null);
            }
        }

        public async Task<(bool success, string message)> Delete(int id)
        {
            try
            {
                await _repository.Delete(id);
                return (true, "Fine/Expense deleted successfully");
            }
            catch (Exception ex)
            {
                return (false, $"Error deleting Fine/Expense: {ex.Message}");
            }
        }

        public async Task<object> GetFineOrExpensesData(IFormCollection form)
        {
            var draw = form["draw"].FirstOrDefault();
            var start = int.TryParse(form["start"].FirstOrDefault(), out int s) ? s : 0;
            var length = int.TryParse(form["length"].FirstOrDefault(), out int l) ? l : 10;
            var searchValue = form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = form["order[0][column]"].FirstOrDefault();
            var sortDirection = form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);
            string[] columnNames = { "EmployeeName", "FineOrExpenseTypeName", "Amount", "Description", "EntryDate" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : "EntryDate";

            var fineOrExpenses = await GetAll();
            var query = fineOrExpenses.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.EmployeeName ?? "").ToLower().Contains(lower) ||
                    (x.FineOrExpenseTypeName ?? "").ToLower().Contains(lower) ||
                    (x.Description ?? "").ToLower().Contains(lower) ||
                    x.Amount.ToString().Contains(searchValue));
            }

            var recordsFiltered = query.Count();
            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            query = sortColumn switch
            {
                "EmployeeName" => ascending ? query.OrderBy(x => x.EmployeeName) : query.OrderByDescending(x => x.EmployeeName),
                "FineOrExpenseTypeName" => ascending ? query.OrderBy(x => x.FineOrExpenseTypeName) : query.OrderByDescending(x => x.FineOrExpenseTypeName),
                "Amount" => ascending ? query.OrderBy(x => x.Amount) : query.OrderByDescending(x => x.Amount),
                "Description" => ascending ? query.OrderBy(x => x.Description) : query.OrderByDescending(x => x.Description),
                "EntryDate" => ascending ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate),
                _ => ascending ? query.OrderBy(x => x.EntryDate) : query.OrderByDescending(x => x.EntryDate)
            };

            var pageData = query.Skip(start).Take(length).Select(x => new
            {
                employeeName = x.EmployeeName,
                fineOrExpenseTypeName = x.FineOrExpenseTypeName,
                amount = x.Amount,
                description = x.Description,
                entryDate = x.EntryDate,
                id = x.Id
            }).ToList();

            return new
            {
                draw,
                recordsTotal,
                recordsFiltered,
                data = pageData
            };
        }
    }
}