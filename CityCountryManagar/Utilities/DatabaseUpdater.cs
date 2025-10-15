using Microsoft.EntityFrameworkCore;
using RidersApp.Data;

namespace RidersApp.Utilities
{
    public static class DatabaseUpdater
    {
        public static async Task UpdateEmployeePicturesAsync(ApplicationDbContext context)
        {
            try
            {
                Console.WriteLine("Starting employee picture update...");
                
                // Only update employees with NULL pictures - don't override existing uploads
                var employeesWithNullPictures = await context.Employees
                    .Where(e => e.PictureUrl == null)
                    .ToListAsync();

                if (employeesWithNullPictures.Any())
                {
                    foreach (var employee in employeesWithNullPictures)
                    {
                        employee.PictureUrl = "/images/default-profile.png";
                    }

                    await context.SaveChangesAsync();
                    Console.WriteLine($"Updated {employeesWithNullPictures.Count} employee pictures to default image.");
                }
                else
                {
                    Console.WriteLine("No employees found with NULL pictures.");
                }

                // Show all employee pictures for debugging
                var allEmployees = await context.Employees
                    .Select(e => new { e.EmployeeId, e.Name, e.PictureUrl })
                    .OrderBy(e => e.EmployeeId)
                    .ToListAsync();

                Console.WriteLine("\nCurrent employee pictures:");
                foreach (var emp in allEmployees)
                {
                    Console.WriteLine($"ID: {emp.EmployeeId}, Name: {emp.Name}, Picture: {emp.PictureUrl ?? "NULL"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating employee pictures: {ex.Message}");
            }
        }
    }
}