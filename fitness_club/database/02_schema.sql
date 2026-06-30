-- Схема ИС «Фитнес-клуб» (3НФ)
-- psql -h localhost -p 5432 -U YOUR_USER -d Fitness_Club -f DatabaseAssets/fitness_club/02_schema.sql

-- ===================== Справочники =====================

CREATE TABLE "UserRole" (
    "IdUserRole" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "RoleName"   character varying(50) NOT NULL,
    CONSTRAINT "UserRole_RoleName_key" UNIQUE ("RoleName")
);

CREATE TABLE "Specialization" (
    "IdSpecialization"   integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "SpecializationName" character varying(100) NOT NULL,
    CONSTRAINT "Specialization_SpecializationName_key" UNIQUE ("SpecializationName")
);

CREATE TABLE "GymHall" (
    "IdGymHall" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "HallName"  character varying(100) NOT NULL,
    "Capacity"  integer NOT NULL,
    CONSTRAINT "GymHall_HallName_key" UNIQUE ("HallName"),
    CONSTRAINT "GymHall_Capacity_check" CHECK ("Capacity" > 0)
);

CREATE TABLE "DayOfWeek" (
    "IdDayOfWeek" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "DayName"     character varying(20) NOT NULL,
    "DayNumber"   smallint NOT NULL,
    CONSTRAINT "DayOfWeek_DayName_key" UNIQUE ("DayName"),
    CONSTRAINT "DayOfWeek_DayNumber_key" UNIQUE ("DayNumber"),
    CONSTRAINT "DayOfWeek_DayNumber_check" CHECK ("DayNumber" BETWEEN 1 AND 7)
);

CREATE TABLE "MembershipStatus" (
    "IdMembershipStatus" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "StatusCode"         character varying(30) NOT NULL,
    "StatusName"         character varying(100) NOT NULL,
    CONSTRAINT "MembershipStatus_StatusCode_key" UNIQUE ("StatusCode"),
    CONSTRAINT "MembershipStatus_StatusName_key" UNIQUE ("StatusName")
);

CREATE TABLE "MembershipType" (
    "IdMembershipType" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "TypeName"         character varying(100) NOT NULL,
    "Price"            numeric(10, 2) NOT NULL,
    "DurationDays"     integer NOT NULL,
    "VisitLimit"       integer,
    CONSTRAINT "MembershipType_TypeName_key" UNIQUE ("TypeName"),
    CONSTRAINT "MembershipType_Price_check" CHECK ("Price" >= 0),
    CONSTRAINT "MembershipType_DurationDays_check" CHECK ("DurationDays" > 0),
    CONSTRAINT "MembershipType_VisitLimit_check" CHECK ("VisitLimit" IS NULL OR "VisitLimit" > 0)
);

-- ===================== Субъекты (персональные данные вынесены из UserAccount) =====================

CREATE TABLE "Client" (
    "IdClient"   integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "LastName"   character varying(100) NOT NULL,
    "FirstName"  character varying(100) NOT NULL,
    "Patronymic" character varying(100),
    "Phone"      character varying(20) NOT NULL,
    "BirthDate"  date,
    "RegisteredAt" timestamp without time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    CONSTRAINT "Client_Phone_key" UNIQUE ("Phone")
);

CREATE TABLE "Trainer" (
    "IdTrainer"  integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "LastName"   character varying(100) NOT NULL,
    "FirstName"  character varying(100) NOT NULL,
    "Patronymic" character varying(100),
    "Phone"      character varying(20),
    "HiredAt"    date NOT NULL DEFAULT CURRENT_DATE,
    CONSTRAINT "Trainer_Phone_key" UNIQUE ("Phone")
);

CREATE TABLE "Staff" (
    "IdStaff"    integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "LastName"   character varying(100) NOT NULL,
    "FirstName"  character varying(100) NOT NULL,
    "Patronymic" character varying(100),
    "Phone"      character varying(20),
    "HiredAt"    date NOT NULL DEFAULT CURRENT_DATE,
    CONSTRAINT "Staff_Phone_key" UNIQUE ("Phone")
);

-- M:N тренер — специализация (специализация вынесена в отдельный справочник)
CREATE TABLE "TrainerSpecialization" (
    "IdTrainer"        integer NOT NULL,
    "IdSpecialization" integer NOT NULL,
    PRIMARY KEY ("IdTrainer", "IdSpecialization"),
    CONSTRAINT "FK_TrainerSpecialization_Trainer" FOREIGN KEY ("IdTrainer")
        REFERENCES "Trainer" ("IdTrainer") ON DELETE CASCADE,
    CONSTRAINT "FK_TrainerSpecialization_Specialization" FOREIGN KEY ("IdSpecialization")
        REFERENCES "Specialization" ("IdSpecialization") ON DELETE CASCADE
);

-- ===================== Учётные записи =====================

