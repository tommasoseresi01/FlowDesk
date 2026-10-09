import MenuItemDto from '@app/models/dtos/MenuItemDto';

export function MenuItemMapper(item: any, level: number): MenuItemDto {
  return {
    name: item.name,
    icon: item.icon,
    path: item.path,
    level,
    children: (item.children ?? []).map((child: any) =>
      MenuItemMapper(child, level + 1)
    )
  };
}
