type MenuItemDto = {
  name: string; // chiave i18n, es. "menusidebar.label.customers"
  icon?: string; // classi FontAwesome, es. "nav-icon fas fa-building"
  path?: string;
  level: number;
  children: MenuItemDto[];
};

export default MenuItemDto;
