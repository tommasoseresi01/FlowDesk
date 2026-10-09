import {useEffect, useMemo, useState} from 'react';
import {RouterProvider} from 'react-router-dom';
import {ToastContainer} from 'react-toastify';
import {InteractionStatus} from '@azure/msal-browser';
import {
  AuthenticatedTemplate,
  useIsAuthenticated,
  useMsal
} from '@azure/msal-react';
import FullPageSpinner from '@app/components/spinners/FullPageSpinner';
import {UserMapper} from '@app/mappers/UserMapper';
import User from '@app/models/dtos/User';
import {ConfirmProvider} from '@app/providers/ConfirmProvider';
import {UserProvider} from '@app/providers/UserProvider';
import {LoadingProvider} from '@app/providers/WaitingProvider';
import {UserRepository} from '@app/repositories/UserRepository';
import {createAppRouter} from '@app/routes/router';
import {loginRequest} from '@app/utils/msal';

const App = () => {
  const {instance, accounts, inProgress} = useMsal();
  const isAuthenticated = useIsAuthenticated();
  const [currentUser, setCurrentUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isUserEnabled, setIsUserEnabled] = useState(true);

  // 1) Nessun account: redirect alla pagina di login Microsoft.
  useEffect(() => {
    if (
      !isAuthenticated &&
      inProgress === InteractionStatus.None &&
      accounts.length === 0
    ) {
      instance.loginRedirect(loginRequest);
    }
  }, [isAuthenticated, inProgress, accounts, instance]);

  // 2) Account presente: carica l'utente applicativo. Se l'API rifiuta, l'utente non è abilitato.
  useEffect(() => {
    if (accounts.length === 0 || inProgress !== InteractionStatus.None) {
      return;
    }
    let ignore = false;
    instance.setActiveAccount(accounts[0]);

    UserRepository.getCurrentUser()
      .then((res) => {
        if (ignore) return;
        setCurrentUser(UserMapper(res));
        setIsUserEnabled(true);
      })
      .catch(() => {
        if (ignore) return;
        setCurrentUser(null);
        setIsUserEnabled(false);
      })
      .finally(() => {
        if (!ignore) setIsLoading(false);
      });

    return () => {
      ignore = true;
    };
  }, [accounts, inProgress, instance]);

  // Il router va creato una sola volta (non a ogni render) e solo a caricamento concluso.
  const router = useMemo(
    () => (isLoading ? null : createAppRouter(isUserEnabled)),
    [isLoading, isUserEnabled]
  );

  if (!router) {
    return <FullPageSpinner />;
  }

  return (
    <ConfirmProvider>
      <LoadingProvider>
        <UserProvider currentUser={currentUser}>
          <AuthenticatedTemplate>
            <RouterProvider router={router} />
            <ToastContainer
              autoClose={3000}
              draggable={false}
              position="top-right"
              newestOnTop
              closeOnClick
              pauseOnHover
            />
          </AuthenticatedTemplate>
        </UserProvider>
      </LoadingProvider>
    </ConfirmProvider>
  );
};

export default App;
