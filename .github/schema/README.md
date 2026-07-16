# SQL Schema Inventory

Last updated: 2026-07-16

This folder is the project-facing inventory for database schema and seed data. The canonical full schema currently lives in `.github/schema.sql`; `.github/schema/schema.sql` is a SQLCMD wrapper that points to that source file so the folder stays usable without duplicating the full script.

## Canonical Files

| File | Purpose |
| --- | --- |
| `.github/schema.sql` | Full SQL Server schema: tables, constraints, indexes, and default `SystemSettings` seeds. |
| `.github/schema/schema.sql` | SQLCMD entrypoint that includes `.github/schema.sql`. |
| `.github/schema/patches.sql` | SQLCMD entrypoint for incremental schema patches. |
| `.github/schema/seeds/*.sql` | SQLCMD entrypoints for seed scripts. |
| `scripts/alter-submissions-rejectreason.sql` | Incremental patch that adds `Submissions.RejectReason`. |
| `scripts/seed-admin-dispute-permissions.sql` | Adds dispute permissions, an admin dispute role, and assigns it to `admin@ttm.com.tw`. |
| `scripts/seed-merchants.sql` | Adds 5 merchant owner users, 5 merchants, and merchant wallets. |
| `scripts/seed-kol-registration-states.sql` | Adds KOL accounts covering registration and review states. |
| `scripts/seed-cases.sql` | Adds one case for each `Cases.Status` value with related platform/category/language/requirement/budget rows. |
| `scripts/seed-demo-15-each.sql` | Adds the broad `[SEED15]` demo dataset for admin feature testing. |
| `scripts/create-admin-seed.ps1` | Generates `scripts/seed-admin.sql` for a real admin login seed. Generated output should not be committed with a plain-text password. |

## Schema Tables

The full schema defines these 50 tables:

| Area | Tables |
| --- | --- |
| Identity and access | `Users`, `Roles`, `Permissions`, `RolePermissions`, `UserRoles`, `AdminProfiles`, `UserInvitations` |
| Merchant | `Merchants`, `MerchantMembers`, `MerchantInvitations`, `MerchantContacts` |
| KOL profile and review | `KolProfiles`, `KolReviewEvents`, `KolCategories`, `KolServiceAreas`, `KolLanguages`, `KolSocialAccounts`, `KolBankAccounts` |
| KOL tax and payout | `KolTaxProfiles`, `KolTaxDocuments`, `KolWallets`, `KolEarnings`, `PayoutRequests`, `PayoutRequestDocuments`, `KolSettlementStatements`, `KolSettlementItems` |
| Cases and recruitment | `Cases`, `CaseBudgetSnapshots`, `CasePlatforms`, `CaseCategories`, `CaseLanguages`, `CaseRequirements`, `CaseBarterItems`, `CaseAttachments`, `CaseApplications` |
| Task delivery | `Tasks`, `Submissions`, `SubmissionItems` |
| Review and disputes | `ReviewCriteria`, `Reviews`, `ReviewScores`, `Disputes`, `DisputeMessages`, `DisputeAttachments` |
| Operations | `Files`, `ActivityLogs`, `Notifications`, `NotificationPreferences`, `SystemSettings`, `SystemSettingLogs` |
| Merchant money | `MerchantWallets`, `MerchantWalletTransactions`, `MerchantCreditWallets`, `MerchantCreditTransactions` |

The schema also creates indexes for common lookup paths such as merchant/user links, KOL profile lookups, case status, task/KOL lookups, submission deadlines, payout settlement, activity logs, notifications, merchant wallet transactions, and disputes.

## Seed Data Summary

### Admin Account

`scripts/create-admin-seed.ps1` generates `scripts/seed-admin.sql`.

Default parameters:

| Field | Value |
| --- | --- |
| Email | `admin@ttm.com.tw` |
| Name | system administrator |
| Password | `Admin@12345` |

The generated SQL stores a BCrypt hash, but the script prints the plain-text password into the generated file comments. Treat `seed-admin.sql` as local-only unless the password comment is removed.

### Admin Permissions

`scripts/seed-admin-dispute-permissions.sql` assumes `admin@ttm.com.tw` already exists. It creates dispute-related permissions, creates or reuses an admin role, grants the permissions, and binds the role to the admin account.

### Merchant Demo Accounts

