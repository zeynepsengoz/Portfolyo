# Portfolyo

A full-stack personal portfolio web app built with **ASP.NET Core MVC (.NET 6)** and **PostgreSQL**.

![Public portfolio preview](./wwwroot/screen1.png)

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
- Upload preview and gallery images from admin panel (Cloudinary or local fallback)

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
- **Image Storage:** Cloudinary (recommended) or local `wwwroot/uploads` fallback
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

Optional image storage keys (recommended for production):
- `CLOUDINARY_CLOUD_NAME`
- `CLOUDINARY_API_KEY`
- `CLOUDINARY_API_SECRET`

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
| `DATA_PROTECTION_KEYS_PATH` | Optional | Persistent key path to avoid auth/antiforgery issues after redeploy |
| `CLOUDINARY_CLOUD_NAME` | Optional** | Cloudinary cloud name for persistent image uploads |
| `CLOUDINARY_API_KEY` | Optional** | Cloudinary API key |
| `CLOUDINARY_API_SECRET` | Optional** | Cloudinary API secret |

`*` Provide either `DATABASE_URL` or `DefaultConnection`.
`**` If all three Cloudinary keys are set, project images are uploaded to Cloudinary.

## Render Deployment

1. Push repository to GitHub.
2. In Render, create a **Blueprint** from this repo (or manual Web Service + Postgres).
3. Set required env vars in Render:
   - `ADMIN_KEY`
   - `JWT_SECRET`
   - `DATABASE_URL` (from your Render PostgreSQL instance)
4. Recommended: add Cloudinary env vars for persistent uploads:
   - `CLOUDINARY_CLOUD_NAME`
   - `CLOUDINARY_API_KEY`
   - `CLOUDINARY_API_SECRET`
5. Deploy.

## Notes

- Without Cloudinary, uploaded files are stored under:
  - `wwwroot/uploads/project-previews`
  - `wwwroot/uploads/project-gallery`
- Local/container disk uploads are not persistent on ephemeral hosts after redeploy/restart.
- With Cloudinary configured, uploaded images are persisted and returned as secure Cloudinary URLs.

## License

Private/personal use unless a license file is explicitly added.
