-- Массовое наполнение Fitness_Club (операционные таблицы)
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/05_seed_more_data.sql

BEGIN;

-- --------------------------------------------------
-- Client: добавим много клиентов (профили) с нормальными ФИО
-- --------------------------------------------------
INSERT INTO "Client" ("LastName", "FirstName", "Patronymic", "Phone", "BirthDate", "RegisteredAt")
SELECT
    v.last_name,
    v.first_name,
    v.patronymic,
    v.phone,
    v.birth_date,
    (now() AT TIME ZONE 'UTC') - ((v.rn % 30) || ' days')::interval
FROM (VALUES
    (1,  'Смирнов',    'Алексей',    'Дмитриевич',   '+79004440001', DATE '1992-03-14'),
    (2,  'Петрова',    'Екатерина',  'Сергеевна',    '+79004440002', DATE '1996-07-22'),
    (3,  'Козлов',     'Никита',     'Андреевич',    '+79004440003', DATE '1989-11-05'),
    (4,  'Морозова',   'Анна',       NULL,           '+79004440004', DATE '2001-01-18'),
    (5,  'Волков',     'Павел',      'Игоревич',     '+79004440005', DATE '1994-09-30'),
    (6,  'Соколова',   'Мария',      'Викторовна',   '+79004440006', DATE '1998-05-12'),
    (7,  'Лебедев',    'Денис',      'Олегович',     '+79004440007', DATE '1991-12-08'),
    (8,  'Новикова',   'Светлана',   'Игоревна',     '+79004440008', DATE '2000-04-27'),
    (9,  'Попов',      'Артём',      'Романович',    '+79004440009', DATE '1993-08-16'),
    (10, 'Васильева',  'Татьяна',    'Петровна',     '+79004440010', DATE '1997-02-03'),
    (11, 'Семёнов',    'Максим',     'Александрович','+79004440011', DATE '1990-06-21'),
    (12, 'Голубева',   'Виктория',   NULL,           '+79004440012', DATE '2002-10-11'),
    (13, 'Виноградов', 'Кирилл',     'Евгеньевич',   '+79004440013', DATE '1988-03-29'),
    (14, 'Борисова',   'Алина',      'Николаевна',   '+79004440014', DATE '1995-07-07'),
    (15, 'Фёдоров',    'Илья',       'Станиславович','+79004440015', DATE '1999-11-19'),
    (16, 'Медведева',  'Юлия',       'Андреевна',    '+79004440016', DATE '1992-01-25'),
    (17, 'Андреев',    'Георгий',    'Павлович',     '+79004440017', DATE '1987-09-13'),
    (18, 'Романова',   'Дарья',      NULL,           '+79004440018', DATE '2003-05-02'),
    (19, 'Орлов',      'Владислав',  'Константинович','+79004440019', DATE '1994-12-24'),
    (20, 'Захарова',   'Полина',     'Михайловна',   '+79004440020', DATE '1998-08-09'),
    (21, 'Макаров',    'Тимур',      'Рашидович',    '+79004440021', DATE '1991-04-17'),
    (22, 'Степанова',  'Кристина',   'Ильинична',    '+79004440022', DATE '1996-03-06'),
    (23, 'Никитин',    'Егор',       'Владимирович', '+79004440023', DATE '1989-10-28'),
    (24, 'Фролова',    'София',      NULL,           '+79004440024', DATE '2001-06-15'),
    (25, 'Михайлов',   'Руслан',     'Тимурович',    '+79004440025', DATE '1993-02-20'),
    (26, 'Белова',     'Вероника',   'Алексеевна',   '+79004440026', DATE '1997-09-01'),
    (27, 'Тарасов',    'Степан',     'Геннадьевич',  '+79004440027', DATE '1990-11-23'),
    (28, 'Комарова',   'Лилия',      NULL,           '+79004440028', DATE '2000-07-14'),
    (29, 'Жданов',     'Олег',       'Борисович',    '+79004440029', DATE '1988-05-31'),
    (30, 'Крылова',    'Надежда',    'Фёдоровна',    '+79004440030', DATE '1995-12-12'),
    (31, 'Григорьев',  'Вадим',      'Сергеевич',    '+79004440031', DATE '1992-08-04'),
    (32, 'Лазарева',   'Елена',      'Дмитриевна',   '+79004440032', DATE '1999-01-27'),
    (33, 'Егоров',     'Антон',      'Иванович',     '+79004440033', DATE '1994-04-10'),
    (34, 'Миронова',   'Инна',       NULL,           '+79004440034', DATE '2002-02-18'),
    (35, 'Киселёв',    'Богдан',     'Артурович',    '+79004440035', DATE '1991-10-05'),
    (36, 'Анисимова',  'Валерия',    'Олеговна',     '+79004440036', DATE '1998-06-22'),
    (37, 'Давыдов',    'Глеб',       'Максимович',   '+79004440037', DATE '1987-03-16'),
    (38, 'Савина',     'Арина',      NULL,           '+79004440038', DATE '2003-09-09'),
    (39, 'Титов',      'Ярослав',    'Петрович',     '+79004440039', DATE '1993-07-25'),
    (40, 'Рыбакова',   'Милана',     'Сергеевна',    '+79004440040', DATE '2000-12-30')
) AS v(rn, last_name, first_name, patronymic, phone, birth_date)
ON CONFLICT ("Phone") DO NOTHING;

-- --------------------------------------------------
-- Staff и Trainer: увеличим персонал
-- --------------------------------------------------
INSERT INTO "Staff" ("LastName", "FirstName", "Patronymic", "Phone", "HiredAt")
VALUES
    ('Егорова', 'Ирина', 'Павловна', '+79001110003', DATE '2024-04-10'),
    ('Громов', 'Павел', 'Алексеевич', '+79001110004', DATE '2024-06-12'),
    ('Тихонова', 'Светлана', 'Ильинична', '+79001110005', DATE '2025-01-15'),
    ('Данилов', 'Артем', 'Романович', '+79001110006', DATE '2025-02-01')
ON CONFLICT ("Phone") DO NOTHING;

INSERT INTO "Trainer" ("LastName", "FirstName", "Patronymic", "Phone", "HiredAt")
VALUES
    ('Ефимов', 'Илья', 'Константинович', '+79002220004', DATE '2024-04-01'),
    ('Орлова', 'Наталья', 'Игоревна', '+79002220005', DATE '2024-07-18'),
    ('Гаврилов', 'Станислав', 'Андреевич', '+79002220006', DATE '2025-01-08'),
    ('Жукова', 'Ксения', 'Олеговна', '+79002220007', DATE '2025-03-10'),
    ('Мельников', 'Роман', 'Сергеевич', '+79002220008', DATE '2025-04-02')
ON CONFLICT ("Phone") DO NOTHING;

-- --------------------------------------------------
-- TrainerSpecialization: добавим связки для новых тренеров
-- --------------------------------------------------
INSERT INTO "TrainerSpecialization" ("IdTrainer", "IdSpecialization")
SELECT t."IdTrainer", s."IdSpecialization"
FROM "Trainer" t
JOIN "Specialization" s ON (
    (t."LastName" = 'Ефимов' AND s."SpecializationName" IN ('Силовые тренировки', 'Кроссфит'))
    OR (t."LastName" = 'Орлова' AND s."SpecializationName" IN ('Йога', 'Пилатес'))
    OR (t."LastName" = 'Гаврилов' AND s."SpecializationName" IN ('Аэробика'))
    OR (t."LastName" = 'Жукова' AND s."SpecializationName" IN ('Стретчинг', 'Йога'))
    OR (t."LastName" = 'Мельников' AND s."SpecializationName" IN ('Кроссфит', 'Аэробика'))
)
ON CONFLICT DO NOTHING;

-- --------------------------------------------------
-- UserAccount: учетки для части новых клиентов и сотрудников
-- --------------------------------------------------
-- Для сотрудников (часть администраторов, часть ресепшн)
INSERT INTO "UserAccount" ("Login", "PasswordHash", "IdUserRole", "IdStaff")
SELECT
    lower('staff_' || s."IdStaff"::text),
    'Staff' || s."IdStaff"::text || '!1',
    r."IdUserRole",
    s."IdStaff"
FROM "Staff" s
JOIN "UserRole" r ON r."RoleName" = CASE WHEN s."IdStaff" % 2 = 0 THEN 'Ресепшн' ELSE 'Администратор' END
WHERE s."Phone" IN ('+79001110003', '+79001110004', '+79001110005', '+79001110006')
  AND NOT EXISTS (
      SELECT 1 FROM "UserAccount" u WHERE u."IdStaff" = s."IdStaff"
  );

-- Для тренеров
INSERT INTO "UserAccount" ("Login", "PasswordHash", "IdUserRole", "IdTrainer")
SELECT
    lower('trainer_' || t."IdTrainer"::text),
    'Trainer' || t."IdTrainer"::text || '!1',
    r."IdUserRole",
    t."IdTrainer"
FROM "Trainer" t
JOIN "UserRole" r ON r."RoleName" = 'Тренер'
WHERE t."Phone" IN ('+79002220004', '+79002220005', '+79002220006', '+79002220007', '+79002220008')
  AND NOT EXISTS (
      SELECT 1 FROM "UserAccount" u WHERE u."IdTrainer" = t."IdTrainer"
  );

-- Для части клиентов (чтобы остались и без аккаунта тоже)
INSERT INTO "UserAccount" ("Login", "PasswordHash", "IdUserRole", "IdClient")
SELECT
    lower('client_' || c."IdClient"::text),
    'Client' || c."IdClient"::text || '!1',
    r."IdUserRole",
    c."IdClient"
FROM "Client" c
JOIN "UserRole" r ON r."RoleName" = 'Клиент'
WHERE c."Phone" LIKE '+7900444%'
  AND c."IdClient" % 2 = 0
  AND NOT EXISTS (
      SELECT 1 FROM "UserAccount" u WHERE u."IdClient" = c."IdClient"
  );

-- --------------------------------------------------
-- Membership: абонементы для клиентов без абонементов
-- --------------------------------------------------
INSERT INTO "Membership" ("IdClient", "IdMembershipType", "StartDate", "EndDate", "IdMembershipStatus", "SoldAt", "IdSoldByUser")
SELECT
    c."IdClient",
    mt."IdMembershipType",
    start_dt,
    end_dt,
    ms."IdMembershipStatus",
    start_dt::timestamp + time '10:00',
    sold_by."IdUserAccount"
FROM (
    SELECT
        c0."IdClient",
        CASE
            WHEN c0."IdClient" % 4 = 0 THEN '12 посещений'
            WHEN c0."IdClient" % 4 = 1 THEN 'Месяц безлимит'
            WHEN c0."IdClient" % 4 = 2 THEN '3 месяца безлимит'
            ELSE 'Разовое посещение'
        END AS type_name,
        CASE
            WHEN c0."IdClient" % 6 = 0 THEN 'expired'
            WHEN c0."IdClient" % 7 = 0 THEN 'frozen'
            ELSE 'active'
        END AS status_code,
        (CURRENT_DATE - ((c0."IdClient" % 20) + 5))::date AS start_dt,
        CASE
            WHEN c0."IdClient" % 4 = 0 THEN (CURRENT_DATE + 40)::date
            WHEN c0."IdClient" % 4 = 1 THEN (CURRENT_DATE + 20)::date
            WHEN c0."IdClient" % 4 = 2 THEN (CURRENT_DATE + 75)::date
            ELSE CURRENT_DATE
        END AS end_dt
    FROM "Client" c0
) c
JOIN "MembershipType" mt ON mt."TypeName" = c.type_name
JOIN "MembershipStatus" ms ON ms."StatusCode" = c.status_code
JOIN "UserAccount" sold_by ON sold_by."Login" = 'reception_fc'
WHERE NOT EXISTS (
    SELECT 1 FROM "Membership" m WHERE m."IdClient" = c."IdClient"
);

-- --------------------------------------------------
-- MembershipStatusHistory: минимум 1 запись на каждый абонемент + доп. события
-- --------------------------------------------------
INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "ChangedAt", "IdChangedByUser", "Comment")
SELECT
    m."IdMembership",
    m."IdMembershipStatus",
    m."SoldAt",
    m."IdSoldByUser",
    'Оформление абонемента'
FROM "Membership" m
WHERE NOT EXISTS (
    SELECT 1 FROM "MembershipStatusHistory" h WHERE h."IdMembership" = m."IdMembership"
);

-- Для части active добавим frozen и обратно active
INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "ChangedAt", "IdChangedByUser", "Comment")
SELECT
    m."IdMembership",
    ms_frozen."IdMembershipStatus",
    (m."SoldAt" + interval '10 day'),
    m."IdSoldByUser",
    'Временная заморозка'
FROM "Membership" m
JOIN "MembershipStatus" ms_cur ON ms_cur."IdMembershipStatus" = m."IdMembershipStatus"
JOIN "MembershipStatus" ms_frozen ON ms_frozen."StatusCode" = 'frozen'
WHERE ms_cur."StatusCode" = 'active'
  AND m."IdMembership" % 5 = 0
  AND NOT EXISTS (
      SELECT 1 FROM "MembershipStatusHistory" h
      WHERE h."IdMembership" = m."IdMembership"
        AND h."IdMembershipStatus" = ms_frozen."IdMembershipStatus"
  );

INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "ChangedAt", "IdChangedByUser", "Comment")
SELECT
    m."IdMembership",
    ms_active."IdMembershipStatus",
    (m."SoldAt" + interval '15 day'),
    m."IdSoldByUser",
    'Разморозка'
