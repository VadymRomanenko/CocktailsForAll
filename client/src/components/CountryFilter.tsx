import { useTranslation } from 'react-i18next';
import type { Country } from '../api/countries';

interface CountryFilterProps {
  countries: Country[];
  value: number | '';
  onChange: (id: number | '') => void;
}

export function CountryFilter({ countries, value, onChange }: CountryFilterProps) {
  const { t } = useTranslation();
  return (
    <select
      value={value}
      onChange={(e) => onChange(e.target.value === '' ? '' : Number(e.target.value))}
      className="w-full md:w-auto px-4 py-2 rounded-xl bg-amber-900/50 border border-amber-700/50 text-amber-50 outline-none focus:ring-2 focus:ring-amber-500"
    >
      <option value="">{t('common.allCountries')}</option>
      {countries.map((c) => (
        <option key={c.id} value={c.id}>
          {c.name}
        </option>
      ))}
    </select>
  );
}
