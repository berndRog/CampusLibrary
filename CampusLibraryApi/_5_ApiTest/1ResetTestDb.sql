-- to start the script  
-- sqlite3 CampusLibraryApi/CampusLibrarycd Db.db <  CampusLibraryApi/_5_ApiTests/1ResetTestDb.sql

-- Reset Database for API Tests
-- This script will delete all data from the tables and reset the auto-incrementing primary keys.    
-- Keeps the EF Core migration history table intact.

PRAGMA
foreign_keys = OFF;

DELETE
FROM "Loans";
DELETE
FROM "Readers";
DELETE
FROM "BookItems";
DELETE
FROM "Books";

PRAGMA
foreign_keys = ON;
-- End of 1ResetTestDb.sql
       

       
