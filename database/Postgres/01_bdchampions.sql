

CREATE SCHEMA IF NOT EXISTS champions;

CREATE TABLE IF NOT EXISTS champions.champion_profile (
    id                  uuid            PRIMARY KEY,
    champion_key        integer         NOT NULL,
    champion_id         varchar(60)     NOT NULL,
    name                varchar(80)     NOT NULL,
    title               varchar(160),
    blurb               varchar(1200),
    image_url           varchar(400),
    version             varchar(30),
    difficulty          integer         NOT NULL,
    stat_hp             double precision NOT NULL DEFAULT 0,
    stat_mp             double precision NOT NULL DEFAULT 0,
    stat_armor          double precision NOT NULL DEFAULT 0,
    stat_spell_block    double precision NOT NULL DEFAULT 0,
    stat_attack_damage  double precision NOT NULL DEFAULT 0,
    stat_attack_speed   double precision NOT NULL DEFAULT 0,
    stat_move_speed     double precision NOT NULL DEFAULT 0,
    is_active           boolean         NOT NULL DEFAULT TRUE,
    created_at          timestamptz     NOT NULL DEFAULT NOW(),
    updated_at          timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_champion_profile_key
    ON champions.champion_profile (champion_key);
CREATE UNIQUE INDEX IF NOT EXISTS ux_champion_profile_champion_id
    ON champions.champion_profile (champion_id);

CREATE TABLE IF NOT EXISTS champions.champion_role (
    id                    serial       PRIMARY KEY,
    champion_profile_id   uuid         NOT NULL
        REFERENCES champions.champion_profile (id) ON DELETE CASCADE,
    role                  varchar(30)  NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_champion_role_role ON champions.champion_role (role);

CREATE TABLE IF NOT EXISTS champions.champion_ability (
    id                    uuid         PRIMARY KEY,
    champion_profile_id   uuid         NOT NULL
        REFERENCES champions.champion_profile (id) ON DELETE CASCADE,
    slot                  varchar(20)  NOT NULL,
    name                  varchar(120) NOT NULL,
    description           varchar(2000),
    image_url             varchar(400),
    cooldown              integer,
    is_active             boolean      NOT NULL DEFAULT TRUE,
    created_at            timestamptz  NOT NULL DEFAULT NOW(),
    updated_at            timestamptz
);

CREATE INDEX IF NOT EXISTS ix_champion_ability_profile_slot
    ON champions.champion_ability (champion_profile_id, slot);

CREATE TABLE IF NOT EXISTS champions.free_rotation (
    id                    uuid         PRIMARY KEY,
    platform              varchar(10)  NOT NULL,
    period_start          timestamptz  NOT NULL,
    period_end            timestamptz  NOT NULL,
    max_new_player_level  integer      NOT NULL DEFAULT 0,
    hash                  varchar(64)  NOT NULL,
    is_active             boolean      NOT NULL DEFAULT TRUE,
    created_at            timestamptz  NOT NULL DEFAULT NOW(),
    updated_at            timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_free_rotation_platform_hash
    ON champions.free_rotation (platform, hash);

CREATE TABLE IF NOT EXISTS champions.free_rotation_entry (
    id                uuid        PRIMARY KEY,
    free_rotation_id  uuid        NOT NULL
        REFERENCES champions.free_rotation (id) ON DELETE CASCADE,
    champion_key      integer     NOT NULL,
    for_new_players   boolean     NOT NULL DEFAULT FALSE,
    is_active         boolean     NOT NULL DEFAULT TRUE,
    created_at        timestamptz NOT NULL DEFAULT NOW(),
    updated_at        timestamptz
);

CREATE INDEX IF NOT EXISTS ix_free_rotation_entry_key
    ON champions.free_rotation_entry (champion_key);

CREATE TABLE IF NOT EXISTS champions.summoner (
    id            uuid        PRIMARY KEY,
    puuid         varchar(80) NOT NULL,
    game_name     varchar(60),
    tag_line      varchar(20),
    platform      varchar(10) NOT NULL,
    last_sync_at  timestamptz,
    is_active     boolean     NOT NULL DEFAULT TRUE,
    created_at    timestamptz NOT NULL DEFAULT NOW(),
    updated_at    timestamptz
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_summoner_puuid ON champions.summoner (puuid);

CREATE TABLE IF NOT EXISTS champions.champion_mastery (
    id              uuid        PRIMARY KEY,
    summoner_id     uuid        NOT NULL
        REFERENCES champions.summoner (id) ON DELETE CASCADE,
    champion_key    integer     NOT NULL,
    points          bigint      NOT NULL DEFAULT 0,
    level           integer     NOT NULL DEFAULT 0,
    last_play_time  timestamptz,
    chest_granted   boolean     NOT NULL DEFAULT FALSE,
    tokens_earned   integer     NOT NULL DEFAULT 0,
    is_active       boolean     NOT NULL DEFAULT TRUE,
    created_at      timestamptz NOT NULL DEFAULT NOW(),
    updated_at      timestamptz
);

CREATE INDEX IF NOT EXISTS ix_champion_mastery_summoner
    ON champions.champion_mastery (summoner_id);

CREATE UNIQUE INDEX IF NOT EXISTS ux_champion_mastery_summoner_champion
    ON champions.champion_mastery (summoner_id, champion_key);
