import React, { useEffect } from "react";
import { inject, observer } from "mobx-react";
import { Navigate, Routes, Route } from "react-router-dom";
import ErrorBoundary from "@docspace/common/components/ErrorBoundary";
import toastr from "@docspace/components/toast/toastr";
import PrivateRoute from "@docspace/common/components/PrivateRoute";
import AppLoader from "@docspace/common/components/AppLoader";
import { /*combineUrl,*/ updateTempContent } from "@docspace/common/utils";
import Home from "./AccountsHome";
import Profile from "./Profile";
import NotificationComponent from "./Notifications";

import Filter from "@docspace/common/api/people/filter";
import { showLoader, hideLoader } from "@docspace/common/utils";

const Error404 = React.lazy(() => import("client/Error404"));

const Error404Route = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Error404 {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const HomeRedirectToFilter = () => {
  const filter = Filter.getDefault();
  const urlFilter = filter.toUrlParams();
  return <Navigate replace to={`/accounts/filter?${urlFilter}`} />;
};

const PeopleSection = React.memo(() => {
  return (
    <Routes>
      <Route
        exact
        path={["/accounts/view/@self"]}
        render={(location) => (
          <PrivateRoute location={location}>
            <Profile />
          </PrivateRoute>
        )}
      />

      <Route
        exact
        path={["/accounts/view/@self/notification"]}
        render={(location) => (
          <PrivateRoute location={location}>
            <NotificationComponent />
          </PrivateRoute>
        )}
      />

      <Route
        exact
        path={["/accounts"]}
        render={(location) => (
          <PrivateRoute restricted withManager location={location}>
            <HomeRedirectToFilter />
          </PrivateRoute>
        )}
      />

      <Route
        exact
        path={"/accounts/filter"}
        render={(location) => (
          <PrivateRoute restricted withManager location={location}>
            <Home />
          </PrivateRoute>
        )}
      />

      <Route
        render={(location) => (
          <PrivateRoute location={location}>
            <Error404Route />
          </PrivateRoute>
        )}
      />
    </Routes>
  );
});

const PeopleContent = (props) => {
  const { loadBaseInfo, isLoading, setFirstLoad } = props;

  useEffect(() => {
    setFirstLoad(false);
  }, [setFirstLoad]);

  useEffect(() => {
    if (isLoading) {
      showLoader();
    } else {
      hideLoader();
    }
  }, [isLoading]);

  useEffect(() => {
    loadBaseInfo()
      .catch((err) => toastr.error(err))
      .finally(() => {
        updateTempContent();
      });
  }, []);

  return <PeopleSection />;
};

const People = inject(({ auth, filesStore, peopleStore }) => {
  const { setFirstLoad, isLoading } = filesStore;

  return {
    loadBaseInfo: async () => {
      await peopleStore.init();
      //auth.setProductVersion(config.version);
    },
    setFirstLoad,
    isLoading,
  };
})(observer(PeopleContent));

export default (props) => <People {...props} />;
