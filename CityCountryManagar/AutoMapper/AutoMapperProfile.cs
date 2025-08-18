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
        }
    }
}
