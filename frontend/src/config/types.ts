export interface FrontendConfig {
  keycloakBaseUrl: string;
  keycloakRealm: 'gestauto-dev' | 'gestauto-hml' | 'gestauto';
  keycloakClientId: string;
  commercialApiBaseUrl: string;
  stockApiBaseUrl: string;
  vehicleEvaluationApiBaseUrl: string;
  version?: string;
  otelEndpoint?: string;
}
