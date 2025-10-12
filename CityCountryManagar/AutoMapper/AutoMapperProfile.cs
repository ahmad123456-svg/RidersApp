using AutoMapper;
using RidersApp.DbModels;
using RidersApp.ViewModels;

namespace RidersApp.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Example mappings
            CreateMap<City, CityVM>().ReverseMap();
            CreateMap<Country, CountryVM>().ReverseMap();
            CreateMap<DailyRides, DailyRidesVM>().ReverseMap();
            CreateMap<Employee, EmployeeVM>().ReverseMap();
            CreateMap<Configuration, ConfigurationVM>().ReverseMap();
            
            // Add new mapping for FineOrExpenseType
            CreateMap<FineOrExpenseType, FineOrExpenseTypeVM>().ReverseMap();
            
            // Add new mapping for FineOrExpense
            CreateMap<FineOrExpense, FineOrExpenseVM>()
                .ForMember(dest => dest.EmployeeName, opt => opt.MapFrom(src => src.Employee.Name))
                .ForMember(dest => dest.FineOrExpenseTypeName, opt => opt.MapFrom(src => src.FineOrExpenseType.Name));
            
            CreateMap<FineOrExpenseVM, FineOrExpense>();
        }
    }
}
