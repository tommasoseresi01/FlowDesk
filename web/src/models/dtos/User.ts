import Role from '@app/models/dtos/Role';

type User = {
  idUser: number;
  name: string;
  surname: string;
  email: string;
  enabled: boolean;
  role: Role;
};

export default User;
