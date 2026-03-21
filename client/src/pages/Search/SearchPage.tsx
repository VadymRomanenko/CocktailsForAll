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
  const { t, i18n } = useTranslation();
  const [cocktails, setCocktails] = useState<Awaited<ReturnType<typeof fetchCocktails>> | null>(null);
  const [countries, setCountries] = useState<Country[]>([]);
  const [countryId, setCountryId] = useState<number | ''>('');
  const [nameSearch, setNameSearch] = useState('');
  const [selectedIngredients, setSelectedIngredients] = useState<SelectedIngredient[]>([]);
  const [matchAllIngredients, setMatchAllIngredients] = useState(false);
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
      name: nameSearch.trim() || undefined,
      countryId: countryId || undefined,
      ingredientIds: ingredientIds.length ? ingredientIds : undefined,
      freeTextTags: freeTextTags.length ? freeTextTags : undefined,
      matchAllIngredients: matchAllIngredients && ingredientIds.length > 0,
      lang: i18n.language,
      page: 1,
      pageSize: 24,
    }).then(setCocktails);
  }, [countryId, selectedIngredients, nameSearch, matchAllIngredients, i18n.language]);

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
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('search.nameLabel')}</label>
          <input
            type="text"
            value={nameSearch}
            onChange={(e) => setNameSearch(e.target.value)}
            placeholder={t('search.namePlaceholder')}
            className="w-full px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 placeholder-amber-400/60"
          />
        </div>
        <div>
          <label className="block text-amber-200 text-sm mb-1">{t('search.ingredientsLabel')}</label>
          <IngredientSearchBar value={selectedIngredients} onChange={setSelectedIngredients} placeholder={t('search.placeholder')} />
          <label className="flex items-center gap-2 mt-2 text-amber-300 text-sm">
            <input
              type="checkbox"
              checked={matchAllIngredients}
              onChange={(e) => setMatchAllIngredients(e.target.checked)}
            />
            {t('search.matchAllIngredients')}
          </label>
        </div>
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
