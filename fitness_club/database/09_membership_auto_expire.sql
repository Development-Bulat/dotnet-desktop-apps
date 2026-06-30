-- Автоматическое истечение абонементов на уровне PostgreSQL.
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/09_membership_auto_expire.sql
--
-- Важно: обычный триггер не срабатывает «в полночь сам по себе» — только при событии в БД.
-- Этот скрипт:
--   1) переводит просроченные абонементы в «Истёк» при визите, записи на занятие или изменении абонемента;
--   2) не даёт оставить статус «Активен», если EndDate уже прошла;
--   3) (опционально) можно повесить ежедневный job через pg_cron — см. конец файла.

CREATE OR REPLACE FUNCTION "fn_ExpireOutdatedMemberships"()
RETURNS void
LANGUAGE plpgsql
AS $$
DECLARE
    active_id  integer;
    expired_id integer;
    rec        record;
BEGIN
    SELECT "IdMembershipStatus" INTO active_id
    FROM "MembershipStatus"
    WHERE "StatusCode" = 'active';

    SELECT "IdMembershipStatus" INTO expired_id
    FROM "MembershipStatus"
    WHERE "StatusCode" = 'expired';

    FOR rec IN
        SELECT m."IdMembership"
        FROM "Membership" m
        WHERE m."IdMembershipStatus" = active_id
          AND m."EndDate" < CURRENT_DATE
    LOOP
        UPDATE "Membership"
        SET "IdMembershipStatus" = expired_id
        WHERE "IdMembership" = rec."IdMembership"
          AND "IdMembershipStatus" = active_id;

        IF NOT EXISTS (
            SELECT 1
            FROM "MembershipStatusHistory" h
            WHERE h."IdMembership" = rec."IdMembership"
              AND h."IdMembershipStatus" = expired_id
              AND h."Comment" = 'Автоматическое истечение срока'
              AND h."ChangedAt"::date = CURRENT_DATE
        ) THEN
            INSERT INTO "MembershipStatusHistory" (
                "IdMembership",
                "IdMembershipStatus",
                "ChangedAt",
                "Comment"
            )
            VALUES (
                rec."IdMembership",
                expired_id,
                (now() AT TIME ZONE 'UTC'),
                'Автоматическое истечение срока'
            );
        END IF;
    END LOOP;
END;
$$;

CREATE OR REPLACE FUNCTION "fn_ExpireOutdatedMemberships_trigger"()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    PERFORM "fn_ExpireOutdatedMemberships"();
    RETURN NULL;
END;
$$;

CREATE OR REPLACE FUNCTION "fn_Membership_row_expire_check"()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
    active_id  integer;
    expired_id integer;
BEGIN
    SELECT "IdMembershipStatus" INTO active_id
    FROM "MembershipStatus"
    WHERE "StatusCode" = 'active';

    SELECT "IdMembershipStatus" INTO expired_id
    FROM "MembershipStatus"
    WHERE "StatusCode" = 'expired';

    IF NEW."IdMembershipStatus" = active_id AND NEW."EndDate" < CURRENT_DATE THEN
        NEW."IdMembershipStatus" := expired_id;
    END IF;

    RETURN NEW;
END;
$$;

DROP TRIGGER IF EXISTS "trg_visit_expire_memberships" ON "Visit";
CREATE TRIGGER "trg_visit_expire_memberships"
    BEFORE INSERT ON "Visit"
    FOR EACH STATEMENT
    EXECUTE FUNCTION "fn_ExpireOutdatedMemberships_trigger"();

DROP TRIGGER IF EXISTS "trg_classbooking_expire_memberships" ON "ClassBooking";
CREATE TRIGGER "trg_classbooking_expire_memberships"
    BEFORE INSERT ON "ClassBooking"
    FOR EACH STATEMENT
    EXECUTE FUNCTION "fn_ExpireOutdatedMemberships_trigger"();

DROP TRIGGER IF EXISTS "trg_membership_expire_on_write" ON "Membership";
CREATE TRIGGER "trg_membership_expire_on_write"
    BEFORE INSERT OR UPDATE ON "Membership"
    FOR EACH STATEMENT
    EXECUTE FUNCTION "fn_ExpireOutdatedMemberships_trigger"();

DROP TRIGGER IF EXISTS "trg_membership_row_expire_check" ON "Membership";
CREATE TRIGGER "trg_membership_row_expire_check"
    BEFORE INSERT OR UPDATE ON "Membership"
    FOR EACH ROW
    EXECUTE FUNCTION "fn_Membership_row_expire_check"();

-- Прогон при установке: обновить уже просроченные записи.
SELECT "fn_ExpireOutdatedMemberships"();

-- ---------------------------------------------------------------------------
-- Опционально: истечение каждый день в 00:05 (нужно расширение pg_cron).
-- Раскомментируйте, если pg_cron установлен в PostgreSQL:
--
-- CREATE EXTENSION IF NOT EXISTS pg_cron;
-- SELECT cron.unschedule(jobid)
-- FROM cron.job
-- WHERE jobname = 'fitness_club_expire_memberships';
-- SELECT cron.schedule(
--     'fitness_club_expire_memberships',
--     '5 0 * * *',
--     $$SELECT public."fn_ExpireOutdatedMemberships"()$$
-- );
