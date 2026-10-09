import {useState} from 'react';
import {useLocation, useNavigate} from 'react-router-dom';
import {useTranslation} from 'react-i18next';
import classNames from 'classnames';
import MenuItemDto from '@app/models/dtos/MenuItemDto';

const isPathActive = (item: MenuItemDto, pathname: string): boolean =>
  Boolean(item.path) &&
  (pathname === item.path || pathname.startsWith(`${item.path}/`));

const hasActiveChild = (item: MenuItemDto, pathname: string): boolean =>
  item.children.some(
    (child) => isPathActive(child, pathname) || hasActiveChild(child, pathname)
  );

// Voce di menu ricorsiva: una voce con figli si espande, una foglia naviga.
const MenuItem = ({menuItem}: {menuItem: MenuItemDto}) => {
  const [t] = useTranslation();
  const navigate = useNavigate();
  const {pathname} = useLocation();

  const isExpandable = menuItem.children.length > 0;
  const isActive = isExpandable
    ? hasActiveChild(menuItem, pathname)
    : isPathActive(menuItem, pathname);
  const [isMenuExtended, setIsMenuExtended] = useState(isActive);

  // Quando la pagina corrente entra o esce da questo ramo, l'apertura si riallinea.
  const [wasActive, setWasActive] = useState(isActive);
  if (wasActive !== isActive) {
    setWasActive(isActive);
    setIsMenuExtended(isExpandable && isActive);
  }

  const handleMainMenuAction = () => {
    if (isExpandable) {
      setIsMenuExtended((extended) => !extended);
      return;
    }
    navigate(menuItem.path ?? '/');
  };

  return (
    <li
      className={classNames(`nav-level${menuItem.level}`, 'nav-item', {
        'menu-open': isMenuExtended
      })}
    >
      <button
        type="button"
        className={classNames('nav-link', {active: isActive})}
        aria-expanded={isExpandable ? isMenuExtended : undefined}
        aria-current={!isExpandable && isActive ? 'page' : undefined}
        onClick={handleMainMenuAction}
      >
        <i className={menuItem.icon} />
        <p>
          {t(menuItem.name)}
          {isExpandable && <i className="right fas fa-angle-left" />}
        </p>
      </button>

      {isExpandable && (
        <ul className="nav nav-treeview">
          {menuItem.children.map((item) => (
            <MenuItem key={item.name + (item.path ?? '')} menuItem={item} />
          ))}
        </ul>
      )}
    </li>
  );
};

export default MenuItem;
