CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

CREATE TABLE "Assets" (
    "Id" uuid NOT NULL,
    "Name" character varying(120) NOT NULL,
    "Description" character varying(500),
    "Url" character varying(2048) NOT NULL,
    "AssetType" character varying(20) NOT NULL,
    "Category" character varying(30) NOT NULL,
    "DefaultVolume" integer NOT NULL,
    "IsPremium" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Assets" PRIMARY KEY ("Id")
);

CREATE TABLE "Rooms" (
    "Id" uuid NOT NULL,
    "Name" character varying(120) NOT NULL,
    "Description" character varying(500),
    "ThumbnailUrl" character varying(2048),
    "BackgroundUrl" character varying(2048),
    "IsPremium" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Rooms" PRIMARY KEY ("Id")
);

CREATE TABLE "Users" (
    "Id" uuid NOT NULL,
    "Username" character varying(50) NOT NULL,
    "Email" character varying(256) NOT NULL,
    "PasswordHash" character varying(512) NOT NULL,
    "AvatarUrl" character varying(2048),
    "Role" character varying(20) NOT NULL,
    "AccountTier" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
);

CREATE TABLE "RoomAssetMappings" (
    "Id" uuid NOT NULL,
    "RoomId" uuid NOT NULL,
    "AssetId" uuid NOT NULL,
    "DefaultPositionX" double precision NOT NULL,
    "DefaultPositionY" double precision NOT NULL,
    "DefaultScale" double precision NOT NULL,
    "DefaultOpacity" double precision NOT NULL,
    "DefaultLayerIndex" integer NOT NULL,
    CONSTRAINT "PK_RoomAssetMappings" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RoomAssetMappings_Assets_AssetId" FOREIGN KEY ("AssetId") REFERENCES "Assets" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_RoomAssetMappings_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE CASCADE
);

CREATE TABLE "PomodoroSessions" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "StartTime" timestamp with time zone NOT NULL,
    "EndTime" timestamp with time zone,
    "DurationMinutes" integer NOT NULL,
    CONSTRAINT "PK_PomodoroSessions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PomodoroSessions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RefreshTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Token" character varying(512) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "IsRevoked" boolean NOT NULL,
    CONSTRAINT "PK_RefreshTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RefreshTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Todos" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Content" character varying(500) NOT NULL,
    "IsCompleted" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_Todos" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Todos_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserRoomConfigs" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "RoomId" uuid NOT NULL,
    "JsonConfig" text NOT NULL,
    "UpdatedAt" timestamp with time zone NOT NULL,
    CONSTRAINT "PK_UserRoomConfigs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserRoomConfigs_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserRoomConfigs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Assets_AssetType" ON "Assets" ("AssetType");

CREATE INDEX "IX_Assets_Category" ON "Assets" ("Category");

CREATE INDEX "IX_PomodoroSessions_UserId_EndTime" ON "PomodoroSessions" ("UserId", "EndTime");

CREATE UNIQUE INDEX "IX_RefreshTokens_Token" ON "RefreshTokens" ("Token");

CREATE INDEX "IX_RefreshTokens_UserId" ON "RefreshTokens" ("UserId");

CREATE INDEX "IX_RoomAssetMappings_AssetId" ON "RoomAssetMappings" ("AssetId");

CREATE UNIQUE INDEX "IX_RoomAssetMappings_RoomId_AssetId" ON "RoomAssetMappings" ("RoomId", "AssetId");

CREATE INDEX "IX_Todos_UserId" ON "Todos" ("UserId");

CREATE INDEX "IX_UserRoomConfigs_RoomId" ON "UserRoomConfigs" ("RoomId");

CREATE UNIQUE INDEX "IX_UserRoomConfigs_UserId_RoomId" ON "UserRoomConfigs" ("UserId", "RoomId");

CREATE UNIQUE INDEX "IX_Users_Email" ON "Users" ("Email");

CREATE UNIQUE INDEX "IX_Users_Username" ON "Users" ("Username");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260518114852_InitPostgres', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "Users" ADD "CoinsBalance" integer NOT NULL DEFAULT 0;

