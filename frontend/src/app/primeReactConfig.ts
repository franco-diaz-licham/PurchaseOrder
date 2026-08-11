import type { PrimeReactProps } from '@primereact/types/core';
import { appConfig } from './appConfig';

export const primeReactConfig: PrimeReactProps = {
  ripple: true,
  license: appConfig.primeReactLicense
};
