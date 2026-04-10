import { apiGet, apiPost } from './client';

export interface CocktailListResponse {
  items: Array<{
    id: number;
    name: string;
    imageUrl: string | null;
    countryName: string;
    isFavorite: boolean;
  }>;
  total: number;
  page: number;
  pageSize: number;
}

export interface CocktailDetail {
  id: number;
  name: string;
  description: string | null;
  instructions: string;
  imageUrl: string | null;
  countryId: number;
  countryName: string;
  isModerated: boolean;
  isFavorite: boolean;
  ingredients: Array<{ ingredientId: number; ingredientName: string; measure: string | null }>;
}

export function fetchCocktails(params: {
  name?: string;
  countryId?: number;
  ingredientIds?: number[];
  freeTextTags?: string[];
  matchAllIngredients?: boolean;
  lang?: string;
  page?: number;
  pageSize?: number;
}) {
  console.log('Start fetchCocktails');
  const sp = new URLSearchParams();
  if (params.name) sp.set('name', params.name);
  if (params.countryId) sp.set('countryId', String(params.countryId));
  if (params.ingredientIds?.length) params.ingredientIds.forEach((id) => sp.append('ingredientIds', String(id)));
  if (params.freeTextTags?.length) params.freeTextTags.forEach((t) => sp.append('freeTextTags', t));
  if (params.matchAllIngredients) sp.set('matchAllIngredients', 'true');
  if (params.lang) sp.set('lang', params.lang);
  if (params.page) sp.set('page', String(params.page));
  if (params.pageSize) sp.set('pageSize', String(params.pageSize));
  console.log('End fetchCocktails');
  return apiGet<CocktailListResponse>(`/cocktails?${sp}`);
}

export function fetchCocktailById(id: number, lang?: string) {
  const sp = lang ? `?lang=${lang}` : '';
  return apiGet<CocktailDetail>(`/cocktails/${id}${sp}`);
}

export interface CocktailOfTheDay {
  id: number;
  name: string;
  description: string | null;
  imageUrl: string | null;
  countryId: number;
  countryName: string;
}

export function fetchCocktailOfTheDay(lang?: string) {
  const sp = lang ? `?lang=${lang}` : '';
  return apiGet<CocktailOfTheDay>(`/cocktails/cocktail-of-the-day${sp}`);
}

export function fetchExtendedDescription(id: number, lang?: string) {
  const sp = lang ? `?lang=${lang}` : '';
  return apiGet<{ content: string }>(`/cocktails/${id}/extended-description${sp}`);
}

export function createCocktail(data: {
  name: string;
  description?: string;
  instructions: string;
  imageUrl?: string;
  countryId: number;
  ingredients: Array<{ ingredientId: number; measure?: string }>;
}) {
  return apiPost<{ id: number }>('/cocktails', data);
}
