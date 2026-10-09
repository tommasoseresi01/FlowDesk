import {
  ReactNode,
  createContext,
  useCallback,
  useContext,
  useMemo,
  useState
} from 'react';
import Modal from 'react-modal';
import LoadingSpinner from '@app/components/spinners/LoadingSpinner';

type LoadingContextType = {
  showLoading: (title?: string, message?: string) => void;
  hideLoading: () => void;
};

type LoadingState = {
  isOpen: boolean;
  title: string;
  message: string;
};

const closedState: LoadingState = {isOpen: false, title: '', message: ''};

const LoadingContext = createContext<LoadingContextType | undefined>(undefined);

export const LoadingProvider = ({children}: {children: ReactNode}) => {
  const [state, setState] = useState<LoadingState>(closedState);

  const showLoading = useCallback((title = '', message = '') => {
    setState({isOpen: true, title, message});
  }, []);

  const hideLoading = useCallback(() => {
    setState(closedState);
  }, []);

  // Valore stabile: aprire/chiudere l'overlay non fa ri-renderizzare chi consuma il context.
  const value = useMemo(
    () => ({showLoading, hideLoading}),
    [showLoading, hideLoading]
  );

  return (
    <LoadingContext.Provider value={value}>
      {children}
      <Modal
        className="my-modal-large"
        isOpen={state.isOpen}
        contentLabel={state.title}
      >
        <div className="my-modal-header">
          <h4 className="my-modal-title">{state.title}</h4>
        </div>
        <div className="my-modal-body">
          <LoadingSpinner />
          <p>{state.message}</p>
        </div>
      </Modal>
    </LoadingContext.Provider>
  );
};

export const useLoading = (): LoadingContextType => {
  const context = useContext(LoadingContext);
  if (!context)
    throw new Error('useLoading must be used within LoadingProvider');
  return context;
};
