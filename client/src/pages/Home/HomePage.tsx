import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
/* import { CocktailCard } from '../../components/CocktailCard';
import { CountryFilter } from '../../components/CountryFilter'; */
import { fetchCocktails, fetchCocktailOfTheDay, fetchExtendedDescription } from '../../api/cocktails';
import type { CocktailOfTheDay } from '../../api/cocktails';
import { fetchCountries } from '../../api/countries';
import { addFavorite, removeFavorite, getMyFavorites } from '../../api/favorites';
import { useAuth } from '../../context/AuthContext';
/* import type { Country } from '../../api/countries'; */

export function HomePage() {
  const { t, i18n } = useTranslation();
  /* const [cocktails, setCocktails] = useState<Awaited<ReturnType<typeof fetchCocktails>> | null>(null);
  const [countries, setCountries] = useState<Country[]>([]); */
  /* const [countryId, setCountryId] = useState<number | ''>(''); */
  const [page, setPage] = useState(1);
  const [favorites, setFavorites] = useState<Set<number>>(new Set());
  const [cocktailOfTheDay, setCocktailOfTheDay] = useState<CocktailOfTheDay | null>(null);
  const [extendedDescription, setExtendedDescription] = useState<string | null>(null);
  const [extDescLoading, setExtDescLoading] = useState(false);
  const { isAuthenticated } = useAuth();

  /* useEffect(() => {
    fetchCountries({ showNonEmptyOnly: true, showCoctailCountsInName: true }).then(setCountries);
  }, []); */

  useEffect(() => {
    setCocktailOfTheDay(null);
    setExtendedDescription(null);
    fetchCocktailOfTheDay(i18n.language).then(setCocktailOfTheDay);
  }, [i18n.language]);

  useEffect(() => {
    if (!cocktailOfTheDay) return;
    setExtendedDescription(null);
    setExtDescLoading(true);
    fetchExtendedDescription(cocktailOfTheDay.id, i18n.language)
      .then((r) => setExtendedDescription(r.content))
      .catch(() => setExtendedDescription(null))
      .finally(() => setExtDescLoading(false));
  }, [cocktailOfTheDay?.id, i18n.language]);

  useEffect(() => {
    if (isAuthenticated) {
      getMyFavorites().then((ids) => setFavorites(new Set(ids)));
    }
  }, [isAuthenticated]);

  /* useEffect(() => {
    fetchCocktails({
      countryId: countryId || undefined,
      lang: i18n.language,
      page,
      pageSize: 12,
    }).then(setCocktails);
  }, [countryId, page, i18n.language]); */

  /* const toggleFavorite = async (id: number) => {
    if (!isAuthenticated) return;
    const isFav = favorites.has(id);
    try {
      if (isFav) {
        await removeFavorite(id);
        setFavorites((prev) => { const next = new Set(prev); next.delete(id); return next; });
      } else {
        await addFavorite(id);
        setFavorites((prev) => new Set(prev).add(id));
      }
    } catch (e) {
      console.error(e);
    }
  }; */

  /* const isFavorite = (id: number) => favorites.has(id); */

  return (
    <div>
      {/* ── Hero ── */}
      <div className="relative -mx-4 px-4 py-8 mb-4 overflow-hidden rounded-b-3xl bg-gradient-to-br from-amber-950 via-amber-900 to-amber-800">
        <div className="absolute inset-0 opacity-10 text-[14rem] select-none pointer-events-none flex items-center justify-center leading-none">
          🍸
        </div>
        <div className="relative text-center max-w-1xl mx-auto">
          <h1 className="text-4xl sm:text-5xl font-bold text-amber-50 mb-4 tracking-tight">
            {t('home.title')}
          </h1>
          <p className="text-xl font-medium text-amber-300 mb-3">
            {t('home.tagline')}
          </p>
          <p className="text-sm text-amber-200/60 leading-relaxed w-[90%] mx-auto">
            {t('home.description')}
          </p>
        </div>
      </div>

      {/* ── Cocktail of the Day ── */}
      {cocktailOfTheDay && (
        <section className="mb-10">
          <h2 className="text-xl font-semibold text-amber-300 mb-4 flex items-center gap-2">
            <span>✨</span>
            {t('home.cocktailOfTheDay')}
          </h2>

          <Link
            to={`/cocktail/${cocktailOfTheDay.id}`}
            className="flex flex-col sm:flex-row rounded-2xl overflow-hidden bg-amber-900/50 border border-amber-600/50 hover:border-amber-500/70 transition-colors group"
          >
            <div className="sm:w-64 sm:shrink-0 aspect-video sm:aspect-auto bg-amber-950">
              {cocktailOfTheDay.imageUrl ? (
                <img
                  src={cocktailOfTheDay.imageUrl}
                  alt={cocktailOfTheDay.name}
                  className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                />
              ) : (
                <div className="w-full h-full flex items-center justify-center text-6xl">🍸</div>
              )}
            </div>
            <div className="p-6 flex flex-col justify-center gap-3">
              <div>
                <p className="text-xs uppercase tracking-widest text-amber-400/70 mb-1">
                  {cocktailOfTheDay.countryName}
                </p>
                <h3 className="text-2xl font-bold text-amber-50">{cocktailOfTheDay.name}</h3>
              </div>
              {cocktailOfTheDay.description && (
                <p className="text-amber-200/70 text-sm line-clamp-2">{cocktailOfTheDay.description}</p>
              )}
              <span className="self-start mt-1 px-4 py-2 rounded-lg bg-amber-600 hover:bg-amber-500 text-amber-50 text-sm font-medium transition-colors">
                {t('home.viewRecipe')}
              </span>
            </div>
          </Link>

          {/* Extended description */}
          <div className="mt-4 rounded-2xl bg-amber-900/30 border border-amber-700/30 px-6 py-5 min-h-[6rem]">
            {extDescLoading ? (
              <div className="flex items-center justify-center py-6">
                <span className="inline-block w-7 h-7 rounded-full border-2 border-amber-400/30 border-t-amber-400 animate-spin" />
              </div>
            ) : extendedDescription ? (
              <div
                className="text-amber-200/80 text-sm leading-relaxed space-y-2
                  [&_h3]:text-amber-300 [&_h3]:font-semibold [&_h3]:text-base [&_h3]:mb-1
                  [&_strong]:text-amber-200 [&_strong]:font-semibold
                  [&_em]:text-amber-300/80
                  [&_ul]:list-disc [&_ul]:pl-5 [&_ul]:space-y-1
                  [&_p]:mb-1"
                dangerouslySetInnerHTML={{ __html: extendedDescription }}
              />
            ) : null}
          </div>
        </section>
      )}

      {/* ── Browse section ── */}
      {/* <section>
        <h2 className="text-xl font-semibold text-amber-300 mb-4">{t('home.browseCocktails')}</h2>
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
      </section> */}
    </div>
  );
}
