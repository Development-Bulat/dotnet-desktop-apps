-- Отменённые занятия (конкретная дата + групповое занятие)
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/11_cancelled_class_sessions.sql

CREATE TABLE IF NOT EXISTS "CancelledClassSession" (
    "IdCancelledClassSession" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "IdGroupClass"            integer NOT NULL,
    "ClassDate"               date NOT NULL,
    "CancelledAt"             timestamp without time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "IdCancelledByUser"       integer NOT NULL,
    CONSTRAINT "CancelledClassSession_class_date_key" UNIQUE ("IdGroupClass", "ClassDate"),
    CONSTRAINT "FK_CancelledClassSession_GroupClass"
        FOREIGN KEY ("IdGroupClass") REFERENCES "GroupClass" ("IdGroupClass")
        ON DELETE CASCADE,
    CONSTRAINT "FK_CancelledClassSession_User"
        FOREIGN KEY ("IdCancelledByUser") REFERENCES "UserAccount" ("IdUserAccount")
);

CREATE INDEX IF NOT EXISTS "IX_CancelledClassSession_Date"
    ON "CancelledClassSession" ("ClassDate");
