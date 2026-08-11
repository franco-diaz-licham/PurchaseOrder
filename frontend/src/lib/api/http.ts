import axios from 'axios';
import { appConfig } from '@/app/appConfig';

const http = axios.create({
  baseURL: appConfig.apiBaseUrl,
  timeout: 30_000,
  headers: { 'Content-Type': 'application/json' }
});

export default http;
