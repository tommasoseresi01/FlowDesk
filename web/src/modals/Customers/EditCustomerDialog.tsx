import {FormEvent, useEffect, useState} from 'react';
import {useTranslation} from 'react-i18next';
import Modal from 'react-modal';
import Form from 'react-bootstrap/Form';
import {useForm} from 'react-hook-form';
import {useErrorBoundary} from 'react-error-boundary';
import classNames from 'classnames';
import ErrorSummary from '@app/components/errors/ErrorSummary';
import LoadingSpinner from '@app/components/spinners/LoadingSpinner';
import useApiError from '@app/hooks/useApiError';
import {CustomerMapper} from '@app/mappers/CustomerMapper';
import Customer from '@app/models/dtos/Customer';
import {CustomerRepository} from '@app/repositories/CustomerRepository';
import {showToastResult} from '@app/utils/toastUtils';

export class EditCustomerDialogProps {
  idCustomer: number = 0;
  title: string = '';
  isOpen: boolean = false;
  closeHandler: (refresh: boolean) => void = () => {};
}

type CustomerFormValues = {
  legalName: string;
  vatNumber: string;
  contactName: string;
  email: string;
  phone: string;
};

const VAT_NUMBER_PATTERN = /^\d{11}$/;
const EMAIL_PATTERN = /^\S+@\S+\.\S+$/;

const EditCustomerDialog = ({model}: {model: EditCustomerDialogProps}) => {
  const [t] = useTranslation();
  const [isLoading, setIsLoading] = useState(true);
  const [currentRecord, setCurrentRecord] = useState<Customer | null>(null);
  const {
    register,
    handleSubmit,
    formState: {errors},
    reset,
    setError,
    clearErrors
  } = useForm<CustomerFormValues>({
    mode: 'onSubmit',
    reValidateMode: 'onSubmit'
  });
  const {showBoundary} = useErrorBoundary();
  const apiError = useApiError();

  // All'apertura: carica il record (id = 0 -> record vuoto, nessuna chiamata).
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

  const handleChange = (event: {target: {name: string; value: string}}) => {
    const {name, value} = event.target;
    setCurrentRecord((prev) => (prev ? {...prev, [name]: value} : prev));
  };

  const submitRecord = async (): Promise<boolean> => {
    if (!currentRecord) return false;
    try {
      if (currentRecord.idCustomer === 0) {
        await CustomerRepository.create(currentRecord);
      } else {
        await CustomerRepository.edit(currentRecord);
      }
      return true;
    } catch (e) {
      apiError(e, setError, showBoundary);
      return false;
    }
  };

  const onSubmitValidationComplete = async () => {
    setIsLoading(true);
    const success = await submitRecord();
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
      <Form onSubmit={onSubmitPreValidation} noValidate>
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
              {currentRecord && (
                <div>
                  <Form.Group className="row" controlId="customerLegalName">
                    <Form.Label className="col-sm-2 col-form-label">
                      {t('dialogs.editCustomerDialog.legalName')}
                    </Form.Label>
                    <Form.Control
                      className={classNames('col-sm-10', {
                        'is-invalid': errors.legalName
                      })}
                      type="text"
                      maxLength={200}
                      value={currentRecord.legalName}
                      {...register('legalName', {
                        required: t(
                          'dialogs.editCustomerDialog.legalNameMandatory'
                        ),
                        onChange: handleChange
                      })}
                    />
                  </Form.Group>
                  <Form.Group className="row" controlId="customerVatNumber">
                    <Form.Label className="col-sm-2 col-form-label">
                      {t('dialogs.editCustomerDialog.vatNumber')}
                    </Form.Label>
                    <Form.Control
                      className={classNames('col-sm-10', {
                        'is-invalid': errors.vatNumber
                      })}
                      type="text"
                      inputMode="numeric"
                      maxLength={11}
                      value={currentRecord.vatNumber}
                      {...register('vatNumber', {
                        required: t(
                          'dialogs.editCustomerDialog.vatNumberMandatory'
                        ),
                        pattern: {
                          value: VAT_NUMBER_PATTERN,
                          message: t(
                            'dialogs.editCustomerDialog.vatNumberInvalid'
                          )
                        },
                        onChange: handleChange
                      })}
                    />
                  </Form.Group>
                  <Form.Group className="row" controlId="customerContactName">
                    <Form.Label className="col-sm-2 col-form-label">
                      {t('dialogs.editCustomerDialog.contactName')}
                    </Form.Label>
                    <Form.Control
                      className="col-sm-10"
                      type="text"
                      maxLength={120}
                      value={currentRecord.contactName}
                      {...register('contactName', {onChange: handleChange})}
                    />
                  </Form.Group>
                  <Form.Group className="row" controlId="customerEmail">
                    <Form.Label className="col-sm-2 col-form-label">
                      {t('dialogs.editCustomerDialog.email')}
                    </Form.Label>
                    <Form.Control
                      className={classNames('col-sm-10', {
                        'is-invalid': errors.email
                      })}
                      type="email"
                      maxLength={254}
                      value={currentRecord.email}
                      {...register('email', {
                        pattern: {
                          value: EMAIL_PATTERN,
                          message: t('dialogs.editCustomerDialog.emailInvalid')
                        },
                        onChange: handleChange
                      })}
                    />
                  </Form.Group>
                  <Form.Group className="row" controlId="customerPhone">
                    <Form.Label className="col-sm-2 col-form-label">
                      {t('dialogs.editCustomerDialog.phone')}
                    </Form.Label>
                    <Form.Control
                      className="col-sm-10"
                      type="tel"
                      maxLength={30}
                      value={currentRecord.phone}
                      {...register('phone', {onChange: handleChange})}
                    />
                  </Form.Group>
                </div>
              )}
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
            <button type="submit" className="btn btn-success">
              {t('shared.buttons.save')}
            </button>
          </div>
        )}
      </Form>
    </Modal>
  );
};

export default EditCustomerDialog;
