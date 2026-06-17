This folder contains manual SQL migration scripts that can be applied safely to the PostgreSQL database.

2026_06_14_add_createdat.sql
- Adds `CreatedAt` TIMESTAMP WITH TIME ZONE columns to `Patients` and `Doctors` tables if they do not exist.
- Sets existing NULL values to the current UTC time and marks the column NOT NULL.

How to apply (example using psql):

PSQL:
```bash
psql "host=localhost port=5432 dbname=NutqDB user=postgres password=1234" -f backend/migrations/2026_06_14_add_createdat.sql
```

Or:

```bash
psql -h localhost -p 5432 -U postgres -d NutqDB -f backend/migrations/2026_06_14_add_createdat.sql
```

Notes:
- This script is non-destructive and does not drop or alter existing data apart from populating the new columns.
- If you use EF Core migrations in your workflow, you can also create a corresponding EF migration so the model and migrations stay in sync:

```bash
cd backend
dotnet ef migrations add AddCreatedAtToPatientAndDoctor
dotnet ef database update
```

(Ensure `dotnet-ef` is installed and your project is configured for EF Core tooling.)
