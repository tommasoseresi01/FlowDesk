import {
  Navigate,
  Route,
  createBrowserRouter,
  createRoutesFromElements
} from 'react-router-dom';
import {Crumb, RouteHandle} from '@app/components/breadcrumbs/Breadcrumbs';
import RoleEnum from '@app/models/enums/RoleEnum';
import Main from '@app/modules/main/Main';
import CustomersPage from '@app/pages/CustomersPage';
import Home from '@app/pages/Home';
import NoAccess from '@app/pages/NoAccess';
import NotFound from '@app/pages/NotFound';
import PrivateRoute from '@app/routes/PrivateRoute';
import {RequireRole} from '@app/routes/RequireRole';

// Costruisce l'handle di una rotta: un link di breadcrumb con etichetta tradotta.
const crumb = (to: string, labelKey: string): RouteHandle => ({
  crumb: () => <Crumb to={to} labelKey={labelKey} />
});

export function createAppRouter(isUserEnabled: boolean) {
  if (!isUserEnabled) {
    return createBrowserRouter(
      createRoutesFromElements(
        <Route element={<Main />}>
          <Route path="*" element={<NoAccess />} />
        </Route>
      )
    );
  }

  return createBrowserRouter(
    createRoutesFromElements(
      <Route
        path="/"
        element={<Main />}
        handle={crumb('/', 'menusidebar.label.home')}
      >
        <Route element={<PrivateRoute />}>
          <Route index element={<Navigate to="/home" replace />} />
          <Route path="home" element={<Home />} />

          {/* L'amministratore configura il sistema ma non lavora su clienti e pratiche. */}
          <Route
            path="customers"
            element={
              <RequireRole allowedRoles={[RoleEnum.MANAGER, RoleEnum.OPERATOR]}>
                <CustomersPage />
              </RequireRole>
            }
            handle={crumb('/customers', 'menusidebar.label.customers')}
          />

          <Route path="noaccess" element={<NoAccess />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Route>
    )
  );
}