ALTER TABLE "Users" ADD "CreatedBy" uuid;

ALTER TABLE "Users" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "Users" ADD "DeletedBy" uuid;

ALTER TABLE "Users" ADD "IsBanned" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "Users" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "Users" ADD "LastLoginAt" timestamp with time zone;

ALTER TABLE "Users" ADD "RoleId" uuid;

ALTER TABLE "Users" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "Users" ADD "UpdatedBy" uuid;

ALTER TABLE "UserRoomConfigs" ALTER COLUMN "UpdatedAt" DROP NOT NULL;

ALTER TABLE "UserRoomConfigs" ADD "CreatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "UserRoomConfigs" ADD "CreatedBy" uuid;

ALTER TABLE "UserRoomConfigs" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "UserRoomConfigs" ADD "DeletedBy" uuid;

ALTER TABLE "UserRoomConfigs" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "UserRoomConfigs" ADD "UpdatedBy" uuid;

ALTER TABLE "Todos" ADD "CreatedBy" uuid;

ALTER TABLE "Todos" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "Todos" ADD "DeletedBy" uuid;

ALTER TABLE "Todos" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "Todos" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "Todos" ADD "UpdatedBy" uuid;

ALTER TABLE "Rooms" ADD "CreatedBy" uuid;

ALTER TABLE "Rooms" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "Rooms" ADD "DeletedBy" uuid;

ALTER TABLE "Rooms" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "Rooms" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "Rooms" ADD "UpdatedBy" uuid;

ALTER TABLE "RoomAssetMappings" ADD "CreatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "RoomAssetMappings" ADD "CreatedBy" uuid;

ALTER TABLE "RoomAssetMappings" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "RoomAssetMappings" ADD "DeletedBy" uuid;

ALTER TABLE "RoomAssetMappings" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "RoomAssetMappings" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "RoomAssetMappings" ADD "UpdatedBy" uuid;

ALTER TABLE "RefreshTokens" ADD "CreatedBy" uuid;

ALTER TABLE "RefreshTokens" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "RefreshTokens" ADD "DeletedBy" uuid;

ALTER TABLE "RefreshTokens" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "RefreshTokens" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "RefreshTokens" ADD "UpdatedBy" uuid;

ALTER TABLE "PomodoroSessions" ADD "CreatedAt" timestamp with time zone NOT NULL DEFAULT TIMESTAMPTZ '-infinity';

ALTER TABLE "PomodoroSessions" ADD "CreatedBy" uuid;

ALTER TABLE "PomodoroSessions" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "PomodoroSessions" ADD "DeletedBy" uuid;

ALTER TABLE "PomodoroSessions" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "PomodoroSessions" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "PomodoroSessions" ADD "UpdatedBy" uuid;

ALTER TABLE "Assets" ADD "CreatedBy" uuid;

ALTER TABLE "Assets" ADD "DeletedAt" timestamp with time zone;

ALTER TABLE "Assets" ADD "DeletedBy" uuid;

ALTER TABLE "Assets" ADD "IsDeleted" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "Assets" ADD "UpdatedAt" timestamp with time zone;

ALTER TABLE "Assets" ADD "UpdatedBy" uuid;

CREATE TABLE "ActivityLogs" (
    "Id" uuid NOT NULL,
    "UserId" uuid,
    "Action" character varying(120) NOT NULL,
    "EntityName" character varying(80),
    "EntityId" character varying(80),
    "MetadataJson" text,
    "IpAddress" character varying(60),
    "UserAgent" character varying(500),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_ActivityLogs" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_ActivityLogs_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE SET NULL
);

CREATE TABLE "Missions" (
    "Id" uuid NOT NULL,
    "Code" character varying(50) NOT NULL,
    "Name" character varying(120) NOT NULL,
    "Description" character varying(500),
    "RewardCoins" integer NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "TriggerKey" character varying(50) NOT NULL,
    "TargetValue" integer,
    "Frequency" character varying(20) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_Missions" PRIMARY KEY ("Id")
);

