import { useEffect, useState } from 'react';
import { CocktailCard } from '../../components/CocktailCard';
import { CountryFilter } from '../../components/CountryFilter';
import { IngredientSearchBar } from '../../components/SearchBar/IngredientSearchBar';
import type { SelectedIngredient } from '../../components/SearchBar/IngredientSearchBar';
import { fetchCocktails } from '../../api/cocktails';
import { fetchCountries } from '../../api/countries';
import { addFavorite, removeFavorite, getMyFavorites } from '../../api/favorites';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import type { Country } from '../../api/countries';

export function SearchPage() {
  const { t } = useTranslation();
  const [cocktails, setCocktails] = useState<Awaited<ReturnType<typeof fetchCocktails>> | null>(null);
  const [countries, setCountries] = useState<Country[]>([]);
  const [countryId, setCountryId] = useState<number | ''>('');
  const [selectedIngredients, setSelectedIngredients] = useState<SelectedIngredient[]>([]);
  const [favorites, setFavorites] = useState<Set<number>>(new Set());
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    fetchCountries().then(setCountries);
  }, []);

  useEffect(() => {
    if (isAuthenticated) {
      getMyFavorites().then((ids) => setFavorites(new Set(ids)));
    }
  }, [isAuthenticated]);

  useEffect(() => {
    const ingredientIds = selectedIngredients
      .filter((s): s is { id: number; name: string } => s.id !== null)
      .map((s) => s.id);
    const freeTextTags = selectedIngredients
      .filter((s) => s.id === null)
      .map((s) => s.name);
    fetchCocktails({
      countryId: countryId || undefined,
      ingredientIds: ingredientIds.length ? ingredientIds : undefined,
      freeTextTags: freeTextTags.length ? freeTextTags : undefined,
      page: 1,
      pageSize: 24,
    }).then(setCocktails);
  }, [countryId, selectedIngredients]);

  const toggleFavorite = async (id: number) => {
    if (!isAuthenticated) return;
    const isFav = favorites.has(id);
    try {
      if (isFav) {
        await removeFavorite(id);
        setFavorites((prev) => {
          const next = new Set(prev);
          next.delete(id);
          return next;
        });
      } else {
        await addFavorite(id);
        setFavorites((prev) => new Set(prev).add(id));
      }
    } catch (e) {
      console.error(e);
    }
  };

  const isFavorite = (id: number) => favorites.has(id);

  return (
    <div className="py-6">
      <h1 className="text-2xl font-bold text-amber-50 mb-4">{t('search.title')}</h1>
      <div className="space-y-4 mb-6">
        <IngredientSearchBar value={selectedIngredients} onChange={setSelectedIngredients} placeholder={t('search.placeholder')} />
        <CountryFilter countries={countries} value={countryId} onChange={setCountryId} />
      </div>
      {cocktails ? (
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {cocktails.items.map((c) => (
            <CocktailCard
              key={c.id}
              id={c.id}
              name={c.name}
              imageUrl={c.imageUrl}
              countryName={c.countryName}
              isFavorite={isFavorite(c.id)}
              onToggleFavorite={isAuthenticated ? toggleFavorite : undefined}
            />
          ))}
        </div>
      ) : (
        <p className="text-amber-200">{t('common.loading')}</p>
      )}
    </div>
  );
}
