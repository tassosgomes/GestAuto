import axios from 'axios';

let tokenGetter: () => string | undefined;

export const setTokenGetter = (getter: () => string | undefined) => {
  tokenGetter = getter;
};

const createApi = () => {
  const instance = axios.create();
  instance.interceptors.request.use((config) => {
    const token = tokenGetter?.();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  });
  return instance;
};

export const commercialApi = createApi();
export const stockApi = createApi();
export const vehicleEvaluationApi = createApi();

export const setCommercialApiBaseUrl = (url: string) => {
  commercialApi.defaults.baseURL = url;
};

export const setStockApiBaseUrl = (url: string) => {
  stockApi.defaults.baseURL = url;
};

export const setVehicleEvaluationApiBaseUrl = (url: string) => {
  vehicleEvaluationApi.defaults.baseURL = url;
};
