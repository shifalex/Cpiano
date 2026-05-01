**Safe Connection**

For a client app, connect to Supabase with:
- project URL
- anon key
- a real user session from Supabase Auth

Do not put these in the app:
- `service_role` key
- database password
- admin email/password

This project now supports local-only config through:
- [supabase.local.json](/C:/Users/alexs/source/repos/shifalex/Cpiano/Cpiano/GestureSample.Maui/supabase.local.json)
- or env vars:
  - `GESTURE_SAMPLE_SUPABASE_URL`
  - `GESTURE_SAMPLE_SUPABASE_ANON_KEY`

Example local config:

```json
{
  "Url": "https://your-project.supabase.co",
  "AnonKey": "your-anon-key"
}
```

**How The App Should Connect Properly**

1. The mobile app starts with the project URL + anon key.
2. The user signs in with Supabase Auth.
3. The app gets a JWT session.
4. All table access goes through RLS policies using `auth.uid()`.

That is the secure normal Supabase pattern.

**Important Current Caveat**

The current app code can load the URL/key safely now, but strict RLS should only be enabled after the app signs in real Supabase Auth users whose `auth.uid()` matches the app user/game ownership.

So use the SQL files in this order:

1. run [supabase_app_schema.sql](/C:/Users/alexs/source/repos/shifalex/Cpiano/Cpiano/GestureSample.Maui/Scripts/supabase_app_schema.sql)
2. restore the sanitized legacy data if needed
3. verify app reads/writes
4. then run [supabase_app_rls.sql](/C:/Users/alexs/source/repos/shifalex/Cpiano/Cpiano/GestureSample.Maui/Scripts/supabase_app_rls.sql) only after Auth integration is ready

**What To Use Outside The App**

Use these only in admin workflows, never in the mobile client:
- SQL editor
- `psql`
- migrations
- Edge Functions with server-side secrets
- `service_role`

**Recommended Long-Term Architecture**

- app: anon key + Supabase Auth session
- tables: RLS locked by `auth.uid()`
- privileged operations: Edge Functions or server-side scripts
