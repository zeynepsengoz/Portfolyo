# Portfolyo

A full-stack personal portfolio web app built with **ASP.NET Core MVC (.NET 6)** and **PostgreSQL**.

![Public portfolio preview](./wwwroot/screen.png)

## Features

### Public Website
- Home, About, Skills, Portfolio, Contact sections
- Category filter and pagination on portfolio cards
- Project detail pages with image gallery and lightbox
- External project links (GitHub / itch.io)

### Admin Panel
- Secure admin login (JWT + HttpOnly cookie)
- Admin dashboard with project/message stats
- Manage About, Skills, Education, Projects, Messages
- Project ordering support for homepage display
- Upload preview and gallery images from admin panel

### Deployment
- PostgreSQL-first architecture
- Render-ready (`render.yaml` + `Dockerfile`)
- Works with `postgres://` and `postgresql://` URL formats

## Tech Stack

- **Backend:** ASP.NET Core MVC (.NET 6)
- **ORM:** Entity Framework Core 6
- **Database:** PostgreSQL (Npgsql)
- **Auth:** JWT Bearer + HttpOnly cookie
- **Hashing:** BCrypt
- **Frontend:** Razor Views + custom CSS/JS
- **Hosting:** Docker + Render

## Project Structure

```text
Areas/Admin/                  Admin auth and dashboard
Controllers/                  MVC controllers
Data/                         Entities and DbContexts
Services/                     JWT and app services
ViewComponents/               Home/About/Skills components
Views/                        Razor views
wwwroot/                      Static assets and uploads
Program.cs                    App startup and env wiring
render.yaml                   Render blueprint
Dockerfile                    Container build/runtime
```

## Local Development (PostgreSQL)

### 1) Prerequisites
- .NET SDK 6+
- PostgreSQL 14+ (local or cloud)

### 2) Environment setup
Create `.env` from `.env.example` and fill values:

```powershell
Copy-Item .env.example .env
```

Minimum required keys:
- `ADMIN_KEY`
- `JWT_ISSUER`
- `JWT_AUDIENCE`
- `JWT_SECRET`
- `JWT_EXPIRES_MINUTES`
- `DATABASE_URL` or `DefaultConnection`

Example PostgreSQL URL:

```text
postgresql://USER:PASSWORD@HOST:5432/DB_NAME
```

### 3) Run

```bash
dotnet restore
dotnet run
```

Default local URLs:
- Public site: `https://localhost:7254`
- Admin login: `/admin/auth/login`

## Environment Variables

| Variable | Required | Description |
|---|---|---|
| `ADMIN_KEY` | Yes | Secret key for first admin setup and protected reset actions |
| `JWT_ISSUER` | Yes | JWT issuer |
| `JWT_AUDIENCE` | Yes | JWT audience |
| `JWT_SECRET` | Yes | JWT signing secret |
| `JWT_EXPIRES_MINUTES` | Yes | Token lifetime (minutes) |
| `DATABASE_URL` | Yes* | PostgreSQL connection URL (Render format) |
| `DefaultConnection` | Yes* | Alternative PostgreSQL connection string |
| `PORT` | Optional | Hosting platform port override |
| `SMTP_HOST` | Optional | SMTP host (if email sending is enabled) |
| `SMTP_PORT` | Optional | SMTP port |
| `SMTP_USERNAME` | Optional | SMTP username |
| `SMTP_PASSWORD` | Optional | SMTP password |
| `SMTP_FROM_EMAIL` | Optional | Sender address |
| `SMTP_FROM_NAME` | Optional | Sender display name |
| `SMTP_ENABLE_SSL` | Optional | SSL/TLS on SMTP (`true/false`) |
| `DATA_PROTECTION_KEYS_PATH` | Optional | Persistent key path to avoid auth/antiforgery issues after redeploy |

`*` Provide either `DATABASE_URL` or `DefaultConnection`.

## Render Deployment

1. Push repository to GitHub.
2. In Render, create a **Blueprint** from this repo (or manual Web Service + Postgres).
3. Set required env vars in Render:
   - `ADMIN_KEY`
   - `JWT_SECRET`
   - `DATABASE_URL` (from your Render PostgreSQL instance)
4. Deploy.

## Notes

- Uploaded files are stored under:
  - `wwwroot/uploads/project-previews`
  - `wwwroot/uploads/project-gallery`
- On ephemeral disks, uploaded files are not persistent unless you use persistent storage.

## License

Private/personal use unless a license file is explicitly added.
