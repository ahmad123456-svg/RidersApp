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
    public class FineOrExpenseTypeService : IFineOrExpenseTypeService
    {
        private readonly IFineOrExpenseTypeRepository _repository;
        private readonly IMapper _mapper;

        public FineOrExpenseTypeService(IFineOrExpenseTypeRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<List<FineOrExpenseTypeVM>> GetAll()
        {
            var entities = await _repository.GetAll();
            return _mapper.Map<List<FineOrExpenseTypeVM>>(entities);
        }

        public async Task<FineOrExpenseTypeVM> GetById(int id)
        {
            var entity = await _repository.GetById(id);
            return _mapper.Map<FineOrExpenseTypeVM>(entity);
        }

        public async Task<List<FineOrExpenseTypeVM>> Add(FineOrExpenseTypeVM vm)
        {
            // Check for duplicate name
            if (await _repository.Exists(vm.Name))
            {
                throw new Exception($"A Fine/Expense Type with name '{vm.Name}' already exists.");
            }

            var entity = _mapper.Map<FineOrExpenseType>(vm);
            await _repository.Add(entity);
            
            return await GetAll();
        }

        public async Task<List<FineOrExpenseTypeVM>> Edit(FineOrExpenseTypeVM vm)
        {
            // Check for duplicate name
            if (await _repository.Exists(vm.Name, vm.Id))
            {
                throw new Exception($"A Fine/Expense Type with name '{vm.Name}' already exists.");
            }

            var entity = _mapper.Map<FineOrExpenseType>(vm);
            await _repository.Update(entity);
            
            return await GetAll();
        }

        public async Task Delete(int id)
        {
            await _repository.Delete(id);
        }

        public async Task<object> GetFineOrExpenseTypesData(IFormCollection form)
        {
            var draw = form["draw"].FirstOrDefault();
            var start = int.TryParse(form["start"].FirstOrDefault(), out int s) ? s : 0;
            var length = int.TryParse(form["length"].FirstOrDefault(), out int l) ? l : 10;
            var searchValue = form["search[value]"].FirstOrDefault()?.Trim();
            var sortColumnIndexString = form["order[0][column]"].FirstOrDefault();
            var sortDirection = form["order[0][dir]"].FirstOrDefault();

            int.TryParse(sortColumnIndexString, out int sortColumnIndex);
            string[] columnNames = { "Name" };
            string sortColumn = (sortColumnIndex >= 0 && sortColumnIndex < columnNames.Length)
                ? columnNames[sortColumnIndex]
                : "Name";

            var fineOrExpenseTypes = await GetAll();
            var query = fineOrExpenseTypes.AsQueryable();

            var recordsTotal = query.Count();

            if (!string.IsNullOrWhiteSpace(searchValue))
            {
                var lower = searchValue.ToLower();
                query = query.Where(x =>
                    (x.Name ?? "").ToLower().Contains(lower));
            }

            var recordsFiltered = query.Count();
            bool ascending = string.Equals(sortDirection, "asc", StringComparison.OrdinalIgnoreCase);

            query = sortColumn switch
            {
                "Name" => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name),
                _ => ascending ? query.OrderBy(x => x.Name) : query.OrderByDescending(x => x.Name)
            };

            var pageData = query.Skip(start).Take(length).Select(x => new
            {
                name = x.Name,
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