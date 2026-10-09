import {
  BrowserCacheLocation,
  Configuration,
  PublicClientApplication
} from '@azure/msal-browser';

export const msalConfig: Configuration = {
  auth: {
    clientId: import.meta.env.VITE_AUTH_CLIENT_ID,
    authority: `https://login.microsoftonline.com/${import.meta.env.VITE_AUTH_TENANT_ID}`,
    redirectUri: import.meta.env.VITE_AUTH_REDIRECT_URI,
    navigateToLoginRequestUrl: true
  },
  cache: {
    cacheLocation: BrowserCacheLocation.LocalStorage,
    storeAuthStateInCookie: false
  }
};

export const loginRequest = {
  scopes: [import.meta.env.VITE_AUTH_SCOPE]
};

export const msalInstance = new PublicClientApplication(msalConfig);
