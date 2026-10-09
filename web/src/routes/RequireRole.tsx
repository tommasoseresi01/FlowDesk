import {ReactNode} from 'react';
import {Navigate} from 'react-router-dom';
import {useCurrentUser} from '@app/providers/UserProvider';

type RequireRoleProps = {
  allowedRoles: number[];
  children: ReactNode;
};

// Controllo di usabilità: la sicurezza reale è nel backend, che rivalida ogni chiamata.
export function RequireRole({allowedRoles, children}: RequireRoleProps) {
  const currentUser = useCurrentUser();

  if (!allowedRoles.includes(currentUser.role.idRole)) {
    return <Navigate to="/noaccess" replace />;
  }

  return <>{children}</>;
}
