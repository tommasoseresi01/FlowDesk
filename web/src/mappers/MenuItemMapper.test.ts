import {describe, expect, test} from 'vitest';
import {MenuItemMapper} from '@app/mappers/MenuItemMapper';

describe('MenuItemMapper', () => {
  test('assigns increasing levels to nested menu items', () => {
    const menu = MenuItemMapper(
      {
        name: 'menusidebar.label.registry',
        icon: 'nav-icon fas fa-folder',
        children: [
          {
            name: 'menusidebar.label.customers',
            icon: 'nav-icon fas fa-building',
            path: '/customers'
          }
        ]
      },
      0
    );

    expect(menu.level).toBe(0);
    expect(menu.children).toHaveLength(1);
    expect(menu.children[0].level).toBe(1);
    expect(menu.children[0].path).toBe('/customers');
  });

  test('a leaf without children gets an empty list', () => {
    const menu = MenuItemMapper(
      {name: 'menusidebar.label.home', path: '/home'},
      0
    );

    expect(menu.children).toEqual([]);
  });
});
