/// <reference types="vite/client" />

interface ImportMetaEnv {
  /** API base URL without path, e.g. https://myapp.azurewebsites.net — omit for dev (Vite proxy). */
  readonly VITE_API_BASE_URL?: string;
}
