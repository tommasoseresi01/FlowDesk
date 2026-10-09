import {FormEvent, useEffect, useState} from 'react';
import {useTranslation} from 'react-i18next';
import Modal from 'react-modal';
import Form from 'react-bootstrap/Form';
import {useForm} from 'react-hook-form';
import {useErrorBoundary} from 'react-error-boundary';
import ErrorSummary from '@app/components/errors/ErrorSummary';
import LoadingSpinner from '@app/components/spinners/LoadingSpinner';
import useApiError from '@app/hooks/useApiError';
import {CustomerMapper} from '@app/mappers/CustomerMapper';
import Customer from '@app/models/dtos/Customer';
import {CustomerRepository} from '@app/repositories/CustomerRepository';
import {showToastResult} from '@app/utils/toastUtils';

export class DeleteCustomerDialogProps {
  idCustomer: number = 0;
  title: string = '';
  isOpen: boolean = false;
  closeHandler: (refresh: boolean) => void = () => {};
}

// Mostra il cliente in sola lettura e chiede conferma prima di archiviarlo.
// Un cliente non viene mai cancellato: "delete" sul repository significa archiviare.
const DeleteCustomerDialog = ({model}: {model: DeleteCustomerDialogProps}) => {
  const [t] = useTranslation();
  const [isLoading, setIsLoading] = useState(true);
  const [currentRecord, setCurrentRecord] = useState<Customer | null>(null);
  const {
    handleSubmit,
    formState: {errors},
    reset,
    setError,
    clearErrors
  } = useForm({mode: 'onSubmit', reValidateMode: 'onSubmit'});
  const {showBoundary} = useErrorBoundary();
  const apiError = useApiError();

  useEffect(() => {
    if (!model.isOpen) return;
    let ignore = false;

    const fetchData = async () => {
      setIsLoading(true);
      try {
        const record = await CustomerRepository.getById(model.idCustomer);
        if (ignore) return;
        setCurrentRecord(CustomerMapper(record));
        reset();
      } catch (err) {
        if (!ignore) apiError(err, setError, showBoundary);
      } finally {
        if (!ignore) setIsLoading(false);
      }
    };

    fetchData();
    return () => {
      ignore = true;
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [model]);

  const submitDelete = async (): Promise<boolean> => {
    if (!currentRecord) return false;
    try {
      await CustomerRepository.delete(currentRecord.idCustomer);
      return true;
    } catch (e) {
      apiError(e, setError, showBoundary);
      return false;
    }
  };

  const onSubmitValidationComplete = async () => {
    setIsLoading(true);
    const success = await submitDelete();
    showToastResult(success, model, setIsLoading);
  };

  const onSubmitPreValidation = (e: FormEvent) => {
    clearErrors();
    handleSubmit(onSubmitValidationComplete)(e);
  };

  return (
    <Modal
      className="my-modal-large"
      isOpen={model.isOpen}
      contentLabel={model.title}
      onRequestClose={() => model.closeHandler(false)}
    >
      <Form onSubmit={onSubmitPreValidation}>
        <div className="my-modal-header">
          <h4 className="my-modal-title">{model.title}</h4>
          <button
            type="button"
            className="close"
            aria-label={t('shared.buttons.close')}
            onClick={() => model.closeHandler(false)}
          >
            <span aria-hidden="true">×</span>
          </button>
        </div>
        <div className="my-modal-body">
          {isLoading && <LoadingSpinner />}
          {!isLoading && (
            <div>
              <ErrorSummary errors={errors} />
              <Form.Group className="row" controlId="archiveCustomerLegalName">
                <Form.Label className="col-sm-2 col-form-label">
                  {t('dialogs.deleteCustomerDialog.legalName')}
                </Form.Label>
                <Form.Control
                  className="col-sm-10"
                  type="text"
                  value={currentRecord?.legalName ?? ''}
                  readOnly
                />
              </Form.Group>
              <Form.Group className="row" controlId="archiveCustomerVatNumber">
                <Form.Label className="col-sm-2 col-form-label">
                  {t('dialogs.deleteCustomerDialog.vatNumber')}
                </Form.Label>
                <Form.Control
                  className="col-sm-10"
                  type="text"
                  value={currentRecord?.vatNumber ?? ''}
                  readOnly
                />
              </Form.Group>
              <p>{t('dialogs.deleteCustomerDialog.confirmQuestion')}</p>
            </div>
          )}
        </div>
        {!isLoading && (
          <div className="my-modal-footer justify-content-between">
            <button
              type="button"
              className="btn btn-primary"
              onClick={() => model.closeHandler(false)}
            >
              {t('shared.buttons.close')}
            </button>
            <button type="submit" className="btn btn-danger">
              {t('shared.buttons.archive')}
            </button>
          </div>
        )}
      </Form>
    </Modal>
  );
};

export default DeleteCustomerDialog;
