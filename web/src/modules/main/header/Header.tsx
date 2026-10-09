import {useTranslation} from 'react-i18next';
import {useMsal} from '@azure/msal-react';

const Header = ({onToggleMenu}: {onToggleMenu: () => void}) => {
  const [t] = useTranslation();
  const {instance} = useMsal();
  const username = instance.getActiveAccount()?.username ?? '';

  return (
    <nav className="main-header navbar navbar-expand navbar-light">
      <ul className="navbar-nav">
        <li className="nav-item">
          <button
            type="button"
            className="nav-link"
            aria-label={t('header.label.toggleMenu')}
            onClick={onToggleMenu}
          >
            <i className="fa fa-bars" />
          </button>
        </li>
      </ul>
      <ul className="navbar-nav ml-auto">
        <li className="nav-item">
          <span className="nav-link">{username}</span>
        </li>
        <li className="nav-item">
          <button
            type="button"
            className="nav-link"
            onClick={() => instance.logoutRedirect()}
          >
            {t('header.label.signOut')} <i className="fas fa-sign-out-alt" />
          </button>
        </li>
      </ul>
    </nav>
  );
};

export default Header;
