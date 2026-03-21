import { Link, useLocation } from 'react-router-dom';
import { Home, Search, Heart, Shield } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';

export function HeaderNav() {
  const { t } = useTranslation();
  const location = useLocation();
  const { isAuthenticated, isAdmin } = useAuth();

  const navItems = [
    { to: '/', icon: Home, label: t('nav.home') },
    { to: '/search', icon: Search, label: t('nav.search') },
    { to: '/favorites', icon: Heart, label: t('nav.favorites') },
    ...(isAdmin ? [{ to: '/admin', icon: Shield, label: t('nav.admin') }] : []),
  ];

  return (
      <div className="flex items-center justify-between gap-4">
        <Link to="/" className="text-xl font-bold text-amber-50 whitespace-nowrap">Cocktail Hub</Link>
        <nav className="hidden md:flex gap-4">
          {navItems.map(({ to, icon: Icon, label }) => {
            const isActive = location.pathname === to || (to !== '/' && location.pathname.startsWith(to));
            return (
              <Link
                key={to}
                to={to}
                className={`flex items-center gap-2 px-3 py-1.5 rounded-lg transition-colors ${
                  isActive ? 'bg-amber-800/50 text-amber-200' : 'text-amber-300 hover:text-amber-100'
                }`}
              >
                <Icon size={18} />
                {label}
              </Link>
            );
          })}
          <Link
            to={isAuthenticated ? '/profile' : '/login'}
            className="flex items-center gap-2 px-3 py-1.5 rounded-lg text-amber-300 hover:text-amber-100"
          >
            {isAuthenticated ? t('nav.profile') : t('nav.login')}
          </Link>
        </nav>
      </div>
  );
}
