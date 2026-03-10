import { Link, useLocation } from 'react-router-dom';
import { Home, Search, Heart, Shield, LogIn } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { useAuth } from '../../context/AuthContext';

export function BottomNav() {
  const { t } = useTranslation();
  const location = useLocation();
  const { isAuthenticated, isAdmin } = useAuth();

  const navItems = [
    { to: '/', icon: Home, label: t('nav.home') },
    { to: '/search', icon: Search, label: t('nav.search') },
    { to: '/favorites', icon: Heart, label: t('nav.favorites') },
    ...(isAdmin ? [{ to: '/admin', icon: Shield, label: t('nav.admin') }] : []),
    { to: isAuthenticated ? '/profile' : '/login', icon: LogIn, label: isAuthenticated ? t('nav.profile') : t('nav.login') },
  ];

  return (
    <nav className="fixed bottom-0 left-0 right-0 z-50 bg-amber-950/95 backdrop-blur border-t border-amber-800/50 md:hidden">
      <div className="flex justify-around py-2">
        {navItems.map(({ to, icon: Icon, label }) => {
          const isActive = location.pathname === to || (to !== '/' && location.pathname.startsWith(to));
          return (
            <Link
              key={to}
              to={to}
              className={`flex flex-col items-center gap-1 px-4 py-1 rounded-lg transition-colors ${
                isActive ? 'text-amber-300' : 'text-amber-100/70 hover:text-amber-200'
              }`}
            >
              <Icon size={22} strokeWidth={1.5} />
              <span className="text-xs font-medium">{label}</span>
            </Link>
          );
        })}
      </div>
    </nav>
  );
}
