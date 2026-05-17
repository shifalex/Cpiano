alter table if exists public."KeyboardQuestion"
    add column if not exists "SubmittedKeyboardJson" text,
    add column if not exists "SubmittedTime" timestamp with time zone,
    add column if not exists "MoveByLength" integer,
    add column if not exists "MoveByDirectionJson" text,
    add column if not exists "KeyboardRows" integer not null default 1,
    add column if not exists "KeyboardKeysInRow" integer not null default 10,
    add column if not exists "AttemptNumber" integer not null default 0,
    add column if not exists "WasTutorialUsed" boolean not null default false,
    add column if not exists "WasHeaderResultToggleUsed" boolean not null default false,
    add column if not exists "KeyDownCount" integer not null default 0,
    add column if not exists "DistinctKeyCount" integer not null default 0,
    add column if not exists "PressClusterCount" integer not null default 0,
    add column if not exists "LargestPressClusterSize" integer not null default 0,
    add column if not exists "MaxInterKeyGapMs" integer not null default 0,
    add column if not exists "AverageInterKeyGapMs" integer not null default 0,
    add column if not exists "FirstKeyToSubmitMs" integer not null default 0,
    add column if not exists "LastKeyToSubmitMs" integer not null default 0,
    add column if not exists "PressPatternKind" integer not null default 0;

alter table if exists public."KeyEvent"
    add column if not exists "AttemptNumber" integer not null default 0;