CREATE TABLE "UserAccount" (
    "IdUserAccount" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Login"         character varying(50) NOT NULL,
    "PasswordHash"  character varying(255) NOT NULL,
    "IdUserRole"    integer NOT NULL,
    "IdClient"      integer,
    "IdTrainer"     integer,
    "IdStaff"       integer,
    CONSTRAINT "UserAccount_Login_key" UNIQUE ("Login"),
    CONSTRAINT "FK_UserAccount_UserRole" FOREIGN KEY ("IdUserRole")
        REFERENCES "UserRole" ("IdUserRole"),
    CONSTRAINT "FK_UserAccount_Client" FOREIGN KEY ("IdClient")
        REFERENCES "Client" ("IdClient") ON DELETE SET NULL,
    CONSTRAINT "FK_UserAccount_Trainer" FOREIGN KEY ("IdTrainer")
        REFERENCES "Trainer" ("IdTrainer") ON DELETE SET NULL,
    CONSTRAINT "FK_UserAccount_Staff" FOREIGN KEY ("IdStaff")
        REFERENCES "Staff" ("IdStaff") ON DELETE SET NULL,
    CONSTRAINT "UserAccount_profile_xor_check" CHECK (
        ("IdClient" IS NOT NULL)::int
        + ("IdTrainer" IS NOT NULL)::int
        + ("IdStaff" IS NOT NULL)::int <= 1
    )
);

-- Ровно одна привязка к профилю в зависимости от роли
CREATE OR REPLACE FUNCTION "fn_UserAccount_role_profile_check"()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
    role_name text;
BEGIN
    SELECT "RoleName" INTO role_name FROM "UserRole" WHERE "IdUserRole" = NEW."IdUserRole";

    IF role_name = 'Клиент' THEN
        IF NEW."IdClient" IS NULL OR NEW."IdTrainer" IS NOT NULL OR NEW."IdStaff" IS NOT NULL THEN
            RAISE EXCEPTION 'Роль «Клиент» требует только IdClient';
        END IF;
    ELSIF role_name = 'Тренер' THEN
        IF NEW."IdTrainer" IS NULL OR NEW."IdClient" IS NOT NULL OR NEW."IdStaff" IS NOT NULL THEN
            RAISE EXCEPTION 'Роль «Тренер» требует только IdTrainer';
        END IF;
    ELSIF role_name IN ('Администратор', 'Ресепшн') THEN
        IF NEW."IdStaff" IS NULL OR NEW."IdClient" IS NOT NULL OR NEW."IdTrainer" IS NOT NULL THEN
            RAISE EXCEPTION 'Роли «Администратор»/«Ресепшн» требуют только IdStaff';
        END IF;
  END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER "trg_UserAccount_role_profile_check"
    BEFORE INSERT OR UPDATE ON "UserAccount"
    FOR EACH ROW
    EXECUTE FUNCTION "fn_UserAccount_role_profile_check"();

-- ===================== Абонементы =====================

CREATE TABLE "Membership" (
    "IdMembership"       integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "IdClient"           integer NOT NULL,
    "IdMembershipType"   integer NOT NULL,
    "StartDate"          date NOT NULL,
    "EndDate"            date NOT NULL,
    "IdMembershipStatus" integer NOT NULL,
    "SoldAt"             timestamp without time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "IdSoldByUser"       integer,
    CONSTRAINT "FK_Membership_Client" FOREIGN KEY ("IdClient")
        REFERENCES "Client" ("IdClient"),
    CONSTRAINT "FK_Membership_MembershipType" FOREIGN KEY ("IdMembershipType")
        REFERENCES "MembershipType" ("IdMembershipType"),
    CONSTRAINT "FK_Membership_MembershipStatus" FOREIGN KEY ("IdMembershipStatus")
        REFERENCES "MembershipStatus" ("IdMembershipStatus"),
    CONSTRAINT "FK_Membership_SoldByUser" FOREIGN KEY ("IdSoldByUser")
        REFERENCES "UserAccount" ("IdUserAccount"),
    CONSTRAINT "Membership_dates_check" CHECK ("EndDate" >= "StartDate")
);

CREATE TABLE "MembershipStatusHistory" (
    "IdMembershipStatusHistory" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "IdMembership"              integer NOT NULL,
    "IdMembershipStatus"        integer NOT NULL,
    "ChangedAt"                 timestamp without time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "IdChangedByUser"           integer,
    "Comment"                   text,
    CONSTRAINT "FK_MembershipStatusHistory_Membership" FOREIGN KEY ("IdMembership")
        REFERENCES "Membership" ("IdMembership") ON DELETE CASCADE,
    CONSTRAINT "FK_MembershipStatusHistory_MembershipStatus" FOREIGN KEY ("IdMembershipStatus")
        REFERENCES "MembershipStatus" ("IdMembershipStatus"),
    CONSTRAINT "FK_MembershipStatusHistory_User" FOREIGN KEY ("IdChangedByUser")
        REFERENCES "UserAccount" ("IdUserAccount")
);

-- ===================== Посещения =====================

