# Fitness Club — desktop app

Avalonia UI desktop application for fitness club management: clients, trainers, staff, memberships, group classes, bookings, visits, notifications, and reports.

## Features

- **Roles:** Admin, Reception, Trainer, Client — separate windows and permissions
- **Clients & memberships** — sell, freeze, extend, history
- **Group classes & schedule** — trainers, halls, time slots, conflict checks
- **Bookings & visits** — mark attendance, expected visits
- **References** — membership types, halls, specializations (admin)
- **Reports** — preview and export
- **Notifications** — in-app for users

## Stack

- .NET 8, Avalonia UI 11
- Entity Framework Core 8
- PostgreSQL

## Database setup

1. Create database `Fitness_Club` in PostgreSQL.
2. Run SQL scripts from `database/` **in numeric order** (01 → 13):

```bash
psql -U YOUR_USER -d postgres -f database/01_create_database.sql
psql -U YOUR_USER -d Fitness_Club -f database/02_schema.sql
psql -U YOUR_USER -d Fitness_Club -f database/03_seed.sql
# ... 04 through 13
```

Or all at once (after step 01 created the DB):

```bash
for f in database/0*.sql database/1*.sql; do
  psql -U YOUR_USER -d Fitness_Club -f "$f"
done
```

## Connection string

The app reads the connection in this order:

1. Environment variable `FITNESS_CLUB_CONNECTION`
2. File `Fitness_Club_01/connection.local.txt` (not in git)
3. Placeholder in code (change before first run)

**Local setup:**

```bash
cp Fitness_Club_01/connection.local.txt.example Fitness_Club_01/connection.local.txt
# edit with your PostgreSQL user and password
```

Or export once per terminal:

```bash
export FITNESS_CLUB_CONNECTION="Host=localhost;Port=5432;Database=Fitness_Club;Username=YOUR_USER;Password=YOUR_PASSWORD"
```

## Run

```bash
cd Fitness_Club_01
dotnet restore
dotnet run
```

Open in Rider: `Fitness_Club_01.sln`.

## Demo logins

After running seed scripts (`03_seed.sql` and later):

| Role | Login | Password |
|------|-------|----------|
| Admin | `admin_fc` | `Admin_fc1!` |
| Reception | `reception_fc` | `Reception1!` |
| Trainer | `trainer_volkova` | `Trainer1!v` |
| Client | `client_ivanov` | `Client1!iv` |

## Project structure

```
fitness_club/
├── Fitness_Club_01/     # Avalonia app
│   ├── Views/           # windows (Admin, Reception, Trainer, Client)
│   ├── Controls/        # reusable UI panels
│   ├── Services/        # business logic, validators
│   ├── Data/            # EF Core entities & DbContext
│   └── Models/          # grid/display rows
├── database/            # PostgreSQL scripts (01–13)
└── README.md
```

## Author

GitHub: [Development-Bulat](https://github.com/Development-Bulat)
