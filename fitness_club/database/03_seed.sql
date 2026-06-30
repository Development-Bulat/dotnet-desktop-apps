-- Начальные данные Fitness_Club
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/03_seed.sql

INSERT INTO "UserRole" ("RoleName")
VALUES
    ('Администратор'),
    ('Ресепшн'),
    ('Тренер'),
    ('Клиент')
ON CONFLICT ("RoleName") DO NOTHING;

INSERT INTO "Specialization" ("SpecializationName")
VALUES
    ('Йога'),
    ('Пилатес'),
    ('Кроссфит'),
    ('Силовые тренировки'),
    ('Стретчинг'),
    ('Аэробика')
ON CONFLICT ("SpecializationName") DO NOTHING;

INSERT INTO "DayOfWeek" ("DayName", "DayNumber")
VALUES
    ('Понедельник', 1),
    ('Вторник', 2),
    ('Среда', 3),
    ('Четверг', 4),
    ('Пятница', 5),
    ('Суббота', 6),
    ('Воскресенье', 7)
ON CONFLICT ("DayName") DO NOTHING;

INSERT INTO "MembershipStatus" ("StatusCode", "StatusName")
VALUES
    ('active', 'Активен'),
    ('frozen', 'Заморожен'),
    ('expired', 'Истёк'),
    ('cancelled', 'Отменён')
ON CONFLICT ("StatusCode") DO NOTHING;

INSERT INTO "MembershipType" ("TypeName", "Price", "DurationDays", "VisitLimit")
VALUES
    ('Месяц безлимит', 3500.00, 30, NULL),
    ('3 месяца безлимит', 9000.00, 90, NULL),
    ('12 посещений', 4200.00, 60, 12),
    ('Разовое посещение', 500.00, 1, 1)
ON CONFLICT ("TypeName") DO NOTHING;

INSERT INTO "GymHall" ("HallName", "Capacity")
VALUES
    ('Зал групповых программ', 25),
    ('Зал йоги', 15),
    ('Зал кроссфита', 20),
    ('Тренажёрный зал', 40)
ON CONFLICT ("HallName") DO NOTHING;

-- Демо-персонал
INSERT INTO "Staff" ("LastName", "FirstName", "Patronymic", "Phone", "HiredAt")
VALUES
    ('Смирнова', 'Елена', 'Андреевна', '+79001110001', '2024-01-15'),
    ('Козлов', 'Дмитрий', 'Игоревич', '+79001110002', '2024-03-01')
ON CONFLICT ("Phone") DO NOTHING;

INSERT INTO "Trainer" ("LastName", "FirstName", "Patronymic", "Phone", "HiredAt")
VALUES
    ('Волкова', 'Анна', 'Сергеевна', '+79002220001', '2023-06-01'),
    ('Петров', 'Максим', 'Олегович', '+79002220002', '2023-09-10'),
    ('Соколова', 'Мария', 'Викторовна', '+79002220003', '2024-02-20')
ON CONFLICT ("Phone") DO NOTHING;

INSERT INTO "TrainerSpecialization" ("IdTrainer", "IdSpecialization")
SELECT t."IdTrainer", s."IdSpecialization"
FROM "Trainer" t
JOIN "Specialization" s ON (
    (t."LastName" = 'Волкова' AND s."SpecializationName" IN ('Йога', 'Стретчинг'))
    OR (t."LastName" = 'Петров' AND s."SpecializationName" IN ('Кроссфит', 'Силовые тренировки'))
    OR (t."LastName" = 'Соколова' AND s."SpecializationName" IN ('Пилатес', 'Аэробика'))
)
ON CONFLICT DO NOTHING;

INSERT INTO "Client" ("LastName", "FirstName", "Patronymic", "Phone", "BirthDate")
VALUES
    ('Иванов', 'Алексей', 'Петрович', '+79003330001', '1995-04-12'),
    ('Кузнецова', 'Ольга', 'Николаевна', '+79003330002', '1990-08-25'),
    ('Новиков', 'Игорь', NULL, '+79003330003', '1988-11-03')
ON CONFLICT ("Phone") DO NOTHING;

