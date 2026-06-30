-- Проверка и исправление конфликтов расписания (один зал / один день / пересечение по времени).
-- Безопасно запускать повторно.

BEGIN;

-- 1) Максимум участников не больше вместимости зала
UPDATE "GroupClass" gc
SET "MaxParticipants" = h."Capacity"
FROM "GymHall" h
WHERE gc."IdGymHall" = h."IdGymHall"
  AND gc."MaxParticipants" > h."Capacity";

-- 2) Сдвинуть более позднее занятие на конец предыдущего при пересечении в том же зале и дне
DO $$
DECLARE
    r RECORD;
    new_start time;
    guard integer := 0;
BEGIN
    LOOP
        guard := guard + 1;
        EXIT WHEN guard > 50;

        SELECT
            later."IdGroupClass" AS id_later,
            (earlier."StartTime" + (earlier."DurationMinutes" || ' minutes')::interval)::time AS shifted_start
        INTO r
        FROM "GroupClass" earlier
        JOIN "GroupClass" later
          ON earlier."IdGymHall" = later."IdGymHall"
         AND earlier."IdDayOfWeek" = later."IdDayOfWeek"
         AND earlier."IdGroupClass" <> later."IdGroupClass"
        WHERE earlier."IsActive"
          AND later."IsActive"
          AND earlier."StartTime" < later."StartTime"
          AND earlier."StartTime" < (later."StartTime" + (later."DurationMinutes" || ' minutes')::interval)
          AND later."StartTime" < (earlier."StartTime" + (earlier."DurationMinutes" || ' minutes')::interval)
        ORDER BY later."IdGroupClass"
        LIMIT 1;

        EXIT WHEN NOT FOUND;

        new_start := r.shifted_start;
        IF new_start >= '23:00'::time THEN
            UPDATE "GroupClass"
            SET "IsActive" = false
            WHERE "IdGroupClass" = r.id_later;
        ELSE
            UPDATE "GroupClass"
            SET "StartTime" = new_start
            WHERE "IdGroupClass" = r.id_later;
        END IF;
    END LOOP;
END $$;

-- 3) Если после сдвига всё ещё есть пересечения — деактивировать более новое занятие
UPDATE "GroupClass" gc
SET "IsActive" = false
FROM (
    SELECT DISTINCT b."IdGroupClass" AS id_later
    FROM "GroupClass" a
    JOIN "GroupClass" b
      ON a."IdGymHall" = b."IdGymHall"
     AND a."IdDayOfWeek" = b."IdDayOfWeek"
     AND a."IdGroupClass" < b."IdGroupClass"
    WHERE a."IsActive"
      AND b."IsActive"
      AND a."StartTime" < (b."StartTime" + (b."DurationMinutes" || ' minutes')::interval)
      AND b."StartTime" < (a."StartTime" + (a."DurationMinutes" || ' minutes')::interval)
) conflicts
WHERE gc."IdGroupClass" = conflicts.id_later;

COMMIT;

-- Отчёт после исправления
SELECT 'active_overlaps_remaining' AS check_name, COUNT(*)::text AS value
FROM "GroupClass" a
JOIN "GroupClass" b
  ON a."IdGymHall" = b."IdGymHall"
 AND a."IdDayOfWeek" = b."IdDayOfWeek"
 AND a."IdGroupClass" < b."IdGroupClass"
WHERE a."IsActive"
  AND b."IsActive"
  AND a."StartTime" < (b."StartTime" + (b."DurationMinutes" || ' minutes')::interval)
  AND b."StartTime" < (a."StartTime" + (a."DurationMinutes" || ' minutes')::interval)
UNION ALL
SELECT 'capacity_violations_remaining', COUNT(*)::text
FROM "GroupClass" gc
JOIN "GymHall" h ON h."IdGymHall" = gc."IdGymHall"
WHERE gc."IsActive"
  AND gc."MaxParticipants" > h."Capacity";
