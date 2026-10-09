import {useEffect, useLayoutEffect, useState} from 'react';
import {Outlet, useLocation} from 'react-router-dom';
import {useTranslation} from 'react-i18next';
import {ErrorBoundary, FallbackProps} from 'react-error-boundary';
import {toast} from 'react-toastify';
import classNames from 'classnames';
import Footer from '@app/modules/main/footer/Footer';
import Header from '@app/modules/main/header/Header';
import MenuSidebar from '@app/modules/main/menu-sidebar/MenuSidebar';
import {addWindowClass, removeWindowClass} from '@app/utils/helpers';

const ErrorFallback = ({error}: FallbackProps) => {
  const [t] = useTranslation();
  return (
    <div>
      <div className="alert alert-danger" role="alert">
        <h5>
          <i className="icon fas fa-ban" />{' '}
          {t('shared.generic.unexpectedError')}
        </h5>
        <ul>
          <li>{error instanceof Error ? error.message : String(error)}</li>
        </ul>
      </div>
      <div className="row">
        <div className="col-sm-12" style={{textAlign: 'right'}}>
          <button
            type="button"
            className="btn btn-primary"
            onClick={() => window.location.reload()}
          >
            {t('shared.buttons.refreshPage')}
          </button>
        </div>
      </div>
    </div>
  );
};

const Main = () => {
  const [t] = useTranslation();
  const [menuVisible, setMenuVisible] = useState(true);
  const location = useLocation();

  // Il contenitore che scorre è #root: a ogni cambio pagina si torna in cima.
  useLayoutEffect(() => {
    const rootItem = document.getElementById('root');
    if (rootItem) rootItem.scrollTop = 0;
  }, [location.pathname]);

  useEffect(() => {
    addWindowClass('sidebar-mini');
    return () => removeWindowClass('sidebar-mini');
  }, []);

  return (
    <div
      className={classNames(
        'wrapper',
        menuVisible ? 'my-menu-visible' : 'my-menu-not-visible'
      )}
    >
      <Header onToggleMenu={() => setMenuVisible((visible) => !visible)} />
      {menuVisible && <MenuSidebar />}
      <div className="content-wrapper">
        <section className="content">
          <ErrorBoundary
            key={location.pathname}
            FallbackComponent={ErrorFallback}
            onError={() => toast.error(t('shared.toast.unexpectedError'))}
          >
            <Outlet />
          </ErrorBoundary>
        </section>
      </div>
      <Footer />
    </div>
  );
};

export default Main;
