import {
  ReactNode,
  createContext,
  useCallback,
  useContext,
  useState
} from 'react';
import {useTranslation} from 'react-i18next';
import Modal from 'react-modal';

type ConfirmFn = (
  title: string,
  message: string,
  content?: ReactNode,
  showYes?: boolean
) => Promise<boolean>;

type ConfirmState = {
  title: string;
  message: string;
  content?: ReactNode;
  showYes: boolean;
  resolve: (value: boolean) => void;
};

const ConfirmContext = createContext<ConfirmFn | null>(null);

export const ConfirmProvider = ({children}: {children: ReactNode}) => {
  const [t] = useTranslation();
  const [state, setState] = useState<ConfirmState | null>(null);

  // La Promise resta pendente finché l'utente non sceglie: il chiamante fa semplicemente "await".
  const confirm = useCallback<ConfirmFn>(
    (title, message, content, showYes = true) =>
      new Promise<boolean>((resolve) => {
        setState({title, message, content, showYes, resolve});
      }),
    []
  );

  const answer = (value: boolean) => {
    state?.resolve(value);
    setState(null);
  };

  return (
    <ConfirmContext.Provider value={confirm}>
      {children}
      {state && (
        <Modal
          className="my-modal-large"
          isOpen
          contentLabel={state.title}
          onRequestClose={() => answer(false)}
        >
          <div className="my-modal-header">
            <h4 className="my-modal-title">{state.title}</h4>
          </div>
          <div className="my-modal-body">
            {state.content}
            <p>{state.message}</p>
          </div>
          <div className="my-modal-footer justify-content-between">
            {state.showYes ? (
              <>
                <button
                  type="button"
                  className="btn btn-primary"
                  onClick={() => answer(false)}
                >
                  {t('shared.generic.no')}
                </button>
                <button
                  type="button"
                  className="btn btn-success"
                  onClick={() => answer(true)}
                >
                  {t('shared.generic.yes')}
                </button>
              </>
            ) : (
              <button
                type="button"
                className="btn btn-primary"
                onClick={() => answer(false)}
              >
                {t('shared.buttons.close')}
              </button>
            )}
          </div>
        </Modal>
      )}
    </ConfirmContext.Provider>
  );
};

export const useConfirm = (): ConfirmFn => {
  const ctx = useContext(ConfirmContext);
  if (!ctx) throw new Error('useConfirm must be used within a ConfirmProvider');
  return ctx;
};
