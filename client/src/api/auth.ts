import { apiPost } from './client';

export interface AuthResponse {
  token: string;
  email: string;
  role: string;
}

export function register(email: string, password: string) {
  return apiPost<AuthResponse>('/auth/register', { email, password });
}

export function login(email: string, password: string) {
  return apiPost<AuthResponse>('/auth/login', { email, password });
}
