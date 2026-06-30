-- Дополнительные каскадные связи (для уже развёрнутой БД)
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/12_cascade_fks.sql

ALTER TABLE "ClassBooking"
    DROP CONSTRAINT IF EXISTS "FK_ClassBooking_GroupClass",
    ADD CONSTRAINT "FK_ClassBooking_GroupClass"
        FOREIGN KEY ("IdGroupClass") REFERENCES "GroupClass" ("IdGroupClass")
        ON DELETE CASCADE;

ALTER TABLE "Visit"
    DROP CONSTRAINT IF EXISTS "FK_Visit_Membership",
    ADD CONSTRAINT "FK_Visit_Membership"
        FOREIGN KEY ("IdMembership") REFERENCES "Membership" ("IdMembership")
        ON DELETE SET NULL;
