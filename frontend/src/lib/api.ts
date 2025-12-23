import axios from 'axios';

let tokenGetter: () => string | undefined;

export const setTokenGetter = (getter: () => string | undefined) => {
  tokenGetter = getter;
};

export const setApiBaseUrl = (url: string) => {
  api.defaults.baseURL = url;
};

export const api = axios.create();

api.interceptors.request.use((config) => {
  const token = tokenGetter?.();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});