`scripts/seed-merchants.sql` inserts 5 merchant owner users. These rows use `PasswordHash = NULL`, so they are demo identities for admin/merchant management flows, not direct login credentials.

| Email | Notes |
| --- | --- |
| `owner@mountainsea.com.tw` | Merchant owner demo account. |
| `owner@taipeihotel.com.tw` | Merchant owner demo account. |
| `owner@hualien-bb.com.tw` | Merchant owner demo account. |
| `owner@tainan-old.com.tw` | Merchant owner demo account. |
| `owner@kenting-resort.com.tw` | Merchant owner demo account. |

Related rows:

| Table | Count / Notes |
| --- | --- |
| `Users` | 5 merchant owners, `AccountType = 2` |
| `Merchants` | 5 merchant profiles |
| `MerchantWallets` | 5 wallet rows |

### KOL Registration State Accounts

`scripts/seed-kol-registration-states.sql` is idempotent by email and creates one fake KOL for each review/registration scenario.

| Email | Scenario |
| --- | --- |
| `fresh-kol-01@example.com` | User exists only; profile not completed. |
| `draft-kol-01@example.com` | Basic profile draft; no review event yet. |
| `pending-kol-01@example.com` | Pending review. |
| `social-check-kol-01@example.com` | Social account review scenario. |
| `bank-pending-kol-01@example.com` | Bank account pending scenario. |
| `returned-kol-01@example.com` | Returned/rejected profile with note. |
| `resubmit-kol-01@example.com` | Returned then resubmitted. |
| `approved-kol-01@example.com` | Approved profile with verified timestamp/admin. |

Related rows include `KolProfiles`, `KolCategories`, `KolServiceAreas`, `KolLanguages`, `KolSocialAccounts`, selected `KolBankAccounts`, and `KolReviewEvents`.

### Case Status Seeds

`scripts/seed-cases.sql` creates one case for every `Cases.Status` value and adds related child rows:

`CasePlatforms`, `CaseCategories`, `CaseLanguages`, `CaseRequirements`, `CaseBarterItems`, and `CaseBudgetSnapshots`.

The script uses merchant plus title as the seed key, so it is designed to be safely rerun.

### Broad Demo Dataset

`scripts/seed-demo-15-each.sql` creates the `[SEED15]` dataset for list/detail/dashboard/finance/dispute testing.

Expected rows:

| Area | Pattern / Count |
| --- | --- |
| Admin user | `seed15-admin@example.com`, `PasswordHash = NULL` |
| Merchant users | `seed15-merchant-01@example.com` through `seed15-merchant-15@example.com` |
| KOL users | `seed15-kol-01@example.com` through `seed15-kol-15@example.com` |
| Merchants | 15 `[SEED15]` merchant profiles |
| KOL profiles | 15 `[SEED15]` KOL profiles |
| Cases | 15 `[SEED15]` case records |
| Applications | Up to 15 related applications |
| Tasks | Up to 15 related tasks |
| Submissions and items | Created for progressed task statuses |
| Money/dispute/activity | `KolEarnings`, merchant wallet/credit transactions, `Disputes`, and `ActivityLogs` |

All generated login-like users in this script use `PasswordHash = NULL`; they are dataset identities, not usable passwords.

## Apply Order

Recommended order for a fresh local database:

1. Apply `.github/schema/schema.sql` in SQLCMD mode.
2. Apply `.github/schema/patches.sql` only if the root schema has not already included those patches.
3. Generate and apply `scripts/seed-admin.sql` with `scripts/create-admin-seed.ps1` if a local admin login is needed.
4. Apply `.github/schema/seeds/01-admin-dispute-permissions.sql`.
5. Apply `.github/schema/seeds/02-merchants.sql`.
6. Apply `.github/schema/seeds/03-kol-registration-states.sql`.
7. Apply `.github/schema/seeds/04-cases.sql`.
8. Apply `.github/schema/seeds/05-demo-15-each.sql` when broad demo coverage is wanted.

## Notes

- `AccountType`: `1 = Admin`, `2 = Merchant`, `3 = KOL`.
- Demo users with `PasswordHash = NULL` are not valid login accounts unless the application allows passwordless/testing flows.
- Keep schema changes mirrored here so future tasks can start from this folder instead of hunting through scripts.
