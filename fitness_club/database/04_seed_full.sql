-- Демо-данные для всех оставшихся таблиц Fitness_Club
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/04_seed_full.sql

BEGIN;

-- --------------------
-- Membership: создаём абонементы для остальных клиентов
-- --------------------
-- Клиент: Кузнецова (IdClient=2)
INSERT INTO "Membership" ("IdClient", "IdMembershipType", "StartDate", "EndDate", "IdMembershipStatus", "SoldAt", "IdSoldByUser")
SELECT
    c."IdClient",
    mt."IdMembershipType",
    DATE '2026-06-01',
    DATE '2026-07-31',
    ms."IdMembershipStatus",
    (TIMESTAMP '2026-06-01 12:00:00'),
    ua."IdUserAccount"
FROM "Client" c
JOIN "MembershipType" mt ON mt."TypeName" = '12 посещений'
JOIN "MembershipStatus" ms ON ms."StatusCode" = 'active'
JOIN "UserAccount" ua ON ua."Login" = 'reception_fc'
WHERE c."IdClient" = 2
  AND NOT EXISTS (
      SELECT 1 FROM "Membership" m
      WHERE m."IdClient" = 2
        AND m."IdMembershipType" = mt."IdMembershipType"
        AND m."StartDate" = DATE '2026-06-01'
  );

-- Клиент: Новиков (IdClient=3)
INSERT INTO "Membership" ("IdClient", "IdMembershipType", "StartDate", "EndDate", "IdMembershipStatus", "SoldAt", "IdSoldByUser")
SELECT
    c."IdClient",
    mt."IdMembershipType",
    DATE '2026-06-05',
    DATE '2026-09-04',
    ms."IdMembershipStatus",
    (TIMESTAMP '2026-06-05 14:30:00'),
    ua."IdUserAccount"
FROM "Client" c
JOIN "MembershipType" mt ON mt."TypeName" = '3 месяца безлимит'
JOIN "MembershipStatus" ms ON ms."StatusCode" = 'active'
JOIN "UserAccount" ua ON ua."Login" = 'reception_fc'
WHERE c."IdClient" = 3
  AND NOT EXISTS (
      SELECT 1 FROM "Membership" m
      WHERE m."IdClient" = 3
        AND m."IdMembershipType" = mt."IdMembershipType"
        AND m."StartDate" = DATE '2026-06-05'
  );

-- --------------------
-- MembershipStatusHistory: добавляем историю статусов
-- --------------------
-- Для абонемента Иванова (Membership у нас уже есть). Добавим заморозку и разморозку.
INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "ChangedAt", "IdChangedByUser", "Comment")
SELECT
    m."IdMembership",
    ms_frozen."IdMembershipStatus",
    TIMESTAMP '2026-06-20 10:00:00',
    ua."IdUserAccount",
    'Заморозка по заявлению'
FROM "Membership" m
JOIN "Client" c ON c."IdClient" = m."IdClient"
JOIN "MembershipStatus" ms_frozen ON ms_frozen."StatusCode" = 'frozen'
JOIN "UserAccount" ua ON ua."Login" = 'reception_fc'
WHERE c."IdClient" = 1
  AND NOT EXISTS (
      SELECT 1 FROM "MembershipStatusHistory" h
      WHERE h."IdMembership" = m."IdMembership"
        AND h."IdMembershipStatus" = ms_frozen."IdMembershipStatus"
        AND h."ChangedAt" = TIMESTAMP '2026-06-20 10:00:00'
  );

INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "ChangedAt", "IdChangedByUser", "Comment")
SELECT
    m."IdMembership",
    ms_active."IdMembershipStatus",
    TIMESTAMP '2026-06-23 18:20:00',
    ua."IdUserAccount",
    'Разморозка'
FROM "Membership" m
JOIN "Client" c ON c."IdClient" = m."IdClient"
JOIN "MembershipStatus" ms_active ON ms_active."StatusCode" = 'active'
JOIN "UserAccount" ua ON ua."Login" = 'reception_fc'
WHERE c."IdClient" = 1
  AND NOT EXISTS (
      SELECT 1 FROM "MembershipStatusHistory" h
      WHERE h."IdMembership" = m."IdMembership"
        AND h."IdMembershipStatus" = ms_active."IdMembershipStatus"
        AND h."ChangedAt" = TIMESTAMP '2026-06-23 18:20:00'
  );

-- Для абонемента Кузнецовой: старт active (история может быть полезна для отчётов)
INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "ChangedAt", "IdChangedByUser", "Comment")
SELECT
    m."IdMembership",
    ms_active."IdMembershipStatus",
    TIMESTAMP '2026-06-01 12:05:00',
    ua."IdUserAccount",
    'Оформление абонемента'
