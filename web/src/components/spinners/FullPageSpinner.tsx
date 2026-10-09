export default function FullPageSpinner() {
  return (
    <div
      className="spinner-container"
      style={{height: '100vh', display: 'flex', alignItems: 'center'}}
    >
      <div className="loading-spinner-fullpage" />
    </div>
  );
}
