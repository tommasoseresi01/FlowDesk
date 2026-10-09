import {useTranslation} from 'react-i18next';
import {useMsal} from '@azure/msal-react';
import Alert, {AlertTypeEnum} from '@app/components/alerts/Alert';

const NoAccess = () => {
  const [t] = useTranslation();
  const {instance} = useMsal();
  const username = instance.getActiveAccount()?.username ?? '';

  return (
    <Alert alertType={AlertTypeEnum.ERROR} title={t('pages.noAccess.title')}>
      <div>{t('pages.noAccess.message', {username})}</div>
      <div>{t('pages.noAccess.contactSupport')}</div>
    </Alert>
  );
};

export default NoAccess;
