Use this as the safe connection path for the MAUI app.

## 1. What goes in the client app

Only these two values belong in the client:

- project URL
- project anon key

Put them in:

- [supabase.local.json](C:/Users/alexs/source/repos/shifalex/Cpiano/Cpiano/GestureSample.Maui/supabase.local.json)

Example:

```json
{
  "Url": "https://your-project-ref.supabase.co",
  "AnonKey": "your-anon-key"
}
```

You can also use environment variables instead:

- `GESTURE_SAMPLE_SUPABASE_URL`
- `GESTURE_SAMPLE_SUPABASE_ANON_KEY`

The app now rejects a `service_role` key in client config on purpose.

## 2. What must never go in the client app

Never place these in the MAUI app:

- database password
- service role key
- shared admin email/password

Those are backend or admin-only secrets.

## 3. Restore old data first

You already did the right thing:

- restored only the app-owned public tables from the old dump
- did not restore Supabase-managed roles, `auth`, `storage`, `realtime`, grants, or event triggers

After the legacy restore, also run:

- [supabase_repair_app_sequences.sql](C:/Users/alexs/source/repos/shifalex/Cpiano/Cpiano/GestureSample.Maui/Scripts/supabase_repair_app_sequences.sql)

That advances the server-side identity counters to the current max IDs in the restored tables, so
future sync inserts do not collide with old restored rows.

## 4. Create the current app tables

Run:

- [supabase_app_schema.sql](C:/Users/alexs/source/repos/shifalex/Cpiano/Cpiano/GestureSample.Maui/Scripts/supabase_app_schema.sql)

This script only creates or updates app-owned tables:

- `Games`
- `QuestionsAnswers`
- `KeyboardQuestion`
- `KeyEvent`
- `TimerChangeEvent`
- `VisibilityChangeEvent`
- `QuestionAnswerPart`

It also adds the newer secondary-PPW columns to `QuestionsAnswers`.

## 5. Proper secure model

The correct long-term model is:

1. The app starts with URL + anon key only.
2. A real user signs in with Supabase Auth.
3. The client then uses that user's session JWT automatically.
4. Row Level Security decides which rows that user may access.

That means:

- app-owned data access should be tied to `auth.uid()`
- the MAUI app should not act like an admin

## 6. Apply strict RLS only when auth flow is really in use

When you are ready to require real Supabase sign-in for syncing, run:

- [supabase_app_rls.sql](C:/Users/alexs/source/repos/shifalex/Cpiano/Cpiano/GestureSample.Maui/Scripts/supabase_app_rls.sql)

That script:

- grants access to `authenticated`
- enables RLS
- limits rows to the signed-in user's own records

Important:

- do not apply the strict RLS script if the app is still syncing without a real authenticated Supabase user session
- otherwise sync/read calls will start failing, which is expected

## 7. Recommended order

1. Fill `supabase.local.json` with the new project URL + anon key.
2. Run `legacy-public-restore.sql` if you have not already.
3. Run `supabase_repair_app_sequences.sql`.
4. Run `supabase_app_schema.sql`.
5. Test the app connection and data loading.
6. Verify the app is signing real users in before syncing.
7. Only then run `supabase_app_rls.sql`.

## 8. Admin work belongs outside the app

Use the Supabase dashboard, SQL editor, `psql`, or a backend/admin script for:

- restores
- schema migrations
- RLS changes
- service-role tasks

Do not move those responsibilities into the MAUI client.
