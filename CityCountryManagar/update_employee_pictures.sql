-- Update existing employees with NULL pictures to have a default picture
UPDATE Employees 
SET Picture = '/Image/download.png' 
WHERE Picture IS NULL OR Picture = '';

-- Verify the update
SELECT EmployeeId, Name, Picture 
FROM Employees 
WHERE Picture IS NOT NULL;

-- Optional: Check if any employees still have NULL pictures
SELECT COUNT(*) as NullPictureCount 
FROM Employees 
WHERE Picture IS NULL OR Picture = '';