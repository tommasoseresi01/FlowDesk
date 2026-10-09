import {createRoot} from 'react-dom/client';
import {MsalProvider} from '@azure/msal-react';
import Modal from 'react-modal';
import App from '@app/App';
import {msalInstance} from '@app/utils/msal';
import './utils/i18n';
import './index.css';

const container = document.getElementById('root') as HTMLElement;
const root = createRoot(container);

Modal.setAppElement(container);

msalInstance.initialize().then(() => {
  root.render(
    <MsalProvider instance={msalInstance}>
      <App />
    </MsalProvider>
  );
});
