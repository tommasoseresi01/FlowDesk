import {toast} from 'react-toastify';
import i18n from '@app/utils/i18n';

export function showToast(success: boolean) {
  if (success) {
    toast.success(i18n.t('shared.toast.operationSuccess'));
  } else {
    toast.error(i18n.t('shared.toast.operationError'));
  }
}

type ClosableDialog = {
  closeHandler: (refresh: boolean) => void;
};

// Esito del submit di una modale: successo -> chiude e fa ricaricare la lista;
// errore -> la modale resta aperta e torna visibile il form.
export function showToastResult(
  success: boolean,
  model: ClosableDialog,
  setIsLoading: (value: boolean) => void
) {
  if (success) {
    model.closeHandler(true);
  } else {
    setIsLoading(false);
  }
  showToast(success);
}
