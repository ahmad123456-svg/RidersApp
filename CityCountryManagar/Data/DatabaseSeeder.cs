using RidersApp.DbModels;
using Microsoft.EntityFrameworkCore;

namespace RidersApp.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            try
            {
                await context.Database.EnsureCreatedAsync();

                // Seed Countries
                if (!await context.Countries.AnyAsync())
                {
                    var countries = new List<Country>
                    {
                        new Country { Name = "United States" },
                        new Country { Name = "Canada" },
                        new Country { Name = "United Kingdom" },
                        new Country { Name = "Germany" },
                        new Country { Name = "France" },
                        new Country { Name = "Australia" },
                        new Country { Name = "Japan" },
                        new Country { Name = "India" },
                        new Country { Name = "Brazil" },
                        new Country { Name = "Mexico" }
                    };

                    await context.Countries.AddRangeAsync(countries);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Countries seeded successfully.");
                }

                // Seed Cities (after countries are saved)
                if (!await context.Cities.AnyAsync())
                {
                    var countries = await context.Countries.ToListAsync();
                    var cities = new List<City>();

                    foreach (var country in countries.Take(5)) // Add cities for first 5 countries
                    {
                        cities.AddRange(new[]
                        {
                            new City { CityName = $"City A - {country.Name}", PostalCode = "10001", CountryId = country.CountryId },
                            new City { CityName = $"City B - {country.Name}", PostalCode = "20002", CountryId = country.CountryId },
                            new City { CityName = $"City C - {country.Name}", PostalCode = "30003", CountryId = country.CountryId }
                        });
                    }

                    await context.Cities.AddRangeAsync(cities);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Cities seeded successfully.");
                }

                // Seed Employees
                if (!await context.Employees.AnyAsync())
                {
                    var countries = await context.Countries.ToListAsync();
                    var cities = await context.Cities.ToListAsync();
                    var employees = new List<Employee>();

                    for (int i = 1; i <= 20; i++)
                    {
                        var randomCountry = countries[i % countries.Count];
                        var randomCity = cities.Where(c => c.CountryId == randomCountry.CountryId).FirstOrDefault();
                        
                        if (randomCity != null)
                        {
                            employees.Add(new Employee
                            {
                                Name = $"Employee {i}",
                                FatherName = $"Father {i}",
                                PhoneNo = $"+1-555-000{i:D3}",
                                Address = $"{i} Main Street",
                                CountryId = randomCountry.CountryId,
                                CityId = randomCity.CityId,
                                Salary = 50000 + (i * 1000),
                                Vehicle = i % 2 == 0 ? "Car" : "Motorcycle",
                                VehicleNumber = $"VH-{i:D3}",
                                Picture = "/Image/download.png" // Set default picture
                            });
                        }
                    }

                    await context.Employees.AddRangeAsync(employees);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Employees seeded successfully.");
                }

                // Seed DailyRides
                if (!await context.DailyRides.AnyAsync())
                {
                    var employees = await context.Employees.ToListAsync();
                    var dailyRides = new List<DailyRides>();

                    foreach (var employee in employees.Take(10))
                    {
                        for (int day = 1; day <= 5; day++)
                        {
                            dailyRides.Add(new DailyRides
                            {
                                EmployeeId = employee.EmployeeId,
                                EntryDate = DateTime.Now.AddDays(-day),
                                CreditAmount = 100.00m + (day * 10),
                                CreditWAT = 5.00m + (day * 0.5m),
                                CashAmount = 50.00m + (day * 5),
                                CashWAT = 2.50m + (day * 0.25m),
                                Expense = 20.00m + (day * 2),
                                TodayRides = 5 + day,
                                OverRides = day,
                                OverRidesAmount = day * 5.00m,
                                TotalRides = 5 + day,
                                LessAmount = day * 1.00m,
                                InsertDate = DateTime.Now,
                                InsertedBy = "System",
                                UpdatedBy = "System"
                            });
                        }
                    }

                    await context.DailyRides.AddRangeAsync(dailyRides);
                    await context.SaveChangesAsync();
                    Console.WriteLine("DailyRides seeded successfully.");
                }

                // Seed Configurations
                if (!await context.Configurations.AnyAsync())
                {
                    var configurations = new List<Configuration>
                    {
                        new Configuration { KeyName = "CompanyName", Value = "RidersApp Transportation" },
                        new Configuration { KeyName = "CompanyAddress", Value = "123 Business Street, City, State" },
                        new Configuration { KeyName = "CompanyPhone", Value = "+1-555-COMPANY" },
                        new Configuration { KeyName = "CompanyEmail", Value = "info@ridersapp.com" },
                        new Configuration { KeyName = "Currency", Value = "USD" },
                        new Configuration { KeyName = "TimeZone", Value = "EST" },
                        new Configuration { KeyName = "DefaultPageSize", Value = "25" },
                        new Configuration { KeyName = "AllowancePerKm", Value = "0.50" }
                    };

                    await context.Configurations.AddRangeAsync(configurations);
                    await context.SaveChangesAsync();
                    Console.WriteLine("Configurations seeded successfully.");
                }

                Console.WriteLine("Database seeding completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during database seeding: {ex.Message}");
                throw;
            }
        }
    }
}
