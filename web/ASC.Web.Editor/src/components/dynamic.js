import React from "react";
// import AppLoader from "@appserver/common/components/AppLoader";
// import ErrorBoundary from "@appserver/common/components/ErrorBoundary";
// import Error520 from "studio/Error520";
// import Error404 from "studio/Error404";

export function loadComponent(scope, module, moduleName = null) {
  return async () => {
    // Initializes the share scope. This fills it with known provided modules from this build and all remotes
    await __webpack_init_sharing__("default");
    const container = window[scope]; // or get the container somewhere else
    // Initialize the container, it may provide shared modules
    await container.init(__webpack_share_scopes__.default);
    const factory = await window[scope].get(module);
    const Module = factory();
    if (moduleName)
      window[moduleName] =
        moduleName === "filesUtils" ? Module : Module.default;
    return Module;
  };
}

const useDynamicScript = (args) => {
  const [ready, setReady] = React.useState(false);
  const [failed, setFailed] = React.useState(false);

  React.useEffect(() => {
    if (!args.url) {
      return;
    }
    const exists = document.getElementById(args.id);

    if (exists) {
      setReady(true);
      setFailed(false);
      return;
    }

    const element = document.createElement("script");

    element.id = args.id;
    element.src = args.url;
    element.type = "text/javascript";
    element.async = true;

    setReady(false);
    setFailed(false);

    element.onload = () => {
      console.log(`Dynamic Script Loaded: ${args.url}`);
      setReady(true);
    };

    element.onerror = () => {
      console.error(`Dynamic Script Error: ${args.url}`);
      setReady(false);
      setFailed(true);
    };

    document.head.appendChild(element);

    //TODO: Comment if you don't want to remove loaded remoteEntry
    return () => {
      console.log(`Dynamic Script Removed: ${args.url}`);
      // document.head.removeChild(element);
    };
  }, [args.url]);

  return {
    ready,
    failed,
  };
};

const DynamicComponent = ({ system, ...rest }) => {
  const { ready, failed } = useDynamicScript({
    url: system && system.url,
    id: system && system.scope,
  });

  if (!system) {
    console.log(`Not system specified`);
    throw Error("Not system specified");
  }

  if (!ready) {
    console.log(`Loading dynamic script: ${system.url}`);
    return <div className={rest.className}>Loading</div>;
  }

  if (failed) {
    console.log(`Failed to load dynamic script: ${system.url}`);
    throw Error("failed");
  }

  const Component = React.lazy(
    loadComponent(system.scope, system.module, system?.name)
  );

  return (
    <React.Suspense fallback={<div />}>
      <Component {...rest} />
    </React.Suspense>
  );
};

export default DynamicComponent;