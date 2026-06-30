-- Приведение демо-данных к правилам валидации приложения
-- Пароль: 8–16 символов, заглавная, строчная, цифра, спецсимвол
-- Возраст клиентов: не младше 18 лет
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/07_fix_validation_data.sql

BEGIN;

-- Основные демо-учётки
UPDATE "UserAccount" SET "PasswordHash" = 'Admin_fc1!'    WHERE "Login" = 'admin_fc';
UPDATE "UserAccount" SET "PasswordHash" = 'Reception1!'   WHERE "Login" = 'reception_fc';
UPDATE "UserAccount" SET "PasswordHash" = 'Trainer1!v'    WHERE "Login" = 'trainer_volkova';
UPDATE "UserAccount" SET "PasswordHash" = 'Client1!iv'    WHERE "Login" = 'client_ivanov';

-- Сгенерированные учётки: пароль Client{id}!1 / Staff{id}!1 / Trainer{id}!1
UPDATE "UserAccount" u
SET "PasswordHash" = 'Client' || u."IdClient"::text || '!1'
WHERE u."Login" ~ '^client_[0-9]+$'
  AND u."IdClient" IS NOT NULL;

UPDATE "UserAccount" u
SET "PasswordHash" = 'Staff' || u."IdStaff"::text || '!1'
WHERE u."Login" LIKE 'staff_%'
  AND u."IdStaff" IS NOT NULL;

UPDATE "UserAccount" u
SET "PasswordHash" = 'Trainer' || u."IdTrainer"::text || '!1'
WHERE u."Login" ~ '^trainer_[0-9]+$'
  AND u."IdTrainer" IS NOT NULL;

-- Сброс хешей PBKDF2 (если успели сохраниться при тестах)
UPDATE "UserAccount"
SET "PasswordHash" = 'Demo1!pass'
WHERE "PasswordHash" LIKE 'PBKDF2:%';

-- Клиенты без даты рождения — проставить 18+ лет
UPDATE "Client"
SET "BirthDate" = DATE '1995-06-15'
WHERE "BirthDate" IS NULL;

-- Клиенты младше 18 лет на текущую дату
UPDATE "Client"
SET "BirthDate" = CURRENT_DATE - INTERVAL '25 years'
WHERE "BirthDate" > CURRENT_DATE - INTERVAL '18 years';

COMMIT;
