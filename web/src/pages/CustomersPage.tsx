import {useCallback, useEffect, useMemo, useState} from 'react';
import {useTranslation} from 'react-i18next';
import {Button, OverlayTrigger, Tooltip} from 'react-bootstrap';
import Form from 'react-bootstrap/Form';
import {
  MaterialReactTable,
  useMaterialReactTable,
  type MRT_ColumnDef,
  type MRT_PaginationState,
  type MRT_SortingState,
  type MRT_Updater
} from 'material-react-table';
import {MRT_Localization_IT} from 'material-react-table/locales/it';
import {Box} from '@mui/material';
import {ContentHeader} from '@components';
import Card from '@app/components/card/Card';
import CardBody from '@app/components/card/CardBody';
import CardHeader from '@app/components/card/CardHeader';
import {CustomerMapper} from '@app/mappers/CustomerMapper';
import DeleteCustomerDialog, {
  DeleteCustomerDialogProps
} from '@app/modals/Customers/DeleteCustomerDialog';
import EditCustomerDialog, {
  EditCustomerDialogProps
} from '@app/modals/Customers/EditCustomerDialog';
import CustomFilter from '@app/models/dtos/CustomFilter';
import Customer from '@app/models/dtos/Customer';
import CustomerStatusFilterEnum from '@app/models/enums/CustomerStatusFilterEnum';
import RoleEnum from '@app/models/enums/RoleEnum';
import {useConfirm} from '@app/providers/ConfirmProvider';
import {useCurrentUser} from '@app/providers/UserProvider';
import {CustomerRepository} from '@app/repositories/CustomerRepository';
import {formatDateFromUtc} from '@app/utils/dateUtils';
import {showToast} from '@app/utils/toastUtils';

const actionsBoxSx = {
  display: 'flex',
  gap: '2ch',
  alignItems: 'center',
  justifyContent: 'center'
};

