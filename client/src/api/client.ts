const getToken = () => localStorage.getItem('token');

/** Production API origin (no trailing slash). Empty = same origin / Vite dev proxy. */
function apiOrigin(): string {
  return (import.meta.env.VITE_API_BASE_URL ?? '').trim().replace(/\/$/, '');
}

export async function api<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = getToken();
  const method = (options.method ?? 'GET').toUpperCase();
  const headers: HeadersInit = {
    ...options.headers,
  };
  const h = headers as Record<string, string>;
  if (method !== 'GET' && method !== 'HEAD' && h['Content-Type'] === undefined) {
    h['Content-Type'] = 'application/json';
  }
  if (token) {
    h['Authorization'] = `Bearer ${token}`;
  }
  const base = apiOrigin();
  const url = base === '' ? `/api${path}` : `${base}/api${path}`;
  const res = await fetch(url, { ...options, headers });
  console.log('api', res);
  if (!res.ok) {
    const err = await res.json().catch(() => ({}));
    throw new Error(err.message || `HTTP ${res.status}`);
  }
  return res.json();
}

export const apiGet = <T>(path: string) => api<T>(path, { method: 'GET' });
export const apiPost = <T>(path: string, body: unknown) =>
  api<T>(path, { method: 'POST', body: JSON.stringify(body) });
export const apiPut = <T>(path: string, body?: unknown) =>
  api<T>(path, { method: 'PUT', body: body ? JSON.stringify(body) : undefined });
export const apiDelete = (path: string) => api<unknown>(path, { method: 'DELETE' });
