using System;
using System.Linq;
using RidersApp.Data;
using RidersApp.DbModels;
using RidersApp.Interfaces;
using RidersApp.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using RidersApp.Areas.Identity.Data;
using RidersApp.IServices;
using RidersApp.Services; // Your custom user class

var builder = WebApplication.CreateBuilder(args);

// Database connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity setup with password options - ✅ FIXED: Made more flexible for user management
builder.Services.AddDefaultIdentity<RidersAppUser>(options =>
{
    // ✅ CRITICAL FIX: Set to false to allow admin-created users to login immediately
    options.SignIn.RequireConfirmedAccount = false;
    
    // Configure password requirements - ✅ RELAXED for admin setup
    options.Password.RequiredLength = 6; // Reduced from 8 to 6 for flexibility
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 1;
    
    // User settings
    options.User.RequireUniqueEmail = true;
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    
    // Lockout settings - ✅ DISABLED for admin
    options.Lockout.AllowedForNewUsers = false;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 10;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Register repositories
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IDailyRidesRepository, DailyRidesRepository>();
builder.Services.AddScoped<ICityService, CityService>();
builder.Services.AddScoped<ICountryService, CountryService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDailyRidesService, DailyRidesService>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<IUserService, UserService>(); // ✅ Added IUserService and UserService

// Login path configuration
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
});

// MVC and Razor support
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// ✅ ENHANCED Admin Seeding with Password Reset
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<RidersAppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@site.com";
    string adminPassword = "Admin1@23";

    try
    {
        Console.WriteLine("🔧 Starting admin user setup...");

        // Create roles if missing
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            var adminRole = new IdentityRole("Admin");
            await roleManager.CreateAsync(adminRole);
            Console.WriteLine("✅ Admin role created");
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            var userRole = new IdentityRole("User");
            await roleManager.CreateAsync(userRole);
            Console.WriteLine("✅ User role created");
        }

        // Find existing admin user
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        
        if (adminUser == null)
        {
            // Create new admin user
            Console.WriteLine("🆕 Creating new admin user...");
            var user = new RidersAppUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "Admin",
                LastName = "User",
                Role = "Admin",
                LockoutEnabled = false
            };

            var result = await userManager.CreateAsync(user, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Admin");
                Console.WriteLine("✅ New admin user created successfully");
            }
            else
            {
                Console.WriteLine($"❌ Failed to create admin user:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"   - {error.Description}");
                }
            }
        }
        else
        {
            // Reset existing admin user to ensure it works
            Console.WriteLine("🔄 Updating existing admin user...");
            
            // Update user properties
            adminUser.EmailConfirmed = true;
            adminUser.LockoutEnabled = false;
            adminUser.LockoutEnd = null;
            adminUser.AccessFailedCount = 0;
            adminUser.FirstName = "Admin";
            adminUser.LastName = "User";
            adminUser.Role = "Admin";
            
            var updateResult = await userManager.UpdateAsync(adminUser);
            if (updateResult.Succeeded)
            {
                Console.WriteLine("✅ Admin user properties updated");
            }
            else
            {
                Console.WriteLine("❌ Failed to update admin user properties");
            }

            // Reset password to ensure it's correct
            var token = await userManager.GeneratePasswordResetTokenAsync(adminUser);
            var passwordResult = await userManager.ResetPasswordAsync(adminUser, token, adminPassword);
            if (passwordResult.Succeeded)
            {
                Console.WriteLine("✅ Admin password reset successfully");
            }
            else
            {
                Console.WriteLine("❌ Failed to reset admin password:");
                foreach (var error in passwordResult.Errors)
                {
                    Console.WriteLine($"   - {error.Description}");
                }
            }

            // Ensure admin role is assigned
            var roles = await userManager.GetRolesAsync(adminUser);
            if (!roles.Contains("Admin"))
            {
                if (roles.Any())
                {
                    await userManager.RemoveFromRolesAsync(adminUser, roles);
                }
                await userManager.AddToRoleAsync(adminUser, "Admin");
                Console.WriteLine("✅ Admin role assigned");
            }
            else
            {
                Console.WriteLine("✅ Admin role already assigned");
            }
        }

        // Final verification
        var finalAdminUser = await userManager.FindByEmailAsync(adminEmail);
        if (finalAdminUser != null)
        {
            Console.WriteLine($"✅ Final admin user status:");
            Console.WriteLine($"   - Email: {finalAdminUser.Email}");
            Console.WriteLine($"   - EmailConfirmed: {finalAdminUser.EmailConfirmed}");
            Console.WriteLine($"   - LockoutEnabled: {finalAdminUser.LockoutEnabled}");
            Console.WriteLine($"   - LockoutEnd: {finalAdminUser.LockoutEnd}");
            Console.WriteLine($"   - AccessFailedCount: {finalAdminUser.AccessFailedCount}");
            
            var finalRoles = await userManager.GetRolesAsync(finalAdminUser);
            Console.WriteLine($"   - Roles: {string.Join(", ", finalRoles)}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"❌ Error during admin setup: {ex.Message}");
        Console.WriteLine($"   Stack trace: {ex.StackTrace}");
    }
}

// Ensure Configurations table exists (idempotent creation to avoid runtime errors
// when the EF migration hasn't been applied yet). This executes a safe
// CREATE TABLE IF NOT EXISTS style check for SQL Server.
using (var scope2 = app.Services.CreateScope())
{
    try
    {
        var services = scope2.ServiceProvider;
        var db = services.GetRequiredService<ApplicationDbContext>();
        var createSql = @"
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Configurations]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[Configurations](
        [ConfigurationId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        [KeyName] NVARCHAR(200) NOT NULL,
        [Value] NVARCHAR(2000) NULL
    );
END
";
        db.Database.ExecuteSqlRaw(createSql);
    }
    catch (Exception ex)
    {
        // If table creation fails (permissions/database offline), we don't want
        // to crash the app here. Log to console for visibility.
        Console.WriteLine("Warning: could not ensure Configurations table exists: " + ex.Message);
    }
}

// Seed default configuration values (CreditWAT and CashWAT) if missing
using (var scope3 = app.Services.CreateScope())
{
    try
    {
        var services = scope3.ServiceProvider;
        var configService = services.GetRequiredService<RidersApp.IServices.IConfigurationService>();

        // Helper to ensure a configuration exists with given key and value
        async Task EnsureConfig(string key, string value)
        {
            var all = await configService.GetAll();
            if (!all.Any(c => string.Equals(c.KeyName, key, StringComparison.OrdinalIgnoreCase)))
            {
                await configService.Add(new RidersApp.ViewModels.ConfigurationVM { KeyName = key, Value = value });
            }
        }

        // Seed the two values requested
        await EnsureConfig("CreditWAT", "10");
        await EnsureConfig("CashWAT", "10");
    }
    catch (Exception ex)
    {
        Console.WriteLine("Warning: could not seed Configurations: " + ex.Message);
    }
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Redirect to login page if not authenticated
app.Use(async (context, next) =>
{
    if (!context.User.Identity.IsAuthenticated && !context.Request.Path.StartsWithSegments("/Identity/Account/Login") && !context.Request.Path.StartsWithSegments("/Identity/Account/Register"))
    {
        context.Response.Redirect("/Identity/Account/Login");
        return;
    }
    await next();
});

// Routing
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
