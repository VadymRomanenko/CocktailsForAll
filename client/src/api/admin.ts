import { apiGet, apiPut } from './client';

export interface PendingCocktail {
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

export function fetchPendingCocktails() {
  return apiGet<PendingCocktail[]>('/admin/pending');
}

export function approveCocktail(id: number) {
  return apiPut<unknown>(`/admin/cocktails/${id}/approve`);
}
