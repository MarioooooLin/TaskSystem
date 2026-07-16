# Seed Entrypoints

These files are SQLCMD wrappers that point to the current source seed scripts under `scripts/`.

Use SQLCMD mode when executing `:r` files. Keeping wrappers here avoids duplicating large SQL scripts while still making `.github/schema` the single place to discover current schema and demo data.

Recommended order:

1. Generate and apply `scripts/seed-admin.sql` if a real admin login is needed.
2. `01-admin-dispute-permissions.sql`
3. `02-merchants.sql`
4. `03-kol-registration-states.sql`
5. `04-cases.sql`
6. `05-demo-15-each.sql`
