import classNames from 'classnames';

export default function LoadingSpinner({
  additionalClass
}: {
  additionalClass?: string;
}) {
  return (
    <div className="spinner-container">
      <div className={classNames('loading-spinner', additionalClass)} />
    </div>
  );
}
