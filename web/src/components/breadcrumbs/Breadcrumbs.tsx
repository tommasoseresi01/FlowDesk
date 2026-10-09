import {ReactNode} from 'react';
import {Link, Params, useMatches} from 'react-router-dom';
import {useTranslation} from 'react-i18next';
import classNames from 'classnames';

export type RouteHandle = {
  crumb: (params: Params<string>) => ReactNode;
};

const hasCrumb = (handle: unknown): handle is RouteHandle =>
  typeof (handle as RouteHandle | undefined)?.crumb === 'function';

export const Crumb = ({to, labelKey}: {to: string; labelKey: string}) => {
  const [t] = useTranslation();
  return <Link to={to}>{t(labelKey)}</Link>;
};

// Compone i breadcrumb dalle rotte attive: ogni rotta contribuisce con il proprio handle.crumb.
export function Breadcrumbs() {
  const matches = useMatches();
  const crumbs = matches
    .filter((match) => hasCrumb(match.handle))
    .map((match) => (match.handle as RouteHandle).crumb(match.params));

  return (
    <ol className="breadcrumb float-sm-right" style={{lineHeight: '38px'}}>
      {crumbs.map((crumb, index) => (
        <li
          key={index}
          className={classNames('breadcrumb-item', {
            active: index === crumbs.length - 1
          })}
        >
          {crumb}
        </li>
      ))}
    </ol>
  );
}
