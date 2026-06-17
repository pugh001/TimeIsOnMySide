If Flyway is used to apply database changes via the pipeline use filename format. V0.0.0.0__description.sql

Flyway will load the scripts folder and loop through the files and apply files parked with V & R. If a V fails the corresponding U will be executed. 
Flyway will create a table holding the version of the database or ... last script version that was applied.

*All sql statements in scripts must be Idempotent.*

V/U/R - is instruction and must be capital. 
    V is the indication that a new version is being applied. This file will be applied once. Example : V0.0.0.0__desc.sql
    U Undo migrations are the opposite of regular versioned migrations. An undo migration is responsible for undoing the effects of the versioned migration with the same version. Undo migrations are optional and not required to run regular versioned migrations. Example U0.0.0.0__desc.sql
    R is for repeatable scripts - that can be repeated for every deployment. Note it does not require a version. Example R__desc.sql
0.0.0.0 - semantic version number for use with V and U.
__ - 2 x Separator between the instruction version and description. 
Description must use "_" underscores in filename instead of spaces.
sql - file extension can be other types but we will use sql for this service.

For more info read:
https://www.red-gate.com/blog/database-devops/flyway-naming-patterns-matter
https://documentation.red-gate.com/fd/migrations-184127470.html