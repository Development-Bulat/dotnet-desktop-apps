-- Уведомления для клиентов и тренеров
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/10_notifications.sql

CREATE TABLE IF NOT EXISTS "Notification" (
    "IdNotification" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "IdUserAccount"  integer NOT NULL,
    "Title"          character varying(200) NOT NULL,
    "Message"        text NOT NULL,
    "CreatedAt"      timestamp without time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "IsRead"         boolean NOT NULL DEFAULT false,
    CONSTRAINT "FK_Notification_UserAccount"
        FOREIGN KEY ("IdUserAccount") REFERENCES "UserAccount" ("IdUserAccount")
        ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_Notification_UserAccount_Read"
    ON "Notification" ("IdUserAccount", "IsRead");

CREATE INDEX IF NOT EXISTS "IX_Notification_UserAccount_CreatedAt"
    ON "Notification" ("IdUserAccount", "CreatedAt" DESC);
