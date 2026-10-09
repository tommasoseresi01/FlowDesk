import {ReactNode} from 'react';
import {Breadcrumbs} from '@app/components/breadcrumbs/Breadcrumbs';

type ContentHeaderProps = {
  title: string;
  children?: ReactNode; // azioni di pagina (es. bottoni), tra titolo e breadcrumb
};

const ContentHeader = ({title, children}: ContentHeaderProps) => {
  return (
    <section className="content-header">
      <div className="container-fluid">
        <div className="row mb-2">
          <div className="col-sm-12">
            <div style={{display: 'flex'}}>
              <h1 style={{flex: 1}}>{title}</h1>
              <div>{children}</div>
              <div style={{marginLeft: '5%'}}>
                <Breadcrumbs />
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
};

export default ContentHeader;
