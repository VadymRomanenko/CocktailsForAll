import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { CocktailCard } from '../../components/CocktailCard';
import { getMyFavorites } from '../../api/favorites';
import { fetchCocktailById } from '../../api/cocktails';
import { removeFavorite } from '../../api/favorites';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import type { CocktailDetail } from '../../api/cocktails';

export function FavoritesPage() {
  const { t } = useTranslation();
  const [favorites, setFavorites] = useState<CocktailDetail[]>([]);
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    if (!isAuthenticated) return;
    getMyFavorites().then(async (ids) => {
      const items = await Promise.all(ids.map((id) => fetchCocktailById(id)));
      setFavorites(items);
    });
  }, [isAuthenticated]);

  const removeFromFavorites = async (id: number) => {
    await removeFavorite(id);
    setFavorites((prev) => prev.filter((c) => c.id !== id));
  };

  if (!isAuthenticated) {
    return (
      <div className="py-6">
        <h1 className="text-2xl font-bold text-amber-50 mb-4">Favorites</h1>
        <p className="text-amber-200">
          <Link to="/login" className="underline text-amber-300 hover:text-amber-200">
            {t('auth.login')}
          </Link>{' '}
          {t('favorites.loginPrompt')}
        </p>
      </div>
    );
  }

  return (
    <div className="py-6">
      <h1 className="text-2xl font-bold text-amber-50 mb-4">Favorites</h1>
      {favorites.length ? (
        <div className="grid grid-cols-2 md:grid-cols-3 gap-4">
          {favorites.map((c) => (
            <CocktailCard
              key={c.id}
              id={c.id}
              name={c.name}
              imageUrl={c.imageUrl}
              countryName={c.countryName}
              isFavorite
              onToggleFavorite={removeFromFavorites}
            />
          ))}
        </div>
      ) : (
        <p className="text-amber-200">{t('favorites.empty')}</p>
      )}
    </div>
  );
}
