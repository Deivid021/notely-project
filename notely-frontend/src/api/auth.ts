import axios from 'axios';
import { api } from './client';
import { getRefreshToken } from './tokenStore';

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
}

export async function register(email: string, password: string): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/auth/register', { email, password });
  return data;
}

export async function login(email: string, password: string): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/auth/login', { email, password });
  return data;
}

export async function refresh(refreshToken: string): Promise<AuthResponse> {
  // Use plain axios to avoid the interceptor triggering on this request
  const { data } = await axios.post<AuthResponse>(
    `${import.meta.env.VITE_API_URL || ''}/auth/refresh`,
    { refreshToken },
  );
  return data;
}

export async function logout(): Promise<void> {
  const refreshToken = getRefreshToken();
  if (refreshToken) {
    await api.post('/auth/logout', { refreshToken }).catch(() => {});
  }
}