const CustomersPage = () => {
  const [t] = useTranslation();
  const confirm = useConfirm();
  const currentUser = useCurrentUser();

  // Archiviare e ripristinare un cliente spetta al solo responsabile.
  const canArchive = currentUser.role.idRole === RoleEnum.MANAGER;

  // --- stato delle modali
  const [editDialog, setEditDialog] = useState(new EditCustomerDialogProps());
  const [deleteDialog, setDeleteDialog] = useState(
    new DeleteCustomerDialogProps()
  );

  // --- stato della tabella
  const [data, setData] = useState<Customer[]>([]);
  const [rowCount, setRowCount] = useState(0);
  const [isError, setIsError] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [refreshKey, setRefreshKey] = useState(0);
  const [sorting, setSorting] = useState<MRT_SortingState>([
    {id: 'legalName', desc: false}
  ]);
  const [pagination, setPagination] = useState<MRT_PaginationState>({
    pageIndex: 0,
    pageSize: 50
  });

  // --- filtri: valori digitati (bozza) e filtri applicati (inviati all'API)
  const [legalNameFilter, setLegalNameFilter] = useState('');
  const [vatNumberFilter, setVatNumberFilter] = useState('');
  const [statusFilter, setStatusFilter] = useState<string>(
    CustomerStatusFilterEnum.ACTIVE
  );
  const [customFilters, setCustomFilters] = useState<CustomFilter[]>([
    {id: 'statusFilter', value: CustomerStatusFilterEnum.ACTIVE}
  ]);

  const handleSearch = () => {
    setCustomFilters([
      {id: 'legalNameFilter', value: legalNameFilter},
      {id: 'vatNumberFilter', value: vatNumberFilter},
      {id: 'statusFilter', value: statusFilter}
    ]);
    setPagination((prev) => ({...prev, pageIndex: 0}));
  };

  // --- apertura / chiusura modali
  const closeEditDialog = useCallback((refresh: boolean) => {
    setEditDialog(new EditCustomerDialogProps());
    if (refresh) setRefreshKey((key) => key + 1);
  }, []);

  const closeDeleteDialog = useCallback((refresh: boolean) => {
    setDeleteDialog(new DeleteCustomerDialogProps());
    if (refresh) setRefreshKey((key) => key + 1);
  }, []);

  const openEditDialog = useCallback(
    (idCustomer: number) => {
      const model = new EditCustomerDialogProps();
      model.isOpen = true;
      model.idCustomer = idCustomer;
      model.title =
        idCustomer === 0
          ? t('pages.customersPage.createRecordModalTitle')
          : t('pages.customersPage.editRecordModalTitle');
      model.closeHandler = closeEditDialog;
      setEditDialog(model);
    },
    [t, closeEditDialog]
  );

  const openDeleteDialog = useCallback(
    (idCustomer: number) => {
      const model = new DeleteCustomerDialogProps();
      model.isOpen = true;
      model.idCustomer = idCustomer;
      model.title = t('pages.customersPage.deleteRecordModalTitle');
      model.closeHandler = closeDeleteDialog;
      setDeleteDialog(model);
    },
    [t, closeDeleteDialog]
  );

  const handleRestore = useCallback(
    async (customer: Customer) => {
      const confirmed = await confirm(
        t('pages.customersPage.restoreConfirmTitle'),
        t('pages.customersPage.restoreConfirmQuestion', {
          legalName: customer.legalName
        })
      );
      if (!confirmed) return;

      try {
        await CustomerRepository.restore(customer.idCustomer);
        showToast(true);
        setRefreshKey((key) => key + 1);
      } catch {
        showToast(false);
      }
    },
    [t, confirm]
  );

  // --- colonne
  const columns = useMemo<MRT_ColumnDef<Customer>[]>(
    () => [
      {
        id: 'actions',
        header: '',
        size: 100,
        enableSorting: false,
        muiTableHeadCellProps: {align: 'center'},
        Header: () => (
          <Box sx={actionsBoxSx}>
            <OverlayTrigger
              placement="top"
              overlay={
                <Tooltip id="tooltip_customer_add">
                  <strong>{t('pages.customersPage.tooltips.add')}</strong>
                </Tooltip>
              }
            >
              <Button
                variant="success"
                aria-label={t('pages.customersPage.tooltips.add')}
                onClick={() => openEditDialog(0)}
              >
                <i className="fa fa-plus" />
              </Button>
            </OverlayTrigger>
          </Box>
        ),
        Cell: ({row}) => (
          <Box sx={actionsBoxSx}>
            {!row.original.archived && (
              <OverlayTrigger
                placement="top"
                overlay={
                  <Tooltip id="tooltip_customer_edit">
                    <strong>{t('pages.customersPage.tooltips.edit')}</strong>
                  </Tooltip>
                }
              >
                <Button
                  aria-label={t('pages.customersPage.tooltips.edit')}
                  onClick={() => openEditDialog(row.original.idCustomer)}
                >
                  <i className="fa fa-edit" />
                </Button>
              </OverlayTrigger>
            )}
            {canArchive && !row.original.archived && (
              <OverlayTrigger
                placement="top"
                overlay={
                  <Tooltip id="tooltip_customer_delete">
                    <strong>{t('pages.customersPage.tooltips.delete')}</strong>
                  </Tooltip>
                }
              >
                <Button
                  variant="danger"
                  aria-label={t('pages.customersPage.tooltips.delete')}
                  onClick={() => openDeleteDialog(row.original.idCustomer)}
                >
                  <i className="fa fa-archive" />
                </Button>
              </OverlayTrigger>
            )}
            {canArchive && row.original.archived && (
              <OverlayTrigger
                placement="top"
                overlay={
                  <Tooltip id="tooltip_customer_restore">
                    <strong>{t('pages.customersPage.tooltips.restore')}</strong>
                  </Tooltip>
                }
              >
                <Button
                  variant="secondary"
                  aria-label={t('pages.customersPage.tooltips.restore')}
                  onClick={() => handleRestore(row.original)}
                >
                  <i className="fa fa-undo" />
                </Button>
              </OverlayTrigger>
            )}
          </Box>
        )
      },
      {
        accessorKey: 'legalName',
        header: t('pages.customersPage.legalName')
      },
      {
        accessorKey: 'vatNumber',
        header: t('pages.customersPage.vatNumber')
      },
      {
        accessorKey: 'contactName',
        header: t('pages.customersPage.contactName')
      },
      {
        accessorKey: 'email',
        header: t('pages.customersPage.email')
      },
      {
        id: 'archived',
        header: t('pages.customersPage.status'),
        accessorFn: (row) =>
          row.archived
            ? t('pages.customersPage.statusArchived')
            : t('pages.customersPage.statusActive')
      },
      {
        id: 'userModification',
        header: t('shared.generic.userModification'),
        enableSorting: false,
        accessorFn: (row) => row.userModification?.email ?? ''
      },
      {
        accessorKey: 'dateModification',
        header: t('shared.generic.dateModification'),
        Cell: ({row}) => (
          <span>
            {row.original.dateModification
              ? formatDateFromUtc(
                  row.original.dateModification,
                  'dd/MM/yyyy HH:mm'
                )
              : ''}
          </span>
        )
      }
    ],
    [t, canArchive, openEditDialog, openDeleteDialog, handleRestore]
  );

  // --- caricamento dati (server-side)
  useEffect(() => {
    let ignore = false;

    const fetchData = async () => {
      setIsLoading(true);
      try {
        const [rows, totResultNumber] = await CustomerRepository.search(
          pagination.pageIndex * pagination.pageSize,
          pagination.pageSize,
          customFilters,
          '',
          sorting
        );
        if (ignore) return;
        setData(rows.map((row) => CustomerMapper(row)));
        setRowCount(totResultNumber);
        setIsError(false);
      } catch {
        if (!ignore) setIsError(true);
      } finally {
        if (!ignore) setIsLoading(false);
      }
    };

    fetchData();
    return () => {
      ignore = true;
    };
  }, [
    pagination.pageIndex,
    pagination.pageSize,
    sorting,
    customFilters,
    refreshKey
  ]);

  const handleSortingChange = (updater: MRT_Updater<MRT_SortingState>) => {
    setSorting(updater);
    setPagination((prev) => ({...prev, pageIndex: 0}));
  };

  const table = useMaterialReactTable({
    columns,
    data,
    localization: MRT_Localization_IT,
    enableFilters: false,
    enableColumnActions: false,
    enableDensityToggle: false,
    enableHiding: false,
    enableFullScreenToggle: false,
    autoResetPageIndex: false,
    manualPagination: true,
    manualSorting: true,
    rowCount,
    onPaginationChange: setPagination,
    onSortingChange: handleSortingChange,
    muiToolbarAlertBannerProps: isError
      ? {color: 'error', children: t('shared.generic.errorLoadingData')}
      : undefined,
    state: {
      isLoading,
      pagination,
      sorting,
      showAlertBanner: isError,
      showProgressBars: isLoading
    }
  });

  return (
    <div>
      <ContentHeader title={t('pages.customersPage.title')} />
      <section className="content">
        <div className="container-fluid">
          <Form
            onSubmit={(e) => {
              e.preventDefault();
              handleSearch();
            }}
          >
            <Card>
              <CardHeader title={t('shared.generic.filters')} />
              <CardBody>
                <Form.Group className="row">
                  <Form.Label
                    className="col-sm-1 col-form-label"
                    htmlFor="legalNameFilter"
                  >
                    {t('pages.customersPage.legalName')}
                  </Form.Label>
                  <Form.Control
                    className="col-sm-5"
                    type="text"
                    id="legalNameFilter"
                    name="legalNameFilter"
                    value={legalNameFilter}
                    onChange={(e) => setLegalNameFilter(e.target.value)}
                  />
                  <Form.Label
                    className="col-sm-1 col-form-label"
                    htmlFor="vatNumberFilter"
                  >
                    {t('pages.customersPage.vatNumber')}
                  </Form.Label>
                  <Form.Control
                    className="col-sm-5"
                    type="text"
                    id="vatNumberFilter"
                    name="vatNumberFilter"
                    value={vatNumberFilter}
                    onChange={(e) => setVatNumberFilter(e.target.value)}
                  />
                </Form.Group>
                <Form.Group className="row">
                  <Form.Label
                    className="col-sm-1 col-form-label"
                    htmlFor="statusFilter"
                  >
                    {t('pages.customersPage.status')}
                  </Form.Label>
                  <Form.Control
                    as="select"
                    className="col-sm-5"
                    id="statusFilter"
                    name="statusFilter"
                    value={statusFilter}
                    onChange={(e) => setStatusFilter(e.target.value)}
                  >
                    <option value={CustomerStatusFilterEnum.ACTIVE}>
                      {t('pages.customersPage.statusFilterActive')}
                    </option>
                    <option value={CustomerStatusFilterEnum.ARCHIVED}>
                      {t('pages.customersPage.statusFilterArchived')}
                    </option>
                    <option value={CustomerStatusFilterEnum.ALL}>
                      {t('shared.generic.all')}
                    </option>
                  </Form.Control>
                  <div className="col-sm-4" />
                  <div
                    className="col-sm-2"
                    style={{display: 'flex', justifyContent: 'end'}}
                  >
                    <button type="submit" className="btn btn-primary">
                      {t('shared.generic.search')}
                      <i
                        className="fa fa-search"
                        style={{marginLeft: '10px'}}
                      />
                    </button>
                  </div>
                </Form.Group>
              </CardBody>
            </Card>
          </Form>
          <Card>
            <CardHeader title={t('shared.generic.results')} />
            <CardBody>
              <MaterialReactTable table={table} />
            </CardBody>
          </Card>
        </div>
      </section>
      <EditCustomerDialog model={editDialog} />
      <DeleteCustomerDialog model={deleteDialog} />
    </div>
  );
};

export default CustomersPage;
