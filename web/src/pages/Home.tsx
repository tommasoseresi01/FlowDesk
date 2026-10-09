import {useTranslation} from 'react-i18next';
import {ContentHeader} from '@components';

const Home = () => {
  const [t] = useTranslation();
  return (
    <div>
      <ContentHeader title={t('pages.home.title')} />
      <section className="content">
        <div className="container-fluid" />
      </section>
    </div>
  );
};

export default Home;