FROM "Membership" m
JOIN "Client" c ON c."IdClient" = m."IdClient"
JOIN "MembershipStatus" ms_active ON ms_active."StatusCode" = 'active'
JOIN "UserAccount" ua ON ua."Login" = 'reception_fc'
WHERE c."IdClient" = 2
  AND NOT EXISTS (
      SELECT 1 FROM "MembershipStatusHistory" h
      WHERE h."IdMembership" = m."IdMembership"
        AND h."IdMembershipStatus" = ms_active."IdMembershipStatus"
        AND h."ChangedAt" = TIMESTAMP '2026-06-01 12:05:00'
  );

-- Для абонемента Новикова
INSERT INTO "MembershipStatusHistory" ("IdMembership", "IdMembershipStatus", "ChangedAt", "IdChangedByUser", "Comment")
SELECT
    m."IdMembership",
    ms_active."IdMembershipStatus",
    TIMESTAMP '2026-06-05 14:35:00',
    ua."IdUserAccount",
    'Оформление абонемента'
FROM "Membership" m
JOIN "Client" c ON c."IdClient" = m."IdClient"
JOIN "MembershipStatus" ms_active ON ms_active."StatusCode" = 'active'
JOIN "UserAccount" ua ON ua."Login" = 'reception_fc'
WHERE c."IdClient" = 3
  AND NOT EXISTS (
      SELECT 1 FROM "MembershipStatusHistory" h
      WHERE h."IdMembership" = m."IdMembership"
        AND h."IdMembershipStatus" = ms_active."IdMembershipStatus"
        AND h."ChangedAt" = TIMESTAMP '2026-06-05 14:35:00'
  );

-- --------------------
-- Visit: посещения (вход в клуб)
-- --------------------
INSERT INTO "Visit" ("IdClient", "VisitDateTime", "IdMarkedByUser", "IdMembership")
SELECT
    v_c."IdClient",
    v_dt.visit_dt,
    ua."IdUserAccount",
    m."IdMembership"
FROM (VALUES
    (2, TIMESTAMP '2026-06-02 10:10:00'),
    (2, TIMESTAMP '2026-06-05 19:40:00'),
    (2, TIMESTAMP '2026-06-09 11:30:00'),
    (2, TIMESTAMP '2026-06-12 18:00:00'),
    (3, TIMESTAMP '2026-06-06 08:25:00'),
    (3, TIMESTAMP '2026-06-10 17:05:00'),
    (3, TIMESTAMP '2026-06-14 12:10:00'),
    (3, TIMESTAMP '2026-06-16 15:55:00'),
    (1, TIMESTAMP '2026-06-12 09:05:00'),
    (1, TIMESTAMP '2026-06-15 18:45:00'),
    (1, TIMESTAMP '2026-06-18 13:20:00')
) AS v_dt(id_client, visit_dt)
JOIN "Client" v_c ON v_c."IdClient" = v_dt.id_client
JOIN "UserAccount" ua ON ua."Login" = 'reception_fc'
LEFT JOIN LATERAL (
    SELECT m2."IdMembership"
    FROM "Membership" m2
    WHERE m2."IdClient" = v_c."IdClient"
      AND m2."StartDate" <= (v_dt.visit_dt::date)
      AND m2."EndDate" >= (v_dt.visit_dt::date)
    ORDER BY m2."IdMembership" DESC
    LIMIT 1
) m ON TRUE
WHERE NOT EXISTS (
    SELECT 1 FROM "Visit" vv
    WHERE vv."IdClient" = v_c."IdClient"
      AND vv."VisitDateTime" = v_dt.visit_dt
);

-- --------------------
-- ClassBooking: записи на групповые занятия
-- --------------------
-- Даты подобраны под дни недели:
-- Йога (Понедельник): 2026-06-15, 2026-06-22
-- Кроссфит (Среда):   2026-06-17, 2026-06-24
-- Пилатес (Пятница):  2026-06-19, 2026-06-26

INSERT INTO "ClassBooking" ("IdClient", "IdGroupClass", "ClassDate", "BookedAt", "IdBookedByUser")
SELECT
    c."IdClient",
    gc."IdGroupClass",
    bd.class_date,
    (TIMESTAMP '2026-06-14 09:00:00'),
    ua."IdUserAccount"
FROM "Client" c
JOIN "GroupClass" gc ON 1=1
JOIN "UserAccount" ua ON ua."Login" = 'reception_fc'
JOIN (VALUES
    ('Йога для начинающих', DATE '2026-06-15'),
    ('Йога для начинающих', DATE '2026-06-22'),
    ('Кроссфит WOD', DATE '2026-06-17'),
    ('Кроссфит WOD', DATE '2026-06-24'),
    ('Пилатес', DATE '2026-06-19'),
    ('Пилатес', DATE '2026-06-26')
) AS bd(class_name, class_date) ON gc."ClassName" = bd.class_name
WHERE NOT EXISTS (
    SELECT 1 FROM "ClassBooking" b
    WHERE b."IdClient" = c."IdClient"
      AND b."IdGroupClass" = gc."IdGroupClass"
      AND b."ClassDate" = bd.class_date
);

COMMIT;

