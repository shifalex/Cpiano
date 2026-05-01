begin;

grant usage on schema public to authenticated;

grant select, insert, update, delete on public."Users" to authenticated;
grant select, insert, update, delete on public."Games" to authenticated;
grant select, insert, update, delete on public."QuestionsAnswers" to authenticated;
grant select, insert, update, delete on public."KeyboardQuestion" to authenticated;
grant select, insert, update, delete on public."KeyEvent" to authenticated;
grant select, insert, update, delete on public."TimerChangeEvent" to authenticated;
grant select, insert, update, delete on public."VisibilityChangeEvent" to authenticated;
grant select, insert, update, delete on public."QuestionAnswerPart" to authenticated;

grant usage, select on sequence public."KeyboardQuestion_QuestionID_seq" to authenticated;
grant usage, select on sequence public."KeyEvent_id_seq" to authenticated;
grant usage, select on sequence public."TimerChangeEvent_Id_seq" to authenticated;
grant usage, select on sequence public."VisibilityChangeEvent_Id_seq" to authenticated;
grant usage, select on sequence public."QuestionAnswerPart_Id_seq" to authenticated;

alter table public."Users" enable row level security;
alter table public."Games" enable row level security;
alter table public."QuestionsAnswers" enable row level security;
alter table public."KeyboardQuestion" enable row level security;
alter table public."KeyEvent" enable row level security;
alter table public."TimerChangeEvent" enable row level security;
alter table public."VisibilityChangeEvent" enable row level security;
alter table public."QuestionAnswerPart" enable row level security;

drop policy if exists "Users self select" on public."Users";
create policy "Users self select"
    on public."Users"
    for select
    to authenticated
    using ("id" = auth.uid());

drop policy if exists "Users self insert" on public."Users";
create policy "Users self insert"
    on public."Users"
    for insert
    to authenticated
    with check ("id" = auth.uid());

drop policy if exists "Users self update" on public."Users";
create policy "Users self update"
    on public."Users"
    for update
    to authenticated
    using ("id" = auth.uid())
    with check ("id" = auth.uid());

drop policy if exists "Games own select" on public."Games";
create policy "Games own select"
    on public."Games"
    for select
    to authenticated
    using ("userId" = auth.uid());

drop policy if exists "Games own insert" on public."Games";
create policy "Games own insert"
    on public."Games"
    for insert
    to authenticated
    with check ("userId" = auth.uid());

drop policy if exists "Games own update" on public."Games";
create policy "Games own update"
    on public."Games"
    for update
    to authenticated
    using ("userId" = auth.uid())
    with check ("userId" = auth.uid());

drop policy if exists "Games own delete" on public."Games";
create policy "Games own delete"
    on public."Games"
    for delete
    to authenticated
    using ("userId" = auth.uid());

drop policy if exists "QuestionsAnswers own access" on public."QuestionsAnswers";
create policy "QuestionsAnswers own access"
    on public."QuestionsAnswers"
    for all
    to authenticated
    using (
        exists (
            select 1
            from public."Games" g
            where g."id" = public."QuestionsAnswers"."gameId"
              and g."userId" = auth.uid()
        )
    )
    with check (
        exists (
            select 1
            from public."Games" g
            where g."id" = public."QuestionsAnswers"."gameId"
              and g."userId" = auth.uid()
        )
    );

drop policy if exists "KeyboardQuestion own access" on public."KeyboardQuestion";
create policy "KeyboardQuestion own access"
    on public."KeyboardQuestion"
    for all
    to authenticated
    using (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."KeyboardQuestion"."GameId"
              and g."userId" = auth.uid()
        )
    )
    with check (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."KeyboardQuestion"."GameId"
              and g."userId" = auth.uid()
        )
    );

drop policy if exists "KeyEvent own access" on public."KeyEvent";
create policy "KeyEvent own access"
    on public."KeyEvent"
    for all
    to authenticated
    using (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."KeyEvent"."GameId"
              and g."userId" = auth.uid()
        )
    )
    with check (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."KeyEvent"."GameId"
              and g."userId" = auth.uid()
        )
    );

drop policy if exists "TimerChangeEvent own access" on public."TimerChangeEvent";
create policy "TimerChangeEvent own access"
    on public."TimerChangeEvent"
    for all
    to authenticated
    using (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."TimerChangeEvent"."GameId"
              and g."userId" = auth.uid()
        )
    )
    with check (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."TimerChangeEvent"."GameId"
              and g."userId" = auth.uid()
        )
    );

drop policy if exists "VisibilityChangeEvent own access" on public."VisibilityChangeEvent";
create policy "VisibilityChangeEvent own access"
    on public."VisibilityChangeEvent"
    for all
    to authenticated
    using (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."VisibilityChangeEvent"."GameId"
              and g."userId" = auth.uid()
        )
    )
    with check (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."VisibilityChangeEvent"."GameId"
              and g."userId" = auth.uid()
        )
    );

drop policy if exists "QuestionAnswerPart own access" on public."QuestionAnswerPart";
create policy "QuestionAnswerPart own access"
    on public."QuestionAnswerPart"
    for all
    to authenticated
    using (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."QuestionAnswerPart"."GameId"
              and g."userId" = auth.uid()
        )
    )
    with check (
        exists (
            select 1
            from public."Games" g
            where g."id"::text = public."QuestionAnswerPart"."GameId"
              and g."userId" = auth.uid()
        )
    );

commit;
