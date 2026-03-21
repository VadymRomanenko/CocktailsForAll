import { apiGet } from './client';

export interface Country {
  id: number;
  name: string;
  isoCode: string;
}

export function fetchCountries(params?: { 
    showNonEmptyOnly?: boolean, 
    showCoctailCountsInName?: boolean 
  }) {
  const sp = new URLSearchParams();
  if (params?.showNonEmptyOnly) sp.set('showNonEmptyOnly', 'true');
  if (params?.showCoctailCountsInName) sp.set('showCoctailCountsInName', 'true');
  const query = sp.toString() ? `?${sp}` : '';
  return apiGet<Country[]>(`/countries${query}`);
}
