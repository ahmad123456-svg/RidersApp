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
                
                // Update all employees with NULL or empty pictures
                var employeesWithNullPictures = await context.Employees
                    .Where(e => e.Picture == null || e.Picture == "")
                    .ToListAsync();

                if (employeesWithNullPictures.Any())
                {
                    foreach (var employee in employeesWithNullPictures)
                    {
                        employee.Picture = "/Image/download.png";
                    }

                    await context.SaveChangesAsync();
                    Console.WriteLine($"Updated {employeesWithNullPictures.Count} employee pictures to default image.");
                }
                else
                {
                    Console.WriteLine("No employees found with NULL pictures.");
                }

                // Verify the update
                var remainingNullPictures = await context.Employees
                    .CountAsync(e => e.Picture == null || e.Picture == "");
                
                Console.WriteLine($"Employees with NULL pictures after update: {remainingNullPictures}");
                
                // Show all employee pictures
                var allEmployees = await context.Employees
                    .Select(e => new { e.EmployeeId, e.Name, e.Picture })
                    .OrderBy(e => e.EmployeeId)
                    .ToListAsync();

                Console.WriteLine("\nCurrent employee pictures:");
                foreach (var emp in allEmployees)
                {
                    Console.WriteLine($"ID: {emp.EmployeeId}, Name: {emp.Name}, Picture: {emp.Picture ?? "NULL"}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating employee pictures: {ex.Message}");
                throw;
            }
        }
    }
}