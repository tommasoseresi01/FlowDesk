import {ReactNode, createContext, useContext} from 'react';
import User from '@app/models/dtos/User';

export const UserContext = createContext<User | null>(null);

type UserProviderProps = {
  children: ReactNode;
  currentUser: User | null;
};

export const UserProvider = ({children, currentUser}: UserProviderProps) => {
  return (
    <UserContext.Provider value={currentUser}>{children}</UserContext.Provider>
  );
};

// Da usare nelle pagine protette, dove l'utente è sempre valorizzato.
export const useCurrentUser = (): User => {
  const currentUser = useContext(UserContext);
  if (!currentUser) {
    throw new Error('useCurrentUser must be used with an enabled user');
  }
  return currentUser;
};
