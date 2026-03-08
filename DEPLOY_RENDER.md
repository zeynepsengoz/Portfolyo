# Render Deployment + PostgreSQL Migration

## 1) Render deploy
- Repository root includes `render.yaml`.
- Create Blueprint on Render and deploy.
- Set secret env vars in Render:
  - `ADMIN_KEY`
  - `JWT_SECRET`

## 2) Local SQL Server -> PostgreSQL one-time data move
Before first run (or one restart), set these env vars on the web service:
- `MIGRATE_LOCAL_TO_POSTGRES=true`
- `LEGACY_SQLSERVER_CONNECTION=Server=...;Database=...;Trusted_Connection=True;TrustServerCertificate=True`

Notes:
- App copies data only into empty target tables (safe for reruns).
- After migration, set `MIGRATE_LOCAL_TO_POSTGRES=false` and redeploy.

## 3) Connection string behavior
Priority order:
1. `DATABASE_URL`
2. `DefaultConnection` env var
3. `ConnectionStrings:DefaultConnection` from `appsettings.json`

`postgres://` and `postgresql://` URL formats are normalized automatically.
