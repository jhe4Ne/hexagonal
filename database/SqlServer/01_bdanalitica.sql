

IF DB_ID('bdanalitica') IS NULL
BEGIN
    CREATE DATABASE bdanalitica;
END
GO

USE bdanalitica;
GO

IF SCHEMA_ID('analitica') IS NULL
BEGIN
    EXEC('CREATE SCHEMA analitica');
END
GO

IF OBJECT_ID('analitica.SincronizacionLog') IS NULL
BEGIN
    CREATE TABLE analitica.SincronizacionLog (
        Id                   UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        Origin               NVARCHAR(20)     NOT NULL,
        Endpoint             NVARCHAR(120)    NOT NULL,
        Platform             NVARCHAR(10)     NULL,
        Successful           BIT              NOT NULL,
        Message              NVARCHAR(1000)   NULL,
        ProcessedRecords     INT              NOT NULL DEFAULT 0,
        ElapsedMilliseconds  BIGINT           NOT NULL DEFAULT 0,
        ExecutedAt           DATETIME2        NOT NULL,
        IsActive             BIT              NOT NULL DEFAULT 1,
        CreatedAt            DATETIME2        NOT NULL,
        UpdatedAt            DATETIME2        NULL
    );

    CREATE INDEX IX_SincronizacionLog_ExecutedAt
        ON analitica.SincronizacionLog (ExecutedAt DESC);
END
GO

IF OBJECT_ID('analitica.MaestriaSnapshot') IS NULL
BEGIN
    CREATE TABLE analitica.MaestriaSnapshot (
        Id              UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
        MaskedPuuid     NVARCHAR(20)     NOT NULL,
        ChampionKey     INT              NOT NULL,
        ChampionName    NVARCHAR(80)     NULL,
        Points          BIGINT           NOT NULL DEFAULT 0,
        [Level]         INT              NOT NULL DEFAULT 0,
        DominanceIndex  DECIMAL(5,2)     NOT NULL DEFAULT 0,
        TakenAt         DATETIME2        NOT NULL,
        IsActive        BIT              NOT NULL DEFAULT 1,
        CreatedAt       DATETIME2        NOT NULL,
        UpdatedAt       DATETIME2        NULL
    );

    CREATE INDEX IX_MaestriaSnapshot_Jugador
        ON analitica.MaestriaSnapshot (MaskedPuuid, ChampionKey, TakenAt);
END
GO
