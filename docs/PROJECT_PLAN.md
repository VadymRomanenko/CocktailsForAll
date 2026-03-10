# Cocktail Hub — Project Plan (Updated)

## Overview

Mobile-first web application for searching and managing cocktail recipes. Tech stack: .NET 8 Web API, React (Vite), PostgreSQL, JWT, TheCocktailDB for initial data.

---

## 1. Architecture

```
CocktailsForAll/
├── src/
│   ├── CocktailHub.Api/
│   ├── CocktailHub.Core/
│   └── CocktailHub.Infrastructure/
├── client/
├── docs/                  # Project documentation
│   └── PROJECT_PLAN.md
├── CocktailHub.sln
└── docker-compose.yml
```

---

## 2. Multi-Language Interface (i18n)

**Languages (starter):** Ukrainian, English, Polish

**Implementation:**
- **Library:** react-i18next + i18next
- **Structure:** `client/src/locales/{uk,en,pl}/translation.json`
- **Features:**
  - Language switcher in header/bottom nav
  - Persist selected language in localStorage
  - Fallback to English if translation missing

**Scope of translation:**
- Navigation labels (Home, Search, Favorites, Login, etc.)
- Form labels and validation messages
- Page titles
- Buttons and common UI text

---

## 3. Countries

**Naming:** Use `ruZZia` instead of `Russia` in the countries list.

**Expanded countries list:** Broader set for better coverage:
- USA, United Kingdom, France, Mexico, Cuba, Italy, Japan, ruZZia, Spain, Ireland
- Brazil, Germany, Poland, Ukraine, Greece, Portugal, Argentina, Colombia
- Jamaica, Thailand, China, India, Australia, Canada, Netherlands, Belgium
- Austria, Sweden, Norway, Denmark, Finland, Switzerland, Czech Republic

---

## 4. Cocktail Seeding (TheCocktailDB)

**Current:** `search.php?f=a` … `f=z` (by first letter)

**Expanded extraction:**
1. **By first letter** — keep existing loop (a–z)
2. **By category** — `filter.php?c={category}` for each category from `list.php?c=list`
3. **By popular ingredient** — `filter.php?i={ingredient}` for top ingredients (Vodka, Gin, Rum, etc.)
4. **Merge and deduplicate** by `idDrink`

**Rate limiting:** 150–200 ms delay between API calls to respect limits.

---

## 5. Database

| Table | Key Fields |
|-------|------------|
| Countries | Id, Name, IsoCode |
| Cocktails | Id, Name, Description, Instructions, ImageUrl, CountryId, IsModerated, CreatedByUserId |
| Ingredients | Id, Name |
| CocktailIngredients | CocktailId, IngredientId, Measure |
| Users | Id, Email, PasswordHash, Role |
| Favorites | UserId, CocktailId |

---

## 6. API Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | /api/cocktails | List (filters, pagination) |
| GET | /api/cocktails/{id} | Details |
| POST | /api/cocktails | Create (auth) |
| GET | /api/countries | Countries for dropdown |
| GET | /api/ingredients?search= | Autocomplete |
| POST | /api/auth/register | Register |
| POST | /api/auth/login | Login |
| POST/DELETE | /api/favorites/{id} | Toggle favorite |
| GET | /api/admin/pending | Pending cocktails |
| PUT | /api/admin/cocktails/{id}/approve | Approve |

---

## 7. Frontend Structure

```
client/src/
├── locales/           # i18n
│   ├── uk/
│   ├── en/
│   └── pl/
├── components/
├── pages/
├── context/
├── api/
└── hooks/
```

---

## 8. Implementation Order (Updates)

1. ~~Solution + EF Core + Auth + Controllers~~
2. **Multi-language** — add react-i18next, translation files, language switcher
3. **Countries** — rename Russia → ruZZia, expand list, migration
4. **Seeding** — add category and ingredient filters, increase cocktail coverage
5. **Save plan** — this document in `docs/PROJECT_PLAN.md`

---

## 9. Technical Notes

- **CORS:** Allow `http://localhost:5173`
- **TheCocktailDB:** Free key `1`; rate limit ~5 req/sec
- **Country seeding:** Migration or data fix for Russia → ruZZia in existing DBs
