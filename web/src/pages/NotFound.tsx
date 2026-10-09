import {useTranslation} from 'react-i18next';

const NotFound = () => {
  const [t] = useTranslation();
  return (
    <div style={{textAlign: 'center'}}>
      <div style={{fontSize: '80px'}}>404</div>
      <div style={{fontSize: '60px'}}>{t('pages.notFound.title')}</div>
      <div style={{fontSize: '30px', marginTop: '20px'}}>
        {t('pages.notFound.message')}
      </div>
    </div>
  );
};

export default NotFound;
