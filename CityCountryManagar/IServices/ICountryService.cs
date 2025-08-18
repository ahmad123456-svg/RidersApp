using RidersApp.ViewModels;

namespace RidersApp.IServices
{
    public interface ICountryService
    {
        Task<List<CountryVM>> GetAll();
        Task<CountryVM> GetById(int id);
        Task<List<CountryVM>> Add(CountryVM country);
        Task<List<CountryVM>> Edit(CountryVM country);
        Task<List<CountryVM>> Delete(int id);
    }
}