CREATE TABLE "Visit" (
    "IdVisit"         integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "IdClient"        integer NOT NULL,
    "VisitDateTime"   timestamp without time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "IdMarkedByUser"  integer NOT NULL,
    "IdMembership"    integer,
    CONSTRAINT "FK_Visit_Client" FOREIGN KEY ("IdClient")
        REFERENCES "Client" ("IdClient"),
    CONSTRAINT "FK_Visit_MarkedByUser" FOREIGN KEY ("IdMarkedByUser")
        REFERENCES "UserAccount" ("IdUserAccount"),
    CONSTRAINT "FK_Visit_Membership" FOREIGN KEY ("IdMembership")
        REFERENCES "Membership" ("IdMembership") ON DELETE SET NULL
);

-- ===================== Групповые занятия и запись =====================

CREATE TABLE "GroupClass" (
    "IdGroupClass"    integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClassName"       character varying(150) NOT NULL,
    "IdGymHall"       integer NOT NULL,
    "IdTrainer"       integer NOT NULL,
    "IdDayOfWeek"     integer NOT NULL,
    "StartTime"       time without time zone NOT NULL,
    "DurationMinutes" integer NOT NULL DEFAULT 60,
    "MaxParticipants" integer NOT NULL,
    "IsActive"        boolean NOT NULL DEFAULT true,
    CONSTRAINT "FK_GroupClass_GymHall" FOREIGN KEY ("IdGymHall")
        REFERENCES "GymHall" ("IdGymHall"),
    CONSTRAINT "FK_GroupClass_Trainer" FOREIGN KEY ("IdTrainer")
        REFERENCES "Trainer" ("IdTrainer"),
    CONSTRAINT "FK_GroupClass_DayOfWeek" FOREIGN KEY ("IdDayOfWeek")
        REFERENCES "DayOfWeek" ("IdDayOfWeek"),
    CONSTRAINT "GroupClass_DurationMinutes_check" CHECK ("DurationMinutes" > 0),
    CONSTRAINT "GroupClass_MaxParticipants_check" CHECK ("MaxParticipants" > 0)
);

CREATE TABLE "ClassBooking" (
    "IdClassBooking" integer GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "IdClient"       integer NOT NULL,
    "IdGroupClass"   integer NOT NULL,
    "ClassDate"      date NOT NULL,
    "BookedAt"       timestamp without time zone NOT NULL DEFAULT (now() AT TIME ZONE 'UTC'),
    "IdBookedByUser" integer NOT NULL,
    CONSTRAINT "FK_ClassBooking_Client" FOREIGN KEY ("IdClient")
        REFERENCES "Client" ("IdClient"),
    CONSTRAINT "FK_ClassBooking_GroupClass" FOREIGN KEY ("IdGroupClass")
        REFERENCES "GroupClass" ("IdGroupClass") ON DELETE CASCADE,
    CONSTRAINT "FK_ClassBooking_BookedByUser" FOREIGN KEY ("IdBookedByUser")
        REFERENCES "UserAccount" ("IdUserAccount"),
    CONSTRAINT "ClassBooking_client_class_date_key" UNIQUE ("IdClient", "IdGroupClass", "ClassDate")
);

-- Лимит мест на занятие в конкретную дату
CREATE OR REPLACE FUNCTION "fn_ClassBooking_capacity_check"()
RETURNS trigger
LANGUAGE plpgsql
AS $$
DECLARE
    max_places integer;
    booked_count integer;
BEGIN
    SELECT "MaxParticipants" INTO max_places
    FROM "GroupClass"
    WHERE "IdGroupClass" = NEW."IdGroupClass";

    SELECT COUNT(*) INTO booked_count
    FROM "ClassBooking"
    WHERE "IdGroupClass" = NEW."IdGroupClass"
      AND "ClassDate" = NEW."ClassDate"
      AND ("IdClassBooking" IS DISTINCT FROM NEW."IdClassBooking");

    IF booked_count >= max_places THEN
        RAISE EXCEPTION 'Нет свободных мест на занятие (лимит %)', max_places;
    END IF;

    RETURN NEW;
END;
$$;

CREATE TRIGGER "trg_ClassBooking_capacity_check"
    BEFORE INSERT OR UPDATE ON "ClassBooking"
    FOR EACH ROW
    EXECUTE FUNCTION "fn_ClassBooking_capacity_check"();

-- Индексы для частых запросов
CREATE INDEX "IX_Membership_Client" ON "Membership" ("IdClient");
CREATE INDEX "IX_Membership_Status" ON "Membership" ("IdMembershipStatus");
CREATE INDEX "IX_Visit_Client_DateTime" ON "Visit" ("IdClient", "VisitDateTime");
CREATE INDEX "IX_ClassBooking_GroupClass_Date" ON "ClassBooking" ("IdGroupClass", "ClassDate");
CREATE INDEX "IX_UserAccount_Role" ON "UserAccount" ("IdUserRole");
