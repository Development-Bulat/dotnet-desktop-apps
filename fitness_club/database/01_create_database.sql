-- Создание БД Fitness_Club (выполнять подключением к postgres)
-- psql -h localhost -p 5432 -U YOUR_USER -d postgres -f DatabaseAssets/fitness_club/01_create_database.sql

SELECT pg_terminate_backend(pid)
FROM pg_stat_activity
WHERE datname = 'Fitness_Club' AND pid <> pg_backend_pid();

DROP DATABASE IF EXISTS "Fitness_Club";

CREATE DATABASE "Fitness_Club"
    WITH ENCODING = 'UTF8'
    LC_COLLATE = 'C'
    LC_CTYPE = 'C'
    TEMPLATE = template0;