-- Учётные записи (пароль соответствует правилам валидации приложения)
INSERT INTO "UserAccount" ("Login", "PasswordHash", "IdUserRole", "IdStaff", "IdTrainer", "IdClient")
SELECT 'admin_fc', 'Admin_fc1!', r."IdUserRole", s."IdStaff", NULL, NULL
FROM "UserRole" r
JOIN "Staff" s ON s."LastName" = 'Смирнова'
WHERE r."RoleName" = 'Администратор'
ON CONFLICT ("Login") DO NOTHING;

INSERT INTO "UserAccount" ("Login", "PasswordHash", "IdUserRole", "IdStaff", "IdTrainer", "IdClient")
SELECT 'reception_fc', 'Reception1!', r."IdUserRole", s."IdStaff", NULL, NULL
FROM "UserRole" r
JOIN "Staff" s ON s."LastName" = 'Козлов'
WHERE r."RoleName" = 'Ресепшн'
ON CONFLICT ("Login") DO NOTHING;

INSERT INTO "UserAccount" ("Login", "PasswordHash", "IdUserRole", "IdStaff", "IdTrainer", "IdClient")
SELECT 'trainer_volkova', 'Trainer1!v', r."IdUserRole", NULL, t."IdTrainer", NULL
FROM "UserRole" r
JOIN "Trainer" t ON t."LastName" = 'Волкова'
WHERE r."RoleName" = 'Тренер'
ON CONFLICT ("Login") DO NOTHING;

INSERT INTO "UserAccount" ("Login", "PasswordHash", "IdUserRole", "IdStaff", "IdTrainer", "IdClient")
SELECT 'client_ivanov', 'Client1!iv', r."IdUserRole", NULL, NULL, c."IdClient"
FROM "UserRole" r
JOIN "Client" c ON c."LastName" = 'Иванов'
WHERE r."RoleName" = 'Клиент'
ON CONFLICT ("Login") DO NOTHING;

-- Групповые занятия
INSERT INTO "GroupClass" ("ClassName", "IdGymHall", "IdTrainer", "IdDayOfWeek", "StartTime", "DurationMinutes", "MaxParticipants")
SELECT
    v.class_name,
    h."IdGymHall",
    t."IdTrainer",
    d."IdDayOfWeek",
    v.start_time::time,
    v.duration_min,
    v.max_part
FROM (VALUES
    ('Йога для начинающих', 'Зал йоги', 'Волкова', 'Понедельник', '18:00', 60, 15),
    ('Кроссфит WOD', 'Зал кроссфита', 'Петров', 'Среда', '19:00', 55, 20),
    ('Пилатес', 'Зал групповых программ', 'Соколова', 'Пятница', '10:00', 50, 25)
) AS v(class_name, hall_name, trainer_last, day_name, start_time, duration_min, max_part)
JOIN "GymHall" h ON h."HallName" = v.hall_name
JOIN "Trainer" t ON t."LastName" = v.trainer_last
JOIN "DayOfWeek" d ON d."DayName" = v.day_name
WHERE NOT EXISTS (
    SELECT 1 FROM "GroupClass" gc WHERE gc."ClassName" = v.class_name
);

-- Демо-абонемент
INSERT INTO "Membership" ("IdClient", "IdMembershipType", "StartDate", "EndDate", "IdMembershipStatus", "IdSoldByUser")
SELECT
    c."IdClient",
    mt."IdMembershipType",
    CURRENT_DATE - 5,
    CURRENT_DATE + 25,
    ms."IdMembershipStatus",
    u."IdUserAccount"
FROM "Client" c
JOIN "MembershipType" mt ON mt."TypeName" = 'Месяц безлимит'
JOIN "MembershipStatus" ms ON ms."StatusCode" = 'active'
JOIN "UserAccount" u ON u."Login" = 'reception_fc'
WHERE c."LastName" = 'Иванов'
  AND NOT EXISTS (
      SELECT 1 FROM "Membership" m
      WHERE m."IdClient" = c."IdClient" AND m."IdMembershipStatus" = ms."IdMembershipStatus"
  );

INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "IdChangedByUser", "Comment")
SELECT m."IdMembership", m."IdMembershipStatus", m."IdSoldByUser", 'Оформление абонемента'
FROM "Membership" m
JOIN "Client" c ON c."IdClient" = m."IdClient"
WHERE c."LastName" = 'Иванов'
  AND NOT EXISTS (
      SELECT 1 FROM "MembershipStatusHistory" h WHERE h."IdMembership" = m."IdMembership"
  );
