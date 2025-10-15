-- Check employee pictures in database
SELECT 
    EmployeeId,
    Name,
    PicturePath,
    CASE 
        WHEN PicturePath IS NULL THEN 'NULL'
        WHEN PicturePath = '' THEN 'EMPTY STRING'
        WHEN PicturePath LIKE '/uploads/employees/%' THEN 'UPLOADED FILE'
        WHEN PicturePath LIKE '/images/%' THEN 'DEFAULT IMAGE'
        ELSE 'OTHER: ' + PicturePath
    END as PathType
FROM Employees
ORDER BY EmployeeId DESC;

-- Count of different picture path types
SELECT 
    CASE 
        WHEN PicturePath IS NULL THEN 'NULL'
        WHEN PicturePath = '' THEN 'EMPTY STRING'
        WHEN PicturePath LIKE '/uploads/employees/%' THEN 'UPLOADED FILE'
        WHEN PicturePath LIKE '/images/%' THEN 'DEFAULT IMAGE'
        ELSE 'OTHER'
    END as PathType,
    COUNT(*) as Count
FROM Employees
GROUP BY 
    CASE 
        WHEN PicturePath IS NULL THEN 'NULL'
        WHEN PicturePath = '' THEN 'EMPTY STRING'
        WHEN PicturePath LIKE '/uploads/employees/%' THEN 'UPLOADED FILE'
        WHEN PicturePath LIKE '/images/%' THEN 'DEFAULT IMAGE'
        ELSE 'OTHER'
    END
ORDER BY Count DESC;