-- Клиенты, абонементы и записи на групповые занятия до 31.08.2026
-- (минимум 10 человек на каждое предстоящее занятие)
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/13_seed_clients_bookings_aug2026.sql

BEGIN;

-- Удаляем записи на «неправильный» день недели (остатки старых seed-скриптов)
DELETE FROM "ClassBooking" b
USING "GroupClass" gc, "DayOfWeek" d
WHERE gc."IdGroupClass" = b."IdGroupClass"
  AND d."IdDayOfWeek" = gc."IdDayOfWeek"
  AND EXTRACT(ISODOW FROM b."ClassDate") <> d."DayNumber";

-- --------------------------------------------------
-- Новые клиенты
-- --------------------------------------------------
INSERT INTO "Client" ("LastName", "FirstName", "Patronymic", "Phone", "BirthDate", "RegisteredAt")
SELECT
    v.last_name,
    v.first_name,
    v.patronymic,
    v.phone,
    v.birth_date,
    TIMESTAMP '2026-05-01 10:00:00' + (v.rn || ' hours')::interval
FROM (VALUES
    (1,  'Алексеев',   'Михаил',   'Сергеевич',    '+79005550001', DATE '1991-02-14'),
    (2,  'Баранова',   'Ольга',    'Ивановна',     '+79005550002', DATE '1994-08-03'),
    (3,  'Власов',     'Артём',    'Николаевич',   '+79005550003', DATE '1998-11-27'),
    (4,  'Громова',    'Елена',    NULL,           '+79005550004', DATE '2000-05-19'),
    (5,  'Дементьев',  'Игорь',    'Павлович',     '+79005550005', DATE '1989-09-08'),
    (6,  'Ермакова',   'Наталья',  'Андреевна',    '+79005550006', DATE '1996-01-22'),
    (7,  'Жуков',      'Константин','Олегович',    '+79005550007', DATE '1993-07-11'),
    (8,  'Зайцева',    'Марина',   'Викторовна',   '+79005550008', DATE '1999-12-05'),
    (9,  'Ильин',      'Роман',    'Алексеевич',   '+79005550009', DATE '1990-04-30'),
    (10, 'Калинина',   'Анастасия','Дмитриевна',   '+79005550010', DATE '2001-03-17'),
    (11, 'Лукин',      'Сергей',   'Геннадьевич',  '+79005550011', DATE '1988-10-24'),
    (12, 'Мартынова',  'Вера',     NULL,           '+79005550012', DATE '1997-06-09'),
    (13, 'Назаров',    'Даниил',   'Евгеньевич',   '+79005550013', DATE '1995-08-21'),
    (14, 'Осипова',    'Ксения',   'Романовна',    '+79005550014', DATE '2002-02-02'),
    (15, 'Панфилов',   'Владимир', 'Игоревич',     '+79005550015', DATE '1992-11-15'),
    (16, 'Родионова',  'Людмила',  'Петровна',     '+79005550016', DATE '1987-05-28'),
    (17, 'Сафонов',    'Николай',  'Анатольевич',  '+79005550017', DATE '1994-09-13'),
    (18, 'Терентьева', 'Ирина',    NULL,           '+79005550018', DATE '1998-04-07'),
    (19, 'Устинов',    'Пётр',     'Михайлович',   '+79005550019', DATE '1991-01-31'),
    (20, 'Филиппова',  'Алёна',    'Сергеевна',    '+79005550020', DATE '2000-10-18'),
    (21, 'Харитонов',  'Олег',     'Владиславович','+79005550021', DATE '1993-03-26'),
    (22, 'Цветкова',   'Диана',    'Артуровна',    '+79005550022', DATE '1999-07-04'),
    (23, 'Широков',    'Евгений',  'Борисович',    '+79005550023', DATE '1990-12-12'),
    (24, 'Щербакова',  'Тамара',   NULL,           '+79005550024', DATE '1996-08-29'),
    (25, 'Яковлев',    'Станислав','Кириллович',   '+79005550025', DATE '1989-06-06')
) AS v(rn, last_name, first_name, patronymic, phone, birth_date)
ON CONFLICT ("Phone") DO NOTHING;

-- Учётные записи для новых клиентов (каждый второй)
INSERT INTO "UserAccount" ("Login", "PasswordHash", "IdUserRole", "IdClient")
SELECT
    lower('client_' || c."IdClient"::text),
    'Client' || c."IdClient"::text || '!1',
    r."IdUserRole",
    c."IdClient"
FROM "Client" c
JOIN "UserRole" r ON r."RoleName" = 'Клиент'
WHERE c."Phone" LIKE '+7900555%'
  AND c."IdClient" % 2 = 0
  AND NOT EXISTS (
      SELECT 1 FROM "UserAccount" u WHERE u."IdClient" = c."IdClient"
  );

-- --------------------------------------------------
-- Абонементы: новым клиентам + продление тем, у кого истекает до 31.08.2026
-- --------------------------------------------------
INSERT INTO "Membership" ("IdClient", "IdMembershipType", "StartDate", "EndDate", "IdMembershipStatus", "SoldAt", "IdSoldByUser")
SELECT
    c."IdClient",
    mt."IdMembershipType",
    DATE '2026-06-01',
    DATE '2026-09-30',
    ms."IdMembershipStatus",
    TIMESTAMP '2026-06-01 11:00:00',
    sold_by."IdUserAccount"
FROM "Client" c
JOIN "MembershipType" mt ON mt."TypeName" = '3 месяца безлимит'
JOIN "MembershipStatus" ms ON ms."StatusCode" = 'active'
JOIN "UserAccount" sold_by ON sold_by."Login" = 'reception_fc'
WHERE c."Phone" LIKE '+7900555%'
  AND NOT EXISTS (
      SELECT 1 FROM "Membership" m WHERE m."IdClient" = c."IdClient"
  );

INSERT INTO "Membership" ("IdClient", "IdMembershipType", "StartDate", "EndDate", "IdMembershipStatus", "SoldAt", "IdSoldByUser")
SELECT
    c."IdClient",
    mt."IdMembershipType",
    DATE '2026-06-01',
    DATE '2026-09-30',
    ms."IdMembershipStatus",
    TIMESTAMP '2026-06-02 12:00:00',
    sold_by."IdUserAccount"
FROM "Client" c
JOIN "MembershipType" mt ON mt."TypeName" = '3 месяца безлимит'
JOIN "MembershipStatus" ms ON ms."StatusCode" = 'active'
JOIN "UserAccount" sold_by ON sold_by."Login" = 'reception_fc'
WHERE NOT EXISTS (
    SELECT 1
    FROM "Membership" m
    JOIN "MembershipStatus" ms2 ON ms2."IdMembershipStatus" = m."IdMembershipStatus"
    WHERE m."IdClient" = c."IdClient"
      AND ms2."StatusCode" = 'active'
      AND m."EndDate" >= DATE '2026-08-31'
);

INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "ChangedAt", "IdChangedByUser", "Comment")
SELECT
    m."IdMembership",
    m."IdMembershipStatus",
    m."SoldAt",
    m."IdSoldByUser",
    'Оформление абонемента'
FROM "Membership" m
JOIN "Client" c ON c."IdClient" = m."IdClient"
WHERE (c."Phone" LIKE '+7900555%' OR m."StartDate" = DATE '2026-06-01')
  AND NOT EXISTS (
      SELECT 1 FROM "MembershipStatusHistory" h WHERE h."IdMembership" = m."IdMembership"
  );

-- --------------------------------------------------
-- Записи на занятия до 31.08.2026 (минимум 10 человек на каждое занятие)
-- --------------------------------------------------
WITH active_classes AS (
    SELECT
        gc."IdGroupClass",
        gc."ClassName",
        gc."MaxParticipants",
        d."DayNumber"
    FROM "GroupClass" gc
    JOIN "DayOfWeek" d ON d."IdDayOfWeek" = gc."IdDayOfWeek"
    WHERE gc."IsActive"
),
occurrences AS (
    SELECT
        ac."IdGroupClass",
        ac."ClassName",
        ac."MaxParticipants",
        gs.dt::date AS class_date
    FROM active_classes ac
    CROSS JOIN generate_series(DATE '2026-06-01', DATE '2026-08-31', INTERVAL '1 day') AS gs(dt)
    WHERE EXTRACT(ISODOW FROM gs.dt) = ac."DayNumber"
),
not_cancelled AS (
    SELECT o.*
    FROM occurrences o
    WHERE NOT EXISTS (
        SELECT 1
        FROM "CancelledClassSession" c
        WHERE c."IdGroupClass" = o."IdGroupClass"
          AND c."ClassDate" = o.class_date
    )
),
booking_need AS (
    SELECT
        nc."IdGroupClass",
        nc.class_date,
        nc."MaxParticipants",
        GREATEST(
            0,
            LEAST(10, nc."MaxParticipants") - COALESCE(existing.cnt, 0)
        ) AS need_count
    FROM not_cancelled nc
    LEFT JOIN (
        SELECT "IdGroupClass", "ClassDate", COUNT(*) AS cnt
        FROM "ClassBooking"
        GROUP BY "IdGroupClass", "ClassDate"
    ) existing ON existing."IdGroupClass" = nc."IdGroupClass"
              AND existing."ClassDate" = nc.class_date
    WHERE GREATEST(
        0,
        LEAST(10, nc."MaxParticipants") - COALESCE(existing.cnt, 0)
    ) > 0
),
ranked_clients AS (
    SELECT
        bn."IdGroupClass",
        bn.class_date,
        c."IdClient",
        ROW_NUMBER() OVER (
            PARTITION BY bn."IdGroupClass", bn.class_date
            ORDER BY c."IdClient"
        ) AS client_rn
    FROM booking_need bn
    JOIN "Client" c ON TRUE
    WHERE EXISTS (
        SELECT 1
        FROM "Membership" m
        JOIN "MembershipStatus" ms ON ms."IdMembershipStatus" = m."IdMembershipStatus"
        WHERE m."IdClient" = c."IdClient"
          AND ms."StatusCode" = 'active'
          AND m."StartDate" <= bn.class_date
          AND m."EndDate" >= bn.class_date
    )
      AND NOT EXISTS (
          SELECT 1
          FROM "ClassBooking" b
          WHERE b."IdClient" = c."IdClient"
            AND b."IdGroupClass" = bn."IdGroupClass"
            AND b."ClassDate" = bn.class_date
      )
),
assignments AS (
    SELECT
        rc."IdClient",
        rc."IdGroupClass",
        rc.class_date
    FROM ranked_clients rc
    JOIN booking_need bn
      ON bn."IdGroupClass" = rc."IdGroupClass"
     AND bn.class_date = rc.class_date
    WHERE rc.client_rn <= bn.need_count
)
INSERT INTO "ClassBooking" ("IdClient", "IdGroupClass", "ClassDate", "BookedAt", "IdBookedByUser")
SELECT
    a."IdClient",
    a."IdGroupClass",
    a.class_date,
    (a.class_date - INTERVAL '2 days') + TIME '10:00',
    booked_by."IdUserAccount"
FROM assignments a
JOIN "UserAccount" booked_by ON booked_by."Login" = 'reception_fc'
WHERE NOT EXISTS (
    SELECT 1
    FROM "ClassBooking" b
    WHERE b."IdClient" = a."IdClient"
      AND b."IdGroupClass" = a."IdGroupClass"
      AND b."ClassDate" = a.class_date
);

