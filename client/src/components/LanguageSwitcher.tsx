import { useTranslation } from 'react-i18next';

const languages = [
  { code: 'uk', label: 'UA' },
  { code: 'en', label: 'EN' },
  { code: 'pl', label: 'PL' },
] as const;

export function LanguageSwitcher() {
  const { i18n } = useTranslation();

  return (
    <div className="flex gap-1 rounded-lg bg-amber-900/50 p-1">
      {languages.map(({ code, label }) => (
        <button
          key={code}
          type="button"
          onClick={() => i18n.changeLanguage(code)}
          className={`px-2 py-1 rounded text-sm font-medium transition-colors ${
            i18n.language === code || i18n.language.startsWith(code)
              ? 'bg-amber-600 text-amber-50'
              : 'text-amber-200 hover:bg-amber-800/80'
          }`}
        >
          {label}
        </button>
      ))}
    </div>
  );
}
