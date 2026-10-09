import {ReactNode} from 'react';

const CardBody = ({children}: {children: ReactNode}) => {
  return <div className="card-body">{children}</div>;
};

export default CardBody;
