-- Additive replay data; existing recordings remain readable.
ALTER TABLE public."KeyEvent"
    ADD COLUMN IF NOT EXISTS "ContactId" text,
    ADD COLUMN IF NOT EXISTS "FromKeyNumber" integer,
    ADD COLUMN IF NOT EXISTS "EventSequence" bigint NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS "ElapsedMs" double precision;

ALTER TABLE public."KeyboardQuestion"
    ADD COLUMN IF NOT EXISTS "QuestionArrowsJson" text,
    ADD COLUMN IF NOT EXISTS "TargetKeyboardJson" text,
    ADD COLUMN IF NOT EXISTS "QuestionKeyboardColorsJson" text,
    ADD COLUMN IF NOT EXISTS "QuestionKeyboardColorsJson2" text,
    ADD COLUMN IF NOT EXISTS "SubmittedKeyboardColorsJson" text,
    ADD COLUMN IF NOT EXISTS "InitialKeyboardColorsJson" text;

NOTIFY pgrst, 'reload schema';
