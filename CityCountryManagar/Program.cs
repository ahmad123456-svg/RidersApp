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

// Identity setup
builder.Services.AddDefaultIdentity<RidersAppUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
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

// ✅ Admin Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<RidersAppUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string adminEmail = "admin@site.com";
    string adminPassword = "Admin@123";

    // Create role if missing
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("User"))
    {
        await roleManager.CreateAsync(new IdentityRole("User"));
    }

    // Create user if missing
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        var user = new RidersAppUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "Admin",
            Role = "Admin",
            LastName = "User"        // ✅ Required to prevent NULL exception
        };

        var result = await userManager.CreateAsync(user, adminPassword);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }
    }
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
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
