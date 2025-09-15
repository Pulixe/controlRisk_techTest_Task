'use client';

import { PublicClientApplication, type AccountInfo, type PopupRequest, InteractionRequiredAuthError, EventType } from '@azure/msal-browser';

const authorityBase = process.env.NEXT_PUBLIC_AZURE_AUTHORITY || '';
const clientId = process.env.NEXT_PUBLIC_AZURE_CLIENT_ID || '';
const redirectUriEnv = process.env.NEXT_PUBLIC_MSAL_REDIRECT_URI;
const apiAudienceEnv = process.env.NEXT_PUBLIC_API_AUDIENCE;
const apiScopesEnv = process.env.NEXT_PUBLIC_API_SCOPES; // comma or space separated list

// Ensure authority ends with /v2.0
const authority = authorityBase.endsWith('/v2.0') ? authorityBase : `${authorityBase}/v2.0`;

// Prefer explicit env redirect, else use login page for robust redirect handling in dev
const computedRedirectUri = redirectUriEnv ?? 'http://localhost:3000/login';
console.log('MSAL redirectUri', { redirectUriEnv, computedRedirectUri });
export const msalConfig = {
  auth: {
    clientId,
    authority,
    navigateToLoginRequestUrl: false,
    redirectUri: computedRedirectUri,
    postLogoutRedirectUri: computedRedirectUri,
  },
  cache: {
    cacheLocation: 'sessionStorage' as const,
    // Helps in browsers with strict storage policies to preserve auth state across redirects
    storeAuthStateInCookie: true,
  },
  system: {
    redirectNavigationTimeout: 30000,
  },
};

function parseScopes(input?: string): string[] {
  if (!input) return [];
  return input
    .split(/[\s,]+/)
    .map((s) => s.trim())
    .filter(Boolean);
}

function computeDefaultScope() {
  const audience = (apiAudienceEnv || clientId).trim();
  if (audience.startsWith('api://')) {
    const id = audience.slice('api://'.length);
    // If SPA and resource are same app, use GUID-based scope to avoid AADSTS90009
    if (id === clientId) {
      return `${clientId}/.default`;
    }
    return `${audience}/.default`;
  }
  // audience is GUID or custom URI without api:// prefix
  return `${audience}/.default`;
}

// Prefer explicit scopes from env; else compute default
const explicitScopes = parseScopes(apiScopesEnv);
export const apiScopes = explicitScopes.length ? explicitScopes : [computeDefaultScope()];
console.log('MSAL API scopes', apiScopes);

export const loginRequest: PopupRequest = {
  // Include API scopes in interactive flow to ensure consent
  scopes: ['openid', 'profile', 'email'],
};
console.log('MSAL API scopes', apiScopes);

export const msalInstance = new PublicClientApplication(msalConfig);

// Initialize + process any redirect response, set an active account if available.
export const msalReady = msalInstance
  .initialize()
  .then(() => {
    console.log('MSAL initialized');
    return msalInstance.handleRedirectPromise();
  })
  .then((resp) => {
    if (resp) {
      console.log('handleRedirectPromise result', {
        tokenType: resp.tokenType,
        state: resp.state,
        account: { username: resp.account?.username, homeAccountId: resp.account?.homeAccountId },
      });
    } else {
      console.log('handleRedirectPromise returned null');
    }
    const accounts = msalInstance.getAllAccounts();
    console.log('Accounts after redirect', accounts.map(a => ({ username: a.username, homeAccountId: a.homeAccountId })));
    const act = resp?.account ?? accounts[0];
    console.log(act)
    if (act) {
      msalInstance.setActiveAccount(act);
      console.log('Active account set', { username: act.username, homeAccountId: act.homeAccountId });
    } else {
      console.log('No account available after init');
    }
  })
  .catch((e) => console.error('MSAL init/redirect error', e));
// Also set active account on LOGIN_SUCCESS to be safe
msalInstance.addEventCallback((event) => {
  if (event.eventType === EventType.LOGIN_SUCCESS) {
    const account = (event as any).payload?.account as AccountInfo | undefined;
    if (account) {
      msalInstance.setActiveAccount(account);
      console.log('MSAL event LOGIN_SUCCESS, active account set', { username: account.username });
    }
  }
});
console.log('msalInstance', msalInstance);
export async function acquireToken(account?: AccountInfo): Promise<string | undefined> {
  try {
    console.log('acquireToken: start', { account });
    await msalReady;
    console.log('acquireToken: msalReady resolved');
    const active = account ?? (msalInstance.getAllAccounts()[0] as AccountInfo | undefined);
    if (!active) {
      console.log('acquireToken: no active account');
      return undefined;
    }
    console.log('acquireTokenSilent: starting', { username: active.username });
    const res = await msalInstance.acquireTokenSilent({ scopes: apiScopes, account: active });
    console.log('acquireTokenSilent: success', { expiresOn: res.expiresOn?.toISOString?.() });
    return res.accessToken;
  } catch (err: any) {
    if (err instanceof InteractionRequiredAuthError) {
      // Redirect to login and then back
      console.log('acquireToken: interaction required -> loginRedirect');
      await msalInstance.loginRedirect({ ...loginRequest, scopes: apiScopes });
      return undefined;
    }
    // Non-interaction errors: surface but do not crash callers
    console.error('acquireToken error', err);
    return undefined;
  }
}
