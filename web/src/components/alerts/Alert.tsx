import {ReactNode} from 'react';
import classNames from 'classnames';

export enum AlertTypeEnum {
  ERROR = 1,
  WARNING = 2,
  SUCCESS = 3
}

const alertStyles: Record<AlertTypeEnum, {className: string; icon: string}> = {
  [AlertTypeEnum.ERROR]: {className: 'alert-danger', icon: 'fas fa-ban'},
  [AlertTypeEnum.WARNING]: {
    className: 'alert-warning',
    icon: 'fas fa-exclamation-triangle'
  },
  [AlertTypeEnum.SUCCESS]: {className: 'alert-success', icon: 'fas fa-check'}
};

type AlertProps = {
  alertType: AlertTypeEnum;
  title: string;
  children?: ReactNode;
};

const Alert = ({alertType, title, children}: AlertProps) => {
  const style = alertStyles[alertType];

  return (
    <div className={classNames('alert', style.className)}>
      <h5>
        <i className={classNames('icon', style.icon)} />
        {title}
      </h5>
      {children}
    </div>
  );
};

export default Alert;