FROM "Membership" m
JOIN "MembershipStatus" ms_cur ON ms_cur."IdMembershipStatus" = m."IdMembershipStatus"
JOIN "MembershipStatus" ms_active ON ms_active."StatusCode" = 'active'
WHERE ms_cur."StatusCode" = 'active'
  AND m."IdMembership" % 5 = 0
  AND NOT EXISTS (
      SELECT 1 FROM "MembershipStatusHistory" h
      WHERE h."IdMembership" = m."IdMembership"
        AND h."IdMembershipStatus" = ms_active."IdMembershipStatus"
        AND h."Comment" = 'Разморозка'
  );

-- --------------------------------------------------
-- GroupClass: расширим расписание
-- --------------------------------------------------
INSERT INTO "GroupClass" ("ClassName", "IdGymHall", "IdTrainer", "IdDayOfWeek", "StartTime", "DurationMinutes", "MaxParticipants", "IsActive")
SELECT
    x.class_name,
    h."IdGymHall",
    t."IdTrainer",
    d."IdDayOfWeek",
    x.start_time::time,
    x.duration_min,
    x.max_part,
    true
FROM (VALUES
    ('Power Hour', 'Тренажёрный зал', 'Ефимов', 'Вторник', '18:30', 60, 18),
    ('Утренняя йога', 'Зал йоги', 'Орлова', 'Четверг', '08:00', 50, 15),
    ('Cardio Mix', 'Зал групповых программ', 'Гаврилов', 'Понедельник', '19:00', 55, 24),
    ('Stretch PRO', 'Зал йоги', 'Жукова', 'Суббота', '11:00', 45, 14),
    ('Cross Training', 'Зал кроссфита', 'Мельников', 'Пятница', '20:00', 60, 20),
    ('Pilates Basic', 'Зал групповых программ', 'Соколова', 'Вторник', '10:00', 50, 22),
    ('Йога баланс', 'Зал йоги', 'Волкова', 'Воскресенье', '09:30', 60, 15)
) AS x(class_name, hall_name, trainer_last_name, day_name, start_time, duration_min, max_part)
JOIN "GymHall" h ON h."HallName" = x.hall_name
JOIN "Trainer" t ON t."LastName" = x.trainer_last_name
JOIN "DayOfWeek" d ON d."DayName" = x.day_name
WHERE NOT EXISTS (
    SELECT 1 FROM "GroupClass" gc WHERE gc."ClassName" = x.class_name
);

-- --------------------------------------------------
-- Visit: добавим поток посещений по абонементам
-- --------------------------------------------------
INSERT INTO "Visit" ("IdClient", "VisitDateTime", "IdMarkedByUser", "IdMembership")
SELECT
    m."IdClient",
    (CURRENT_DATE - (g.n * 2))::timestamp + time '18:00',
    marked_by."IdUserAccount",
    m."IdMembership"
FROM "Membership" m
JOIN generate_series(1, 6) AS g(n) ON true
JOIN "MembershipStatus" ms ON ms."IdMembershipStatus" = m."IdMembershipStatus"
JOIN "UserAccount" marked_by ON marked_by."Login" = 'reception_fc'
WHERE ms."StatusCode" IN ('active', 'frozen', 'expired')
  AND NOT EXISTS (
      SELECT 1 FROM "Visit" v
      WHERE v."IdClient" = m."IdClient"
        AND v."VisitDateTime" = ((CURRENT_DATE - (g.n * 2))::timestamp + time '18:00')
  );

-- --------------------------------------------------
-- ClassBooking: массовые записи (с запасом до лимита)
-- --------------------------------------------------
WITH class_dates AS (
    SELECT gc."IdGroupClass", (CURRENT_DATE + 3)::date AS class_date FROM "GroupClass" gc
    UNION ALL
    SELECT gc."IdGroupClass", (CURRENT_DATE + 10)::date FROM "GroupClass" gc
),
ranked_clients AS (
    SELECT c."IdClient", ROW_NUMBER() OVER (ORDER BY c."IdClient") AS rn
    FROM "Client" c
),
booking_pool AS (
    SELECT
        cd."IdGroupClass",
        cd.class_date,
        rc."IdClient"
    FROM class_dates cd
    JOIN ranked_clients rc ON rc.rn <= 10
)
INSERT INTO "ClassBooking" ("IdClient", "IdGroupClass", "ClassDate", "BookedAt", "IdBookedByUser")
SELECT
    bp."IdClient",
    bp."IdGroupClass",
    bp.class_date,
    (now() AT TIME ZONE 'UTC') - interval '1 day',
    booked_by."IdUserAccount"
FROM booking_pool bp
JOIN "UserAccount" booked_by ON booked_by."Login" = 'reception_fc'
WHERE NOT EXISTS (
    SELECT 1 FROM "ClassBooking" b
    WHERE b."IdClient" = bp."IdClient"
      AND b."IdGroupClass" = bp."IdGroupClass"
      AND b."ClassDate" = bp.class_date
);

COMMIT;

