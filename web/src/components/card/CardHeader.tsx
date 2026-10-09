import {ReactNode} from 'react';

type CardHeaderProps = {
  title: string;
  children?: ReactNode;
};

const CardHeader = ({title, children}: CardHeaderProps) => {
  return (
    <div
      className="card-header"
      style={{display: 'flex', alignItems: 'center'}}
    >
      <h3 className="card-title">{title}</h3>
      {children}
    </div>
  );
};

export default CardHeader;
