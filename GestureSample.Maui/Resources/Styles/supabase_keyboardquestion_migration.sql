alter table if exists public."KeyboardQuestion"
    add column if not exists "SubmittedKeyboardJson" text,
    add column if not exists "SubmittedTime" timestamp with time zone,
    add column if not exists "MoveByLength" integer,
    add column if not exists "MoveByDirectionJson" text,
    add column if not exists "KeyboardRows" integer not null default 1,
    add column if not exists "KeyboardKeysInRow" integer not null default 10,
    add column if not exists "AttemptNumber" integer not null default 0,
    add column if not exists "WasTutorialUsed" boolean not null default false;

alter table if exists public."KeyEvent"
    add column if not exists "AttemptNumber" integer not null default 0;
