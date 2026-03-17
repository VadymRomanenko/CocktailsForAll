# Початкове налаштування Cocktail Hub

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

## 2. Seed дані (коктейлі та інгредієнти)

При першому старті API автоматично заповнює БД. Є два варіанти:

### Варіант A: Статичний JSON (рекомендовано)

Згенеруйте файл даних з TheCocktailDB — не потрібні виклики API при кожному старті:

```bash
cd d:\Work\CocktailsForAll
dotnet run --project tools/SeedDataGenerator/SeedDataGenerator.csproj
```

Або з обмеженням кількості: `-- 100` (за замовчуванням 500).

Створюється `src/CocktailHub.Infrastructure/SeedData/cocktails.json`. При старті API імпортує з цього файлу.

### Варіант B: Живий API

Якщо `cocktails.json` відсутній або порожній — API при старті сам завантажує коктейлі з TheCocktailDB. Перший запуск триватиме довше (~2–3 хв на 500 коктейлів).

## 3. Backend (API)

```bash
cd d:\Work\CocktailsForAll\src\CocktailHub.Api
dotnet run
```

API доступне на `http://localhost:5146`. При старті застосовуються міграції та seeding.

## 4. Frontend

```bash
cd d:\Work\CocktailsForAll\client
npm install
npm run dev
```

Frontend доступний на `http://localhost:5173`.

## 5. Відкрити застосунок

Відкрийте в браузері: **http://localhost:5173**

### Облікові дані адміна

- **Email:** admin@cocktailhub.com
- **Пароль:** Admin123!

---

## Зупинка

```bash
cd d:\Work\CocktailsForAll
docker-compose down
```

Повне видалення БД (volume):

```bash
docker-compose down -v
```

### Після консолідації міграцій

Якщо було виконано об'єднання міграцій у одну, потрібно скинути БД (старі записи в `__EFMigrationsHistory` більше не валідні):

```bash
docker-compose down -v
docker-compose up -d
```

Потім запустіть API — міграція `ConsolidatedSchema` застосується автоматично при старті.