-- Дозаполнение, если на занятии всё ещё меньше 10 человек
WITH active_classes AS (
    SELECT gc."IdGroupClass", gc."MaxParticipants", d."DayNumber"
    FROM "GroupClass" gc
    JOIN "DayOfWeek" d ON d."IdDayOfWeek" = gc."IdDayOfWeek"
    WHERE gc."IsActive"
),
occurrences AS (
    SELECT ac."IdGroupClass", ac."MaxParticipants", gs.dt::date AS class_date
    FROM active_classes ac
    CROSS JOIN generate_series(DATE '2026-06-01', DATE '2026-08-31', INTERVAL '1 day') AS gs(dt)
    WHERE EXTRACT(ISODOW FROM gs.dt) = ac."DayNumber"
),
booking_need AS (
    SELECT
        o."IdGroupClass",
        o.class_date,
        GREATEST(
            0,
            LEAST(10, o."MaxParticipants") - COALESCE(existing.cnt, 0)
        ) AS need_count
    FROM occurrences o
    LEFT JOIN (
        SELECT "IdGroupClass", "ClassDate", COUNT(*) AS cnt
        FROM "ClassBooking"
        GROUP BY "IdGroupClass", "ClassDate"
    ) existing ON existing."IdGroupClass" = o."IdGroupClass"
              AND existing."ClassDate" = o.class_date
    WHERE NOT EXISTS (
        SELECT 1 FROM "CancelledClassSession" c
        WHERE c."IdGroupClass" = o."IdGroupClass" AND c."ClassDate" = o.class_date
    )
      AND GREATEST(
          0,
          LEAST(10, o."MaxParticipants") - COALESCE(existing.cnt, 0)
      ) > 0
),
ranked_clients AS (
    SELECT
        bn."IdGroupClass",
        bn.class_date,
        c."IdClient",
        ROW_NUMBER() OVER (
            PARTITION BY bn."IdGroupClass", bn.class_date
            ORDER BY c."IdClient"
        ) AS client_rn
    FROM booking_need bn
    JOIN "Client" c ON TRUE
    WHERE EXISTS (
        SELECT 1
        FROM "Membership" m
        JOIN "MembershipStatus" ms ON ms."IdMembershipStatus" = m."IdMembershipStatus"
        WHERE m."IdClient" = c."IdClient"
          AND ms."StatusCode" = 'active'
          AND m."StartDate" <= bn.class_date
          AND m."EndDate" >= bn.class_date
    )
      AND NOT EXISTS (
          SELECT 1
          FROM "ClassBooking" b
          WHERE b."IdClient" = c."IdClient"
            AND b."IdGroupClass" = bn."IdGroupClass"
            AND b."ClassDate" = bn.class_date
      )
)
INSERT INTO "ClassBooking" ("IdClient", "IdGroupClass", "ClassDate", "BookedAt", "IdBookedByUser")
SELECT
    rc."IdClient",
    rc."IdGroupClass",
    rc.class_date,
    (rc.class_date - INTERVAL '2 days') + TIME '10:00',
    booked_by."IdUserAccount"
FROM ranked_clients rc
JOIN booking_need bn
  ON bn."IdGroupClass" = rc."IdGroupClass"
 AND bn.class_date = rc.class_date
JOIN "UserAccount" booked_by ON booked_by."Login" = 'reception_fc'
WHERE rc.client_rn <= bn.need_count;

COMMIT;

-- Отчёт
SELECT 'clients_total' AS metric, COUNT(*)::text AS value FROM "Client"
UNION ALL
SELECT 'clients_new_7900555', COUNT(*)::text FROM "Client" WHERE "Phone" LIKE '+7900555%'
UNION ALL
SELECT 'active_memberships_until_aug31', COUNT(*)::text
FROM "Membership" m
JOIN "MembershipStatus" ms ON ms."IdMembershipStatus" = m."IdMembershipStatus"
WHERE ms."StatusCode" = 'active' AND m."EndDate" >= DATE '2026-08-31'
UNION ALL
SELECT 'sessions_under_10_bookings', COUNT(*)::text
FROM (
    WITH active_classes AS (
        SELECT gc."IdGroupClass", gc."MaxParticipants", d."DayNumber"
        FROM "GroupClass" gc
        JOIN "DayOfWeek" d ON d."IdDayOfWeek" = gc."IdDayOfWeek"
        WHERE gc."IsActive"
    ),
    occurrences AS (
        SELECT ac."IdGroupClass", ac."MaxParticipants", gs.dt::date AS class_date
        FROM active_classes ac
        CROSS JOIN generate_series(DATE '2026-06-01', DATE '2026-08-31', INTERVAL '1 day') AS gs(dt)
        WHERE EXTRACT(ISODOW FROM gs.dt) = ac."DayNumber"
    )
    SELECT o."IdGroupClass", o.class_date
    FROM occurrences o
    LEFT JOIN "CancelledClassSession" c
      ON c."IdGroupClass" = o."IdGroupClass" AND c."ClassDate" = o.class_date
    LEFT JOIN (
        SELECT "IdGroupClass", "ClassDate", COUNT(*) AS cnt
        FROM "ClassBooking"
        GROUP BY "IdGroupClass", "ClassDate"
    ) b ON b."IdGroupClass" = o."IdGroupClass" AND b."ClassDate" = o.class_date
    WHERE c."IdCancelledClassSession" IS NULL
      AND COALESCE(b.cnt, 0) < LEAST(10, o."MaxParticipants")
) x;