CREATE TABLE "PaymentTransactions" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Provider" character varying(20) NOT NULL,
    "Status" character varying(20) NOT NULL,
    "TransactionCode" character varying(64) NOT NULL,
    "Amount" bigint NOT NULL,
    "Currency" character varying(10) NOT NULL,
    "ProviderPayloadJson" text,
    "SucceededAt" timestamp with time zone,
    "FailedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_PaymentTransactions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PaymentTransactions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Roles" (
    "Id" uuid NOT NULL,
    "Name" character varying(50) NOT NULL,
    "Description" character varying(500),
    "IsSystem" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_Roles" PRIMARY KEY ("Id")
);

CREATE UNIQUE INDEX "IX_Roles_Name" ON "Roles" ("Name");

INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystem", "CreatedAt", "IsDeleted")
VALUES ('aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa1', 'Guest', 'Demo mode only; cannot persist layouts.', TRUE, TIMESTAMPTZ '2026-07-01T12:08:16.887845Z', FALSE);
INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystem", "CreatedAt", "IsDeleted")
VALUES ('aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa2', 'User', 'Freemium user.', TRUE, TIMESTAMPTZ '2026-07-01T12:08:16.887847Z', FALSE);
INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystem", "CreatedAt", "IsDeleted")
VALUES ('aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa3', 'PremiumUser', 'Premium user.', TRUE, TIMESTAMPTZ '2026-07-01T12:08:16.887848Z', FALSE);
INSERT INTO "Roles" ("Id", "Name", "Description", "IsSystem", "CreatedAt", "IsDeleted")
VALUES ('aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa4', 'Admin', 'Administrator.', TRUE, TIMESTAMPTZ '2026-07-01T12:08:16.887849Z', FALSE);


UPDATE "Users"
SET "RoleId" = CASE
  WHEN "Role" = 'Admin' THEN 'aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa4'::uuid
  ELSE 'aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa2'::uuid
END
WHERE "RoleId" IS NULL;


UPDATE "Users" SET "RoleId" = 'aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa2' WHERE "RoleId" IS NULL;
ALTER TABLE "Users" ALTER COLUMN "RoleId" SET NOT NULL;
ALTER TABLE "Users" ALTER COLUMN "RoleId" SET DEFAULT 'aaaaaaa1-aaaa-aaaa-aaaa-aaaaaaaaaaa2';

ALTER TABLE "Users" DROP COLUMN "Role";

CREATE TABLE "RoomLayouts" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Name" character varying(120) NOT NULL,
    "Description" character varying(500),
    "RoomId" uuid,
    "LayoutJson" text NOT NULL,
    "ThumbnailUrl" character varying(2048),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_RoomLayouts" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RoomLayouts_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_RoomLayouts_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "StoreItems" (
    "Id" uuid NOT NULL,
    "Category" character varying(30) NOT NULL,
    "Name" character varying(120) NOT NULL,
    "Description" character varying(500),
    "AssetUrl" character varying(2048) NOT NULL,
    "IsPremium" boolean NOT NULL,
    "CoinPrice" integer,
    "RealMoneyPriceVnd" bigint,
    "IsActive" boolean NOT NULL DEFAULT TRUE,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_StoreItems" PRIMARY KEY ("Id")
);

CREATE TABLE "UserMissions" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "MissionId" uuid NOT NULL,
    "ProgressValue" integer NOT NULL,
    "IsCompleted" boolean NOT NULL,
    "CompletedAt" timestamp with time zone,
    "PeriodDate" date NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_UserMissions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserMissions_Missions_MissionId" FOREIGN KEY ("MissionId") REFERENCES "Missions" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_UserMissions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Subscriptions" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "StartsAt" timestamp with time zone NOT NULL,
    "EndsAt" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL DEFAULT FALSE,
    "PaymentTransactionId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_Subscriptions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Subscriptions_PaymentTransactions_PaymentTransactionId" FOREIGN KEY ("PaymentTransactionId") REFERENCES "PaymentTransactions" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Subscriptions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "RoomThumbnails" (
    "Id" uuid NOT NULL,
    "RoomLayoutId" uuid NOT NULL,
    "Url" character varying(2048) NOT NULL,
    "PublicId" character varying(200),
    "Width" integer NOT NULL,
    "Height" integer NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_RoomThumbnails" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_RoomThumbnails_RoomLayouts_RoomLayoutId" FOREIGN KEY ("RoomLayoutId") REFERENCES "RoomLayouts" ("Id") ON DELETE CASCADE
);

