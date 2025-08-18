using RidersApp.Areas.Identity.Data;
using RidersApp.DbModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace RidersApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<RidersAppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        //  Your application models
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<DailyRides> DailyRides { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Required for Identity
            base.OnModelCreating(modelBuilder);

            //  Custom IdentityUser configuration
            modelBuilder.ApplyConfiguration(new ApplicationUserEntityConfiguration());

            //  Configure Country-City relationship
            modelBuilder.Entity<Country>()
                .HasMany(c => c.Cities)
                .WithOne(c => c.Country)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            //  Configure City-Employee relationship
            modelBuilder.Entity<City>()
                .HasMany(c => c.Employees)
                .WithOne(e => e.City)
                .HasForeignKey(e => e.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            //  Configure Country-Employee relationship
            modelBuilder.Entity<Country>()
                .HasMany(c => c.Employees)
                .WithOne(e => e.Country)
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure decimal precision for DailyRides
            modelBuilder.Entity<DailyRides>()
                .Property(d => d.CashAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DailyRides>()
                .Property(d => d.CashWAT)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DailyRides>()
                .Property(d => d.CreditAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DailyRides>()
                .Property(d => d.CreditWAT)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DailyRides>()
                .Property(d => d.Expense)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DailyRides>()
                .Property(d => d.LessAmount)
                .HasPrecision(18, 2);
            modelBuilder.Entity<DailyRides>()
                .Property(d => d.OverRidesAmount)
                .HasPrecision(18, 2);

            //  Configure Employee-DailyRides relationship with cascade delete
            modelBuilder.Entity<DailyRides>()
                .HasOne(d => d.Employee)
                .WithMany(e => e.DailyRides)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes for better performance
            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.CountryId);

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.CityId);

            modelBuilder.Entity<City>()
                .HasIndex(c => c.CountryId);

            modelBuilder.Entity<DailyRides>()
                .HasIndex(d => d.EmployeeId);
        }
    }

    //  IdentityUser extension config
    public class ApplicationUserEntityConfiguration : IEntityTypeConfiguration<RidersAppUser>
    {
        public void Configure(EntityTypeBuilder<RidersAppUser> builder)
        {
            builder.Property(x => x.FirstName).HasMaxLength(255);
            builder.Property(x => x.LastName).HasMaxLength(255);

            builder.Property(x => x.Role)
             .HasMaxLength(50);
        }
    }
}
