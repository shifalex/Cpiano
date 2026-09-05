create or replace function pg_temp.fix_identity_sequence(
    target_table regclass,
    candidate_columns text[]
)
returns void
language plpgsql
as $$
declare
    v_table_name text := target_table::text;
    v_schema_name text;
    v_rel_name text;
    v_column_name text;
    v_sequence_name text;
    max_value bigint;
begin
    select n.nspname, c.relname
    into v_schema_name, v_rel_name
    from pg_class c
    join pg_namespace n on n.oid = c.relnamespace
    where c.oid = target_table;

    select candidate.column_name
    into v_column_name
    from unnest(candidate_columns) as candidate(column_name)
    where exists
    (
        select 1
        from information_schema.columns c
        where c.table_schema = v_schema_name
          and c.table_name = v_rel_name
          and c.column_name = candidate.column_name
    )
    limit 1;

    if v_column_name is null then
        raise notice 'Skipping %, no matching identity column found from %.',
            v_table_name,
            array_to_string(candidate_columns, ', ');
        return;
    end if;

    select pg_get_serial_sequence(v_table_name, v_column_name)
    into v_sequence_name;

    if v_sequence_name is null then
        raise notice 'Skipping %.%, no serial/identity sequence found.',
            v_table_name,
            v_column_name;
        return;
    end if;

    execute format(
        'select coalesce(max(%1$I), 0) from %2$s',
        v_column_name,
        v_table_name
    )
    into max_value;

    execute format(
        'select setval(%L, %s, false)',
        v_sequence_name,
        max_value + 1
    );

    raise notice 'Sequence repaired for %.% using % (next value %).',
        v_table_name,
        v_column_name,
        v_sequence_name,
        max_value + 1;
end;
$$;

select pg_temp.fix_identity_sequence('public."QuestionsAnswers"', array['id']);
select pg_temp.fix_identity_sequence('public."KeyboardQuestion"', array['QuestionID', 'id', 'questionId']);
select pg_temp.fix_identity_sequence('public."KeyEvent"', array['id', 'Id']);
select pg_temp.fix_identity_sequence('public."TimerChangeEvent"', array['Id', 'id']);
select pg_temp.fix_identity_sequence('public."VisibilityChangeEvent"', array['Id', 'id']);
select pg_temp.fix_identity_sequence('public."QuestionAnswerPart"', array['Id', 'id']);