CREATE TABLE "Purchases" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "StoreItemId" uuid,
    "CoinsSpent" integer,
    "AmountVnd" bigint,
    "Currency" character varying(10) NOT NULL,
    "PaymentTransactionId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_Purchases" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Purchases_PaymentTransactions_PaymentTransactionId" FOREIGN KEY ("PaymentTransactionId") REFERENCES "PaymentTransactions" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Purchases_StoreItems_StoreItemId" FOREIGN KEY ("StoreItemId") REFERENCES "StoreItems" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_Purchases_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "UserInventories" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "StoreItemId" uuid NOT NULL,
    "AcquiredAt" timestamp with time zone NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_UserInventories" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserInventories_StoreItems_StoreItemId" FOREIGN KEY ("StoreItemId") REFERENCES "StoreItems" ("Id") ON DELETE RESTRICT,
    CONSTRAINT "FK_UserInventories_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE TABLE "CoinTransactions" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Type" character varying(20) NOT NULL,
    "Amount" integer NOT NULL,
    "Reason" character varying(200) NOT NULL,
    "RelatedMissionId" uuid,
    "RelatedPurchaseId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_CoinTransactions" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_CoinTransactions_Missions_RelatedMissionId" FOREIGN KEY ("RelatedMissionId") REFERENCES "Missions" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CoinTransactions_Purchases_RelatedPurchaseId" FOREIGN KEY ("RelatedPurchaseId") REFERENCES "Purchases" ("Id") ON DELETE SET NULL,
    CONSTRAINT "FK_CoinTransactions_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Users_RoleId" ON "Users" ("RoleId");

CREATE INDEX "IX_ActivityLogs_Action_CreatedAt" ON "ActivityLogs" ("Action", "CreatedAt");

CREATE INDEX "IX_ActivityLogs_UserId" ON "ActivityLogs" ("UserId");

CREATE INDEX "IX_CoinTransactions_RelatedMissionId" ON "CoinTransactions" ("RelatedMissionId");

CREATE INDEX "IX_CoinTransactions_RelatedPurchaseId" ON "CoinTransactions" ("RelatedPurchaseId");

CREATE INDEX "IX_CoinTransactions_UserId_CreatedAt" ON "CoinTransactions" ("UserId", "CreatedAt");

CREATE UNIQUE INDEX "IX_Missions_Code" ON "Missions" ("Code");

CREATE INDEX "IX_Missions_IsActive_TriggerKey" ON "Missions" ("IsActive", "TriggerKey");

CREATE INDEX "IX_PaymentTransactions_Provider_Status" ON "PaymentTransactions" ("Provider", "Status");

CREATE UNIQUE INDEX "IX_PaymentTransactions_TransactionCode" ON "PaymentTransactions" ("TransactionCode");

CREATE INDEX "IX_PaymentTransactions_UserId" ON "PaymentTransactions" ("UserId");

CREATE INDEX "IX_Purchases_PaymentTransactionId" ON "Purchases" ("PaymentTransactionId");

CREATE INDEX "IX_Purchases_StoreItemId" ON "Purchases" ("StoreItemId");

CREATE INDEX "IX_Purchases_UserId" ON "Purchases" ("UserId");

CREATE INDEX "IX_RoomLayouts_RoomId" ON "RoomLayouts" ("RoomId");

CREATE INDEX "IX_RoomLayouts_UserId" ON "RoomLayouts" ("UserId");

CREATE INDEX "IX_RoomLayouts_UserId_CreatedAt" ON "RoomLayouts" ("UserId", "CreatedAt");

CREATE UNIQUE INDEX "IX_RoomThumbnails_RoomLayoutId" ON "RoomThumbnails" ("RoomLayoutId");

