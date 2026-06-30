# Fitness Club — desktop app

Avalonia UI desktop application for fitness club management: members, trainers, schedules, memberships, and role-based access (Admin, Reception, Trainer, Client).

## Stack

- .NET 8, Avalonia UI 11
- Entity Framework Core 8
- PostgreSQL

## Database setup

1. Create database `Fitness_Club` in PostgreSQL.
2. Run SQL scripts from `database/` **in order** (01 → 12):

```bash
psql -U YOUR_USER -d Fitness_Club -f database/01_create_tables.sql
# ... repeat for 02–12
```

Or run all at once:

```bash
for f in database/*.sql; do psql -U YOUR_USER -d Fitness_Club -f "$f"; done
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
dotnet run
```

Open the solution in Rider: `Fitness_Club_01.sln`.

## Demo logins

See SQL seed data in `database/11_seed_users.sql` (passwords are hashed in DB).

## Author

[Development-Bulat](https://github.com/Development-Bulat)
