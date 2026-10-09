import {Outlet} from 'react-router-dom';
import {useIsAuthenticated} from '@azure/msal-react';

// Rotta di layout: rende le pagine figlie solo con una sessione MSAL attiva.
// (Il redirect al login è gestito da App.)
const PrivateRoute = () => {
  const isAuthenticated = useIsAuthenticated();
  return isAuthenticated ? <Outlet /> : null;
};

export default PrivateRoute;
