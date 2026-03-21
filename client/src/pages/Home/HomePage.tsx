import { useEffect, useState } from 'react';
import { CocktailCard } from '../../components/CocktailCard';
import { CountryFilter } from '../../components/CountryFilter';
import { fetchCocktails } from '../../api/cocktails';
import { fetchCountries } from '../../api/countries';
import { addFavorite, removeFavorite, getMyFavorites } from '../../api/favorites';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import type { Country } from '../../api/countries';

export function HomePage() {
  const { t, i18n } = useTranslation();
  const [cocktails, setCocktails] = useState<Awaited<ReturnType<typeof fetchCocktails>> | null>(null);
  const [countries, setCountries] = useState<Country[]>([]);
  const [countryId, setCountryId] = useState<number | ''>('');
  const [page, setPage] = useState(1);
  const [favorites, setFavorites] = useState<Set<number>>(new Set());
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    fetchCountries({ showNonEmptyOnly: true, showCoctailCountsInName: true }).then(setCountries);
  }, []);

  useEffect(() => {
    if (isAuthenticated) {
      getMyFavorites().then((ids) => setFavorites(new Set(ids)));
    }
  }, [isAuthenticated]);

  useEffect(() => {
    fetchCocktails({
      countryId: countryId || undefined,
      lang: i18n.language,
      page,
      pageSize: 12,
    }).then(setCocktails);
  }, [countryId, page, i18n.language]);

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
      <h1 className="text-2xl font-bold text-amber-50 mb-4">{t('home.title')}</h1>
      <div className="flex flex-wrap gap-4 mb-6">
        <CountryFilter countries={countries} value={countryId} onChange={setCountryId} />
      </div>
      {cocktails ? (
        <>
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
          {cocktails.total > cocktails.pageSize && (
            <div className="flex justify-center gap-2 mt-6">
              <button
                type="button"
                disabled={page <= 1}
                onClick={() => setPage((p) => p - 1)}
                className="px-4 py-2 rounded-lg bg-amber-800 disabled:opacity-50"
              >
                {t('common.prev')}
              </button>
              <span className="px-4 py-2 text-amber-200">
                {page} / {Math.ceil(cocktails.total / cocktails.pageSize)}
              </span>
              <button
                type="button"
                disabled={page >= Math.ceil(cocktails.total / cocktails.pageSize)}
                onClick={() => setPage((p) => p + 1)}
                className="px-4 py-2 rounded-lg bg-amber-800 disabled:opacity-50"
              >
                {t('common.next')}
              </button>
            </div>
          )}
        </>
      ) : (
        <p className="text-amber-200">{t('common.loading')}</p>
      )}
    </div>
  );
}
