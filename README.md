# Cocktail Hub

Mobile-first web app for searching and managing cocktail recipes.

## Tech Stack

- **Backend:** .NET 8 Web API, PostgreSQL, JWT Auth, FluentValidation
- **Frontend:** React (Vite), Tailwind CSS, Lucide Icons, React Router, Zod
- **External API:** TheCocktailDB for initial data seeding

## Setup

### 1. PostgreSQL

Run PostgreSQL (e.g. via Docker):

```bash
docker-compose up -d
```

Or use a local PostgreSQL instance and update `appsettings.json` connection string.

### 2. Backend

```bash
cd src/CocktailHub.Api
dotnet run
```

API runs at `http://localhost:5146` (see `Properties/launchSettings.json`).

### 3. Frontend

```bash
cd client
npm install
npm run dev
```

Frontend runs at `http://localhost:5173` and proxies `/api` to the backend.

## Default Admin

- Email: `admin@cocktailhub.com`
- Password: `Admin123!`

## Features

- Browse cocktails with filters (country, ingredients)
- Hybrid ingredient search (autocomplete + free text)
- User registration and JWT login
- Favorites (requires login)
- Create new cocktails (moderation required)
- Admin moderation of pending cocktails
