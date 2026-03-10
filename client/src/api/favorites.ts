import { apiDelete, apiGet, apiPost } from './client';

export function addFavorite(cocktailId: number) {
  return apiPost<unknown>(`/favorites/${cocktailId}`, {});
}

export function removeFavorite(cocktailId: number) {
  return apiDelete(`/favorites/${cocktailId}`);
}

export function getMyFavorites() {
  return apiGet<number[]>('/favorites');
}
