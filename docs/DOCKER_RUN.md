# Запуск Cocktail Hub за допомогою Docker

## Передумови

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) встановлено і запущено
- .NET 8 SDK (для backend)
- Node.js та npm (для frontend)

## 1. PostgreSQL в Docker

```bash
cd d:\Work\CocktailsForAll
docker-compose up -d
```

Перевірка статусу контейнера:

```bash
docker-compose ps
```

Повинен бути `Up (healthy)`. БД слухає порт **5433** на localhost.

## 2. Backend (API)

```bash
cd d:\Work\CocktailsForAll\src\CocktailHub.Api
dotnet run
```

API доступне на `http://localhost:5146`. При старті застосовуються міграції та seeding.

## 3. Frontend

```bash
cd d:\Work\CocktailsForAll\client
npm install
npm run dev
```

Frontend доступний на `http://localhost:5173`.

## 4. Відкрити застосунок

Відкрийте в браузері: **http://localhost:5173**

### Облікові дані адміна

- **Email:** admin@cocktailhub.com
- **Пароль:** Admin123!

## Зупинка

```bash
cd d:\Work\CocktailsForAll
docker-compose down
```

Повне видалення БД (volume):

```bash
docker-compose down -v
```
