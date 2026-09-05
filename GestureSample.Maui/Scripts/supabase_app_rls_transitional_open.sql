begin;

-- Transitional RLS setup for the current MAUI app architecture.
-- Use this when the app still syncs with anon access and local/offline-created
-- user IDs, but you want Supabase security lints to pass.
--
-- This is intentionally not the final secure model.
-- It keeps access effectively open to anon/authenticated while RLS is enabled.
-- When the app moves to real Supabase Auth ownership, replace this with
-- supabase_app_rls.sql.

grant usage on schema public to anon;
grant usage on schema public to authenticated;

grant select, insert, update, delete on public."Users" to anon, authenticated;
grant select, insert, update, delete on public."Games" to anon, authenticated;
grant select, insert, update, delete on public."QuestionsAnswers" to anon, authenticated;
grant select, insert, update, delete on public."KeyboardQuestion" to anon, authenticated;
grant select, insert, update, delete on public."KeyEvent" to anon, authenticated;
grant select, insert, update, delete on public."TimerChangeEvent" to anon, authenticated;
grant select, insert, update, delete on public."VisibilityChangeEvent" to anon, authenticated;
grant select, insert, update, delete on public."QuestionAnswerPart" to anon, authenticated;

grant usage, select on sequence public."QuestionsAnswers_id_seq" to anon, authenticated;
grant usage, select on sequence public."KeyboardQuestion_QuestionID_seq" to anon, authenticated;
grant usage, select on sequence public."KeyEvent_id_seq" to anon, authenticated;
grant usage, select on sequence public."TimerChangeEvent_Id_seq" to anon, authenticated;
grant usage, select on sequence public."VisibilityChangeEvent_Id_seq" to anon, authenticated;
grant usage, select on sequence public."QuestionAnswerPart_Id_seq" to anon, authenticated;

alter table public."Users" enable row level security;
alter table public."Games" enable row level security;
alter table public."QuestionsAnswers" enable row level security;
alter table public."KeyboardQuestion" enable row level security;
alter table public."KeyEvent" enable row level security;
alter table public."TimerChangeEvent" enable row level security;
alter table public."VisibilityChangeEvent" enable row level security;
alter table public."QuestionAnswerPart" enable row level security;

drop policy if exists "Users transitional open access" on public."Users";
create policy "Users transitional open access"
    on public."Users"
    for all
    to anon, authenticated
    using (true)
    with check (true);

drop policy if exists "Games transitional open access" on public."Games";
create policy "Games transitional open access"
    on public."Games"
    for all
    to anon, authenticated
    using (true)
    with check (true);

drop policy if exists "QuestionsAnswers transitional open access" on public."QuestionsAnswers";
create policy "QuestionsAnswers transitional open access"
    on public."QuestionsAnswers"
    for all
    to anon, authenticated
    using (true)
    with check (true);

drop policy if exists "KeyboardQuestion transitional open access" on public."KeyboardQuestion";
create policy "KeyboardQuestion transitional open access"
    on public."KeyboardQuestion"
    for all
    to anon, authenticated
    using (true)
    with check (true);

drop policy if exists "KeyEvent transitional open access" on public."KeyEvent";
create policy "KeyEvent transitional open access"
    on public."KeyEvent"
    for all
    to anon, authenticated
    using (true)
    with check (true);

drop policy if exists "TimerChangeEvent transitional open access" on public."TimerChangeEvent";
create policy "TimerChangeEvent transitional open access"
    on public."TimerChangeEvent"
    for all
    to anon, authenticated
    using (true)
    with check (true);

drop policy if exists "VisibilityChangeEvent transitional open access" on public."VisibilityChangeEvent";
create policy "VisibilityChangeEvent transitional open access"
    on public."VisibilityChangeEvent"
    for all
    to anon, authenticated
    using (true)
    with check (true);

drop policy if exists "QuestionAnswerPart transitional open access" on public."QuestionAnswerPart";
create policy "QuestionAnswerPart transitional open access"
    on public."QuestionAnswerPart"
    for all
    to anon, authenticated
    using (true)
    with check (true);

commit;