CREATE INDEX "IX_StoreItems_Category" ON "StoreItems" ("Category");

CREATE INDEX "IX_StoreItems_IsActive" ON "StoreItems" ("IsActive");

CREATE INDEX "IX_StoreItems_IsPremium" ON "StoreItems" ("IsPremium");

CREATE INDEX "IX_Subscriptions_EndsAt" ON "Subscriptions" ("EndsAt");

CREATE INDEX "IX_Subscriptions_PaymentTransactionId" ON "Subscriptions" ("PaymentTransactionId");

CREATE INDEX "IX_Subscriptions_UserId_IsActive" ON "Subscriptions" ("UserId", "IsActive");

CREATE INDEX "IX_UserInventories_StoreItemId" ON "UserInventories" ("StoreItemId");

CREATE INDEX "IX_UserInventories_UserId" ON "UserInventories" ("UserId");

CREATE UNIQUE INDEX "IX_UserInventories_UserId_StoreItemId" ON "UserInventories" ("UserId", "StoreItemId");

CREATE INDEX "IX_UserMissions_IsCompleted" ON "UserMissions" ("IsCompleted");

CREATE INDEX "IX_UserMissions_MissionId" ON "UserMissions" ("MissionId");

CREATE UNIQUE INDEX "IX_UserMissions_UserId_MissionId_PeriodDate" ON "UserMissions" ("UserId", "MissionId", "PeriodDate");

ALTER TABLE "Users" ADD CONSTRAINT "FK_Users_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES "Roles" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260528055632_P0_AddCoreDomainsAndAudit', '8.0.22');

COMMIT;

START TRANSACTION;

CREATE TABLE "PasswordResetTokens" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "TokenHash" character varying(128) NOT NULL,
    "ExpiresAt" timestamp with time zone NOT NULL,
    "UsedAt" timestamp with time zone,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_PasswordResetTokens" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_PasswordResetTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_PasswordResetTokens_ExpiresAt" ON "PasswordResetTokens" ("ExpiresAt");

CREATE UNIQUE INDEX "IX_PasswordResetTokens_TokenHash" ON "PasswordResetTokens" ("TokenHash");

CREATE INDEX "IX_PasswordResetTokens_UserId" ON "PasswordResetTokens" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260528060843_P1_AddPasswordResetTokens', '8.0.22');

COMMIT;

START TRANSACTION;

DELETE FROM "RefreshTokens";

ALTER TABLE "RefreshTokens" ADD "TokenHash" character varying(64) NOT NULL DEFAULT '';

CREATE UNIQUE INDEX "IX_RefreshTokens_TokenHash" ON "RefreshTokens" ("TokenHash");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260528061228_P1_HashRefreshTokens', '8.0.22');

COMMIT;

START TRANSACTION;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260528062658_P1_PaymentIntegrations', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "UserMissions" ADD "ClaimedAt" timestamp with time zone;

CREATE INDEX "IX_UserMissions_ClaimedAt" ON "UserMissions" ("ClaimedAt");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260528063451_P1_UserMissionClaimedAt', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "PaymentTransactions" ADD "IsFulfilled" boolean NOT NULL DEFAULT FALSE;

ALTER TABLE "PaymentTransactions" ADD "MetadataJson" text;

ALTER TABLE "PaymentTransactions" ADD "Purpose" character varying(20) NOT NULL DEFAULT '';

CREATE INDEX "IX_PaymentTransactions_Purpose_Status" ON "PaymentTransactions" ("Purpose", "Status");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260528064458_P1_PaymentPurposeAndFulfillment', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "Rooms" ADD "UserId" uuid;

CREATE INDEX "IX_Rooms_UserId" ON "Rooms" ("UserId");

ALTER TABLE "Rooms" ADD CONSTRAINT "FK_Rooms_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260531084353_P2_AddUserOwnedRooms', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "StoreItems" ADD "CreatorId" uuid;

ALTER TABLE "StoreItems" ADD "RejectionNote" character varying(1000);

