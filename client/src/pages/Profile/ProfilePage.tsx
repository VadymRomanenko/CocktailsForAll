import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';

export function ProfilePage() {
  const { t } = useTranslation();
  const { user, logout } = useAuth();

  return (
    <div className="py-6">
      <h1 className="text-2xl font-bold text-amber-50 mb-4">{t('profile.title')}</h1>
      <p className="text-amber-200">{user?.email}</p>
      <p className="text-amber-300/80 text-sm">{t('profile.role')}: {user?.role}</p>
      <div className="mt-6 flex gap-4">
        <Link
          to="/create"
          className="px-4 py-2 rounded-xl bg-amber-600 hover:bg-amber-500 text-amber-950 font-semibold"
        >
          {t('nav.addCocktail')}
        </Link>
        <button
          type="button"
          onClick={logout}
          className="px-4 py-2 rounded-xl bg-amber-800 hover:bg-amber-700 text-amber-100"
        >
          Log out
        </button>
      </div>
    </div>
  );
}
