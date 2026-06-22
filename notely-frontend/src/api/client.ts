import axios from 'axios';
import type { AuthResponse } from './auth';
import { getAccessToken, getRefreshToken, setAccessToken, setRefreshToken } from './tokenStore';

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || '',
});

// Attach Bearer token to every request
api.interceptors.request.use((config) => {
  const token = getAccessToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

// On 401: try to refresh, retry the original request once, then bail
let isRefreshing = false;
let queue: Array<{ resolve: (token: string) => void; reject: (err: unknown) => void }> = [];

const flushQueue = (token: string | null, error: unknown = null) => {
  queue.forEach((p) => (token ? p.resolve(token) : p.reject(error)));
  queue = [];
};

api.interceptors.response.use(
  (res) => res,
  async (error) => {
    const original = error.config;

    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error);
    }

    const refreshToken = getRefreshToken();
    if (!refreshToken) {
      clearAuthAndRedirect();
      return Promise.reject(error);
    }

    if (isRefreshing) {
      return new Promise((resolve, reject) => {
        queue.push({
          resolve: (token) => {
            original.headers.Authorization = `Bearer ${token}`;
            resolve(api(original));
          },
          reject,
        });
      });
    }

    original._retry = true;
    isRefreshing = true;

    try {
      const { data } = await axios.post<AuthResponse>(
        `${import.meta.env.VITE_API_URL || ''}/auth/refresh`,
        { refreshToken },
      );
      setAccessToken(data.accessToken);
      setRefreshToken(data.refreshToken);
      flushQueue(data.accessToken);
      original.headers.Authorization = `Bearer ${data.accessToken}`;
      return api(original);
    } catch (err) {
      flushQueue(null, err);
      clearAuthAndRedirect();
      return Promise.reject(err);
    } finally {
      isRefreshing = false;
    }
  },
);

function clearAuthAndRedirect() {
  setAccessToken(null);
  setRefreshToken(null);
  window.location.replace('/login');
}
