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
├── docs/
│   ├── PROJECT_PLAN.md
│   └── DOCKER_RUN.md
├── CocktailHub.sln
└── docker-compose.yml
```

---

## 2. Multi-Language (i18n)

**Languages:** Ukrainian, English, Polish

- **UI:** react-i18next, `client/src/locales/{uk,en,pl}/translation.json`
- **Recipes:** CocktailTranslation, IngredientTranslation tables; API returns content by `?lang=uk|en|pl`
- **Optional:** MyMemory API for seeding uk/pl translations (`CocktailHub:SeedTranslations: true` in config)

---

## 3. Recipe Translations

- **CocktailTranslation:** CocktailId, LangCode, Name, Description, Instructions
- **IngredientTranslation:** IngredientId, LangCode, Name
- EN seeded from TheCocktailDB; uk/pl via MyMemory during seed (optional)
- API: `GET /api/cocktails?lang=uk` and `GET /api/cocktails/{id}?lang=uk`

---

## 4. Cocktail Seeding (TheCocktailDB)

**Limit:** Top 500 cocktails

**Sources:**
1. `search.php?f=a` … `f=z` (by first letter)
2. `filter.php?c={category}` for categories from `list.php?c=list`
3. `filter.php?i={ingredient}` for popular ingredients (Vodka, Gin, Rum, etc.)
4. Merge and deduplicate by `idDrink`

**Ingredients:** Full list from `list.php?i=list` plus new ones from each cocktail.

**Rate limiting:** 150–200 ms between API calls.

---

## 5. Search

**By cocktail name:** `?name=...` (searches Name and translations)

**By ingredients:** `?ingredientIds=...` and `?freeTextTags=...`

**Match mode:**
- `matchAllIngredients=false` (default): cocktail has ANY of the ingredients
- `matchAllIngredients=true`: cocktail has ALL selected ingredients

**Language:** `?lang=uk|en|pl` for localized content.

---

## 6. Database

| Table | Key Fields |
|-------|------------|
| Countries | Id, Name, IsoCode |
| Cocktails | Id, Name, Description, Instructions, ImageUrl, CountryId, IsModerated, CreatedByUserId |
| CocktailTranslations | CocktailId, LangCode, Name, Description, Instructions |
| Ingredients | Id, Name |
| IngredientTranslations | IngredientId, LangCode, Name |
| CocktailIngredients | CocktailId, IngredientId, Measure |
| Users | Id, Email, PasswordHash, Role |
| Favorites | UserId, CocktailId |

---

## 7. API Endpoints

| Method | Endpoint | Purpose |
|--------|----------|---------|
| GET | /api/cocktails?name=&countryId=&ingredientIds=&freeTextTags=&matchAllIngredients=&lang=&page=&pageSize= | List with filters |
| GET | /api/cocktails/{id}?lang= | Details |
| POST | /api/cocktails | Create (auth) |
| GET | /api/countries | Countries dropdown |
| GET | /api/ingredients?search= | Autocomplete |
| POST | /api/auth/register | Register |
| POST | /api/auth/login | Login |
| POST/DELETE | /api/favorites/{id} | Toggle favorite |
| GET | /api/admin/pending | Pending cocktails |
| PUT | /api/admin/cocktails/{id}/approve | Approve |

---

## 8. Frontend Search UI

- **Name search:** Separate text input
- **Ingredient search:** IngredientSearchBar (autocomplete + free text)
- **Match-all checkbox:** “Must contain ALL selected ingredients”
- **Country filter:** Dropdown
- **Lang:** Passed from i18n to API

---

## 9. Technical Notes

- **CORS:** Allow `http://localhost:5173`
- **TheCocktailDB:** Free key `1`; respect rate limits
- **Translation seeding:** Set `CocktailHub:SeedTranslations: true` to translate recipes (slower)
- **PostgreSQL:** Port 5433 (docker-compose) to avoid conflict with local instance
