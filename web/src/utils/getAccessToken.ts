import {InteractionRequiredAuthError} from '@azure/msal-browser';
import {loginRequest, msalInstance} from '@app/utils/msal';

// Token silente (MSAL lo rinnova da solo quando scade); se serve interazione, redirect al login.
export async function getAccessToken(): Promise<string> {
  const account =
    msalInstance.getActiveAccount() ?? msalInstance.getAllAccounts()[0];
  if (!account) {
    throw new Error('No authenticated account');
  }

  const request = {...loginRequest, account};
  try {
    const response = await msalInstance.acquireTokenSilent(request);
    return response.accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      await msalInstance.acquireTokenRedirect(request);
    }
    throw error;
  }
}
