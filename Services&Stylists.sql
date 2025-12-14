/* =========================
   Seed Stylists + Services
   - Inserts only if missing
   - Updates service prices if they already exist
   ========================= */

BEGIN TRAN;

-------------------------
-- STYLISTS (add missing)
-------------------------
INSERT INTO dbo.Stylists (StylistName)
SELECT v.StylistName
FROM (VALUES
    ('Alex'),
    ('Jordan'),
    ('Sam')
) v(StylistName)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Stylists s
    WHERE s.StylistName = v.StylistName
);

-------------------------
-- SERVICES (upsert)
-- Your table: dbo.Services (Id, Name, Price)
-------------------------

-- 1) Insert any missing services (with price)
INSERT INTO dbo.Services (Name, Price)
SELECT v.Name, v.Price
FROM (VALUES
    ('Haircut',       25),
    ('Hair Styling',  40),
    ('Hair Coloring', 80),
    ('Beard Trim',    15)
) v(Name, Price)
WHERE NOT EXISTS (
    SELECT 1
    FROM dbo.Services s
    WHERE s.Name = v.Name
);

-- 2) Ensure prices are correct (fills NULLs or fixes wrong values)
UPDATE s
SET s.Price = v.Price
FROM dbo.Services s
JOIN (VALUES
    ('Haircut',       25),
    ('Hair Styling',  40),
    ('Hair Coloring', 80),
    ('Beard Trim',    15)
) v(Name, Price)
  ON s.Name = v.Name
WHERE s.Price IS NULL OR s.Price <> v.Price;

COMMIT;

-- Quick check
SELECT StylistID, StylistName FROM dbo.Stylists ORDER BY StylistID;
SELECT Id, Name, Price FROM dbo.Services ORDER BY Id;
