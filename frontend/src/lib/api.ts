import axios from 'axios';
import { context, propagation, trace, SpanStatusCode } from '@opentelemetry/api';

let tokenGetter: () => string | undefined;

export const setTokenGetter = (getter: () => string | undefined) => {
  tokenGetter = getter;
};

const isTelemetryEnabled = () =>
  import.meta.env.PROD || import.meta.env.VITE_FORCE_TELEMETRY === 'true';

const createApi = () => {
  const instance = axios.create();
  instance.interceptors.request.use((config) => {
    const token = tokenGetter?.();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    if (isTelemetryEnabled()) {
      const activeContext = context.active();
      const headers: Record<string, string> = {};
      propagation.inject(activeContext, headers);
      
      // Inject W3C trace context headers
      Object.entries(headers).forEach(([key, value]) => {
        if (config.headers && typeof config.headers.set === 'function') {
          config.headers.set(key, value);
        } else {
          // Fallback for test environments where headers may not be AxiosHeaders
          (config.headers as any)[key] = value;
        }
      });
    }

    return config;
  });

  instance.interceptors.response.use(
    (response) => response,
    (error) => {
      if (isTelemetryEnabled()) {
        const span = trace.getActiveSpan();
        if (span) {
          const message = error instanceof Error ? error.message : 'Request failed';
          span.setStatus({ code: SpanStatusCode.ERROR, message });
          if (error instanceof Error) {
            span.recordException(error);
          }
        }
      }
      return Promise.reject(error);
    }
  );
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
