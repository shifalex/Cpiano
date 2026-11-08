begin;

-- Transitional policy set for the current app architecture.
-- Use this only while the app syncs with local/offline-created user IDs and
-- does not authenticate to Supabase with a matching auth user/session.
--
-- This intentionally relaxes protection on app-owned tables so the current
-- MAUI client can sync with the anon key. The long-term secure model should
-- move to adult-owned Supabase Auth accounts and row ownership tied to that
-- account, not to local device-created profile GUIDs.

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

alter table public."Users" disable row level security;
alter table public."Games" disable row level security;
alter table public."QuestionsAnswers" disable row level security;
alter table public."KeyboardQuestion" disable row level security;
alter table public."KeyEvent" disable row level security;
alter table public."TimerChangeEvent" disable row level security;
alter table public."VisibilityChangeEvent" disable row level security;
alter table public."QuestionAnswerPart" disable row level security;

commit;
