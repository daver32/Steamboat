DROP TABLE IF EXISTS Apps;
CREATE TABLE "Apps"
(
    "Id"                     INTEGER NOT NULL UNIQUE,
    "Name"                   TEXT    NOT NULL,
    "LastDiscountPercentage" REAL,
    "LastModified"           INTEGER,
    "PriceChangeNumber"      INTEGER, 
    "PriceFetchId"           TEXT NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000',
    PRIMARY KEY ("Id")
);

DROP TABLE IF EXISTS NotificationJobs;
CREATE TABLE "NotificationJobs"
(
    "Id"               INTEGER NOT NULL UNIQUE,
    "AppName"          TEXT,
    "AppId"            INTEGER,
    "CreatedUtc"       INTEGER,
    "HasBeenProcessed" INTEGER,
    PRIMARY KEY ("Id" AUTOINCREMENT)
);

DROP TABLE IF EXISTS StateEntries;
CREATE TABLE "StateEntries"
(
    "Key"   TEXT NOT NULL UNIQUE,
    "Value" TEXT,
    PRIMARY KEY ("Key")
);