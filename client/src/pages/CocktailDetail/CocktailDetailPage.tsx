import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import { Heart } from 'lucide-react';
import { fetchCocktailById } from '../../api/cocktails';
import { addFavorite, removeFavorite } from '../../api/favorites';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';
import type { CocktailDetail } from '../../api/cocktails';

export function CocktailDetailPage() {
  const { t, i18n } = useTranslation();
  const { id } = useParams();
  const [cocktail, setCocktail] = useState<CocktailDetail | null>(null);
  const [isFavorite, setIsFavorite] = useState(false);
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    if (!id) return;
    fetchCocktailById(Number(id), i18n.language).then((c) => {
      setCocktail(c);
      setIsFavorite(c.isFavorite);
    });
  }, [id, i18n.language]);

  const toggleFavorite = async () => {
    if (!isAuthenticated || !cocktail) return;
    try {
      if (isFavorite) {
        await removeFavorite(cocktail.id);
        setIsFavorite(false);
      } else {
        await addFavorite(cocktail.id);
        setIsFavorite(true);
      }
    } catch (e) {
      console.error(e);
    }
  };

  if (!cocktail) return <p className="text-amber-200 py-8">{t('common.loading')}</p>;

  return (
    <div className="py-6">
      <div className="flex gap-4">
        <div className="w-32 h-32 flex-shrink-0 rounded-2xl overflow-hidden bg-amber-950">
          {cocktail.imageUrl ? (
            <img src={cocktail.imageUrl} alt={cocktail.name} className="w-full h-full object-cover" />
          ) : (
            <div className="w-full h-full flex items-center justify-center text-4xl">🍸</div>
          )}
        </div>
        <div className="flex-1 min-w-0">
          <h1 className="text-2xl font-bold text-amber-50 truncate">{cocktail.name}</h1>
          <div className="flex items-center gap-2">
          <p className="text-amber-300">{cocktail.countryName}</p>
          {!cocktail.isModerated && (
            <span className="px-2 py-0.5 rounded text-xs bg-amber-700/80 text-amber-200">{t('cocktail.pendingModeration')}</span>
          )}
        </div>
          {isAuthenticated && (
            <button
              type="button"
              onClick={toggleFavorite}
              className="mt-2 flex items-center gap-2 text-amber-200 hover:text-amber-100"
            >
              <Heart size={20} className={isFavorite ? 'fill-amber-400 text-amber-400' : ''} />
              {isFavorite ? 'Remove from favorites' : 'Add to favorites'}
            </button>
          )}
        </div>
      </div>
      {cocktail.description && (
        <p className="mt-4 text-amber-200/90">{cocktail.description}</p>
      )}
      <section className="mt-6">
        <h2 className="text-lg font-semibold text-amber-50 mb-2">{t('cocktail.ingredients')}</h2>
        <ul className="space-y-1">
          {cocktail.ingredients.map((ing) => (
            <li key={ing.ingredientId} className="text-amber-200">
              {ing.ingredientName}
              {ing.measure ? ` — ${ing.measure}` : ''}
            </li>
          ))}
        </ul>
      </section>
      <section className="mt-6">
        <h2 className="text-lg font-semibold text-amber-50 mb-2">{t('cocktail.instructions')}</h2>
        <p className="text-amber-200/90 whitespace-pre-wrap">{cocktail.instructions}</p>
      </section>
    </div>
  );
}
