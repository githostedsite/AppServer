import React, { useEffect } from "react";
import { Provider as ProjectProvider, inject, observer } from "mobx-react";
import { Switch } from "react-router-dom";
import ErrorBoundary from "@appserver/common/components/ErrorBoundary";
import toastr from "studio/toastr";
import PrivateRoute from "@appserver/common/components/PrivateRoute";
import AppLoader from "@appserver/common/components/AppLoader";
import { combineUrl, updateTempContent } from "@appserver/common/utils";
import config from "../package.json";
import i18n from "./i18n";
import { I18nextProvider, withTranslation } from "react-i18next";
import Home from "./pages/Home";
import { AppServerConfig } from "@appserver/common/constants";
import stores from "./store/index";

const { proxyURL } = AppServerConfig;
const homepage = config.homepage;
const PROXY_HOMEPAGE_URL = combineUrl(proxyURL, homepage);
const HOME_URL = combineUrl(PROXY_HOMEPAGE_URL, "/");
const FILTER_URL = combineUrl(PROXY_HOMEPAGE_URL, "/filter");
const TASK_FILTER_URL = combineUrl(PROXY_HOMEPAGE_URL, "/task/filter");
const COMING_SOON_URLS = combineUrl(PROXY_HOMEPAGE_URL, "/projects-soon");

console.log(COMING_SOON_URLS);

const Error404 = React.lazy(() => import("studio/Error404"));
const ComingSoon = React.lazy(() => import("./components/ComingSoon/index"));

const Error404Route = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <Error404 {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const ComingSoonRoute = (props) => (
  <React.Suspense fallback={<AppLoader />}>
    <ErrorBoundary>
      <ComingSoon {...props} />
    </ErrorBoundary>
  </React.Suspense>
);

const ProjectsContent = (props) => {
  const { isLoaded, loadBaseInfo, setIsLoaded } = props;

  useEffect(() => {
    loadBaseInfo()
      .catch((err) => toastr.error(err))
      .finally(() => {
        setIsLoaded(true);
        updateTempContent();
      });
  }, []);
  return (
    <Switch>
      <PrivateRoute path={COMING_SOON_URLS} component={ComingSoonRoute} />
      <PrivateRoute exact path={HOME_URL} component={Home} />
      <PrivateRoute path={FILTER_URL} component={Home} />
      <PrivateRoute path={TASK_FILTER_URL} component={Home} />
      <PrivateRoute component={Error404Route} />
    </Switch>
  );
};

const Projects = inject(({ auth, projectsStore }) => {
  return {
    user: auth.userStore.user,
    isAuthenticated: auth.isAuthenticated,
    isLoaded: auth.isLoaded && projectsStore.isLoaded,
    setIsLoaded: projectsStore.setIsLoaded,
    loadBaseInfo: async () => {
      await projectsStore.init();
      auth.setProductVersion(config.version);
    },
    isLoaded: auth.isLoaded && projectsStore.isLoaded,
  };
})(withTranslation("Common")(observer(ProjectsContent)));

export default (props) => (
  <ProjectProvider {...stores}>
    <I18nextProvider i18n={i18n}>
      <Projects {...props} />
    </I18nextProvider>
  </ProjectProvider>
);
