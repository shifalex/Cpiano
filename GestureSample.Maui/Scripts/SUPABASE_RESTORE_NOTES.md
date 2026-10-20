Use this flow for old cluster backups with a new Supabase project.

1. Generate a sanitized SQL file that keeps only app tables:

```powershell
pwsh -File .\Scripts\Export-LegacySupabasePublicData.ps1 `
  -DumpPath "C:\Users\alexs\Downloads\db_cluster-22-08-2025@02-30-35.backup.gz" `
  -OutputPath ".\Scripts\legacy-public-restore.sql"
```

2. Review the output and confirm it only contains:
- `public."Games"`
- `public."QuestionsAnswers"`
- `public."Users"`

3. Restore that sanitized SQL into the new Supabase database:

```powershell
psql "postgresql://postgres.<project-ref>:<password>@aws-0-<region>.pooler.supabase.com:6543/postgres" `
  -v ON_ERROR_STOP=1 `
  -f ".\Scripts\legacy-public-restore.sql"
```

4. Repair the app identity sequences after the legacy import:

```powershell
psql "postgresql://postgres.<project-ref>:<password>@aws-0-<region>.pooler.supabase.com:6543/postgres" `
  -v ON_ERROR_STOP=1 `
  -f ".\Scripts\supabase_repair_app_sequences.sql"
```

This is important for `QuestionsAnswers`, because the old rows keep their original numeric `id`
values and the new Supabase identity sequence may otherwise still try to generate an already-used
ID.

5. Do not restore the raw cluster dump directly into Supabase.
The raw dump contains managed Supabase roles, schemas, auth/storage/realtime objects, grants, triggers, and extensions that conflict with a fresh hosted project.
