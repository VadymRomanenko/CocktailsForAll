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
  countryId?: number;
  ingredientIds?: number[];
  freeTextTags?: string[];
  page?: number;
  pageSize?: number;
}) {
  const sp = new URLSearchParams();
  if (params.countryId) sp.set('countryId', String(params.countryId));
  if (params.ingredientIds?.length) params.ingredientIds.forEach((id) => sp.append('ingredientIds', String(id)));
  if (params.freeTextTags?.length) params.freeTextTags.forEach((t) => sp.append('freeTextTags', t));
  if (params.page) sp.set('page', String(params.page));
  if (params.pageSize) sp.set('pageSize', String(params.pageSize));
  return apiGet<CocktailListResponse>(`/cocktails?${sp}`);
}

export function fetchCocktailById(id: number) {
  return apiGet<CocktailDetail>(`/cocktails/${id}`);
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
