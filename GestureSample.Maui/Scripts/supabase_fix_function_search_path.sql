do $$
declare
    function_record record;
begin
    for function_record in
        select
            n.nspname as schema_name,
            p.proname as function_name,
            pg_get_function_identity_arguments(p.oid) as identity_args
        from pg_proc p
        join pg_namespace n on n.oid = p.pronamespace
        where n.nspname = 'public'
          and p.proname = 'get_users_by_classroom'
    loop
        execute format(
            'alter function %I.%I(%s) set search_path = public, pg_temp',
            function_record.schema_name,
            function_record.function_name,
            function_record.identity_args
        );

        raise notice 'Updated search_path for %.%(%)',
            function_record.schema_name,
            function_record.function_name,
            function_record.identity_args;
    end loop;

    if not found then
        raise notice 'No public.get_users_by_classroom function found.';
    end if;
end
$$;