ALTER TABLE "StoreItems" ADD "ReviewedAt" timestamp with time zone;

ALTER TABLE "StoreItems" ADD "Status" character varying(30) NOT NULL DEFAULT 'AdminCreated';

CREATE INDEX "IX_StoreItems_CreatorId" ON "StoreItems" ("CreatorId");

CREATE INDEX "IX_StoreItems_Status" ON "StoreItems" ("Status");

ALTER TABLE "StoreItems" ADD CONSTRAINT "FK_StoreItems_Users_CreatorId" FOREIGN KEY ("CreatorId") REFERENCES "Users" ("Id") ON DELETE RESTRICT;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260604020553_P3_UserThemeSubmission', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "StoreItems" ADD "ThemeAmbientSoundItemId" uuid;

ALTER TABLE "StoreItems" ADD "ThemeBackgroundItemId" uuid;

ALTER TABLE "StoreItems" ADD "ThemeEffectItemId" uuid;

ALTER TABLE "StoreItems" ADD "ThemeSource" character varying(20);

ALTER TABLE "StoreItems" ADD "ThemeStickerItemId" uuid;

CREATE INDEX "IX_StoreItems_ThemeSource" ON "StoreItems" ("ThemeSource");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260607053748_P4_ThemeComboAndSource', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "StoreItems" ADD "PreviewUrl" character varying(2048);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260610151941_AddStoreItemPreviewUrl', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "StoreItems" ADD "ParentThemeId" uuid;

CREATE INDEX "IX_StoreItems_ParentThemeId" ON "StoreItems" ("ParentThemeId");

ALTER TABLE "StoreItems" ADD CONSTRAINT "FK_StoreItems_StoreItems_ParentThemeId" FOREIGN KEY ("ParentThemeId") REFERENCES "StoreItems" ("Id") ON DELETE SET NULL;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260618080621_P5_UserComponentSubmission', '8.0.22');

COMMIT;

START TRANSACTION;

CREATE TABLE "Reports" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "Title" character varying(256) NOT NULL,
    "Content" character varying(4000) NOT NULL,
    "Status" character varying(50) NOT NULL DEFAULT 'Pending',
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_Reports" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_Reports_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_Reports_CreatedAt" ON "Reports" ("CreatedAt");

CREATE INDEX "IX_Reports_Status" ON "Reports" ("Status");

CREATE INDEX "IX_Reports_UserId" ON "Reports" ("UserId");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260618101749_P6_AddReport', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "Reports" ADD "AttachmentUrl" character varying(2048);

ALTER TABLE "Reports" ADD "Type" character varying(50) NOT NULL DEFAULT 'Feedback';

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260618102927_P7_EnhanceReport', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "Assets" ADD "PreviewUrl" character varying(2048);

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260623144920_AddAssetPreviewUrl', '8.0.22');

COMMIT;

START TRANSACTION;

ALTER TABLE "Users" ADD "LastRetentionEmailSentAt" timestamp with time zone;

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260701074746_AddUserLastRetentionEmailSentAt', '8.0.22');

COMMIT;

START TRANSACTION;

CREATE TABLE IF NOT EXISTS "UserLuckyDraws" (
    "Id" uuid NOT NULL,
    "UserId" uuid NOT NULL,
    "DrawDate" date NOT NULL,
    "RewardCoins" integer NOT NULL,
    "RewardDescription" character varying(255) NOT NULL DEFAULT '',
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone,
    "IsDeleted" boolean NOT NULL DEFAULT FALSE,
    "DeletedAt" timestamp with time zone,
    "CreatedBy" uuid,
    "UpdatedBy" uuid,
    "DeletedBy" uuid,
    CONSTRAINT "PK_UserLuckyDraws" PRIMARY KEY ("Id"),
    CONSTRAINT "FK_UserLuckyDraws_Users_UserId" FOREIGN KEY ("UserId") REFERENCES "Users" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserLuckyDraws_UserId_DrawDate" ON "UserLuckyDraws" ("UserId", "DrawDate");

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES ('20260801120000_AddUserLuckyDraws', '8.0.22');

COMMIT;


