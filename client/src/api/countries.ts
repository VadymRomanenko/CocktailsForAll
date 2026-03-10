import { apiGet } from './client';

export interface Country {
  id: number;
  name: string;
  isoCode: string;
}

export function fetchCountries() {
  return apiGet<Country[]>('/countries');
}
