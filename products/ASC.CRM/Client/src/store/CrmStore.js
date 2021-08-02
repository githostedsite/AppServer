import { action, makeObservable, observable } from "mobx";
import config from "../../package.json";
import store from "studio/store";
const { auth: authStore } = store;

class CrmStore {
  isLoading = false;
  isLoaded = false;
  isInit = false;

  firstLoad = true;

  constructor() {
    makeObservable(this, {
      isLoading: observable,
      isLoaded: observable,
      firstLoad: observable,
      setFirstLoad: action,
      setIsLoading: action,
      setIsLoaded: action,
      init: action,
    });
  }

  init = async () => {
    if (this.isInit) return;
    this.isInit = true;

    authStore.settingsStore.setModuleInfo(config.homepage, config.id);

    this.setIsLoaded(true);
  };

  setIsLoading = (loading) => {
    this.isLoading = loading;
  };

  setIsLoaded = (isLoaded) => {
    this.isLoaded = isLoaded;
  };

  setFirstLoad = (firstLoad) => {
    this.firstLoad = firstLoad;
  };
}

export default CrmStore;
