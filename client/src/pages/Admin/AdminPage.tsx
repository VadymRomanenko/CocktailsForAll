import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { fetchPendingCocktails, approveCocktail } from '../../api/admin';
import type { PendingCocktail } from '../../api/admin';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';

export function AdminPage() {
  const { t } = useTranslation();
  const [pending, setPending] = useState<PendingCocktail[]>([]);
  const { isAdmin } = useAuth();

  useEffect(() => {
    if (isAdmin) {
      fetchPendingCocktails().then(setPending);
    }
  }, [isAdmin]);

  const handleApprove = async (id: number) => {
    await approveCocktail(id);
    setPending((prev) => prev.filter((c) => c.id !== id));
  };

  if (!isAdmin) {
    return (
      <div className="py-6">
        <p className="text-amber-200">{t('admin.accessDenied')}</p>
      </div>
    );
  }

  return (
    <div className="py-6">
      <h1 className="text-2xl font-bold text-amber-50 mb-4">{t('admin.title')}</h1>
      {pending.length ? (
        <div className="space-y-4">
          {pending.map((c) => (
            <div
              key={c.id}
              className="p-4 rounded-xl bg-amber-900/50 border border-amber-700/50"
            >
              <div className="flex items-start gap-4">
                <div className="w-20 h-20 flex-shrink-0 rounded-lg overflow-hidden bg-amber-950">
                  {c.imageUrl ? (
                    <img src={c.imageUrl} alt={c.name} className="w-full h-full object-cover" />
                  ) : (
                    <div className="w-full h-full flex items-center justify-center text-2xl">🍸</div>
                  )}
                </div>
                <div className="flex-1 min-w-0">
                  <Link to={`/cocktail/${c.id}`} className="font-semibold text-amber-100 hover:underline">
                    {c.name}
                  </Link>
                  <p className="text-amber-300/80 text-sm">{c.countryName}</p>
                  <p className="text-amber-200/90 text-sm mt-1 line-clamp-2">{c.instructions}</p>
                  <ul className="mt-2 text-amber-400/80 text-xs flex flex-wrap gap-1">
                    {c.ingredients.map((i) => (
                      <li key={i.ingredientId}>{i.ingredientName};</li>
                    ))}
                  </ul>
                </div>
                <button
                  type="button"
                  onClick={() => handleApprove(c.id)}
                  className="px-4 py-2 rounded-lg bg-green-700 hover:bg-green-600 text-white font-medium"
                >
                  {t('admin.approve')}
                </button>
              </div>
            </div>
          ))}
        </div>
      ) : (
        <p className="text-amber-200">{t('admin.noPending')}</p>
      )}
    </div>
  );
}
