import { BottomNav } from './BottomNav';
import { HeaderNav } from './HeaderNav';
import { LanguageSwitcher } from '../LanguageSwitcher';

interface MainLayoutProps {
  children: React.ReactNode;
}

export function MainLayout({ children }: MainLayoutProps) {
  return (
    <div className="min-h-screen bg-gradient-to-b from-amber-950 via-amber-900 to-amber-950 text-amber-50">
      <header className="px-4 md:px-8 max-w-4xl mx-auto flex items-center justify-between gap-4 py-2 md:py-0">
        <div className="flex-1 md:hidden" />
        <HeaderNav />
        <LanguageSwitcher />
      </header>
      <main className="pb-20 md:pb-8 px-4 md:px-8 max-w-4xl mx-auto">
        {children}
      </main>
      <BottomNav />
    </div>
  );
}
