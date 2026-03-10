import { apiGet } from './client';

export interface Ingredient {
  id: number;
  name: string;
}

export function searchIngredients(search: string, limit = 20) {
  const sp = new URLSearchParams();
  if (search) sp.set('search', search);
  sp.set('limit', String(limit));
  return apiGet<Ingredient[]>(`/ingredients?${sp}`);
}
