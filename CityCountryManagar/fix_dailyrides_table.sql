-- Fix DailyRides table by making IdentityUser columns nullable
-- Run this script in your SQL Server database

USE RiderApp;

-- Make IdentityUser columns nullable
ALTER TABLE DailyRides ALTER COLUMN UserName NVARCHAR(MAX) NULL;
ALTER TABLE DailyRides ALTER COLUMN NormalizedUserName NVARCHAR(MAX) NULL;
ALTER TABLE DailyRides ALTER COLUMN Email NVARCHAR(MAX) NULL;
ALTER TABLE DailyRides ALTER COLUMN NormalizedEmail NVARCHAR(MAX) NULL;
ALTER TABLE DailyRides ALTER COLUMN EmailConfirmed BIT NULL;
ALTER TABLE DailyRides ALTER COLUMN PasswordHash NVARCHAR(MAX) NULL;
ALTER TABLE DailyRides ALTER COLUMN SecurityStamp NVARCHAR(MAX) NULL;
ALTER TABLE DailyRides ALTER COLUMN ConcurrencyStamp NVARCHAR(MAX) NULL;
ALTER TABLE DailyRides ALTER COLUMN PhoneNumber NVARCHAR(MAX) NULL;
ALTER TABLE DailyRides ALTER COLUMN PhoneNumberConfirmed BIT NULL;
ALTER TABLE DailyRides ALTER COLUMN TwoFactorEnabled BIT NULL;
ALTER TABLE DailyRides ALTER COLUMN LockoutEnd DATETIMEOFFSET NULL;
ALTER TABLE DailyRides ALTER COLUMN LockoutEnabled BIT NULL;
ALTER TABLE DailyRides ALTER COLUMN AccessFailedCount INT NULL;

-- Set default values for existing records
UPDATE DailyRides SET 
    EmailConfirmed = 0,
    PhoneNumberConfirmed = 0,
    TwoFactorEnabled = 0,
    LockoutEnabled = 0,
    AccessFailedCount = 0
WHERE EmailConfirmed IS NULL OR PhoneNumberConfirmed IS NULL OR TwoFactorEnabled IS NULL OR LockoutEnabled IS NULL OR AccessFailedCount IS NULL;

PRINT 'DailyRides table has been fixed successfully!';
