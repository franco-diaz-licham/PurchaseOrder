type AppEnvKey = 'VITE_API_BASE_URL' | 'VITE_APP_NAME' | 'VITE_PRIMEREACT_LICENSE_KEY';

const readEnvValue = (key: AppEnvKey, fallback = '') => import.meta.env[key]?.trim() || fallback;

const rootUrl = readEnvValue('VITE_API_BASE_URL', 'http://localhost:5180').replace(/\/+$/, '');

export const appConfig = {
  apiBaseUrl: `${rootUrl}/api`,
  appName: readEnvValue('VITE_APP_NAME', 'PurchaseOrderApp'),
  primeReactLicense: readEnvValue('VITE_PRIMEREACT_LICENSE_KEY')
} as const;
