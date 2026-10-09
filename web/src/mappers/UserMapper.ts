import User from '@app/models/dtos/User';

export function UserMapper(item: any): User {
  return {
    idUser: item.idUser,
    name: item.name,
    surname: item.surname,
    email: item.email,
    enabled: item.enabled,
    role: {
      idRole: item.role.idRole,
      roleName: item.role.roleName
    }
  };
}
