import {useEffect, useState} from 'react';
import {Link} from 'react-router-dom';
import {MenuItem} from '@components';
import {MenuItemMapper} from '@app/mappers/MenuItemMapper';
import MenuItemDto from '@app/models/dtos/MenuItemDto';
import {UserRepository} from '@app/repositories/UserRepository';

// Il nome del prodotto è un marchio: non si traduce.
const APP_NAME = 'FlowDesk';

// Il menu arriva dall'API già filtrato in base ai permessi dell'utente.
const MenuSidebar = () => {
  const [menuItems, setMenuItems] = useState<MenuItemDto[]>([]);

  useEffect(() => {
    let ignore = false;
    UserRepository.getMenu()
      .then((menu) => {
        if (!ignore) setMenuItems(menu.map((m) => MenuItemMapper(m, 0)));
      })
      .catch(() => {
        if (!ignore) setMenuItems([]);
      });
    return () => {
      ignore = true;
    };
  }, []);

  return (
    <aside className="main-sidebar elevation-4 sidebar-dark-primary">
      <Link to="/" className="brand-link">
        <span className="brand-text font-weight-light">{APP_NAME}</span>
      </Link>
      <div className="sidebar">
        <nav className="mt-2" style={{overflowY: 'hidden'}}>
          <ul className="nav nav-pills nav-sidebar flex-column">
            {menuItems.map((menuItem) => (
              <MenuItem
                key={menuItem.name + (menuItem.path ?? '')}
                menuItem={menuItem}
              />
            ))}
          </ul>
        </nav>
      </div>
    </aside>
  );
};

export default MenuSidebar;
