import React from "react";
import { inject, observer } from "mobx-react";
import { withTranslation } from "react-i18next";
import {
  getCommonFoldersTree,
  getFolder,
  getFoldersTree,
  getThirdPartyFoldersTree,
} from "@appserver/common/api/files";
import PropTypes from "prop-types";
import ModalDialog from "@appserver/components/modal-dialog";
import toastr from "studio/toastr";
import {
  exceptSortedByTagsFolders,
  exceptPrivacyTrashFolders,
} from "./ExceptionFoldersConstants";
import RootPage from "./SubComponents/RootPage";
import ElementsPage from "./SubComponents/ElementsPage";
import IconButton from "@appserver/components/icon-button";
import Button from "@appserver/components/button";
import Loaders from "@appserver/common/components/Loaders";
import { FolderType } from "@appserver/common/constants";
import {
  StyledRootPage,
  StyledBody,
  StyledHeader,
} from "./StyledSelectFolderDialog";

class SelectFolderDialog extends React.PureComponent {
  constructor(props) {
    super(props);
    const { filter, t, id } = props;
    this.rootTitle = t("SelectFolder");
    this.newFilter = filter.clone();

    this.state = {
      resultingFolderTree: [],
      isDataLoading: false,
      isInitialLoader: false,
      isNextPageLoading: false,
      page: 0,
      hasNextPage: true,

      id: id ? id : "root",
      title: this.rootTitle,
      folders: [],
    };
  }
  filterFoldersTree = (folders, arrayOfExceptions) => {
    let newArray = [];

    for (let i = 0; i < folders.length; i++) {
      if (!arrayOfExceptions.includes(folders[i].rootFolderType)) {
        newArray.push(folders[i]);
      }
    }

    return newArray;
  };

  getRequestFolderTree = () => {
    const { foldersType } = this.props;

    switch (foldersType) {
      case "exceptSortedByTags":
      case "exceptPrivacyTrashFolders":
        try {
          return getFoldersTree();
        } catch (err) {
          console.error(err);
        }
        break;
      case "common":
        try {
          return getCommonFoldersTree();
        } catch (err) {
          console.error(err);
        }
        break;

      case "third-party":
        try {
          return getThirdPartyFoldersTree();
        } catch (err) {
          console.error(err);
        }
        break;
    }
  };

  getExceptionsFolders = (treeFolders) => {
    const { foldersType } = this.props;

    switch (foldersType) {
      case "exceptSortedByTags":
        return this.filterFoldersTree(treeFolders, exceptSortedByTagsFolders);
      case "exceptPrivacyTrashFolders":
        return this.filterFoldersTree(treeFolders, exceptPrivacyTrashFolders);
    }
  };

  async componentDidMount() {
    const {
      treeFolders,
      foldersType,
      id,
      onSetBaseFolderPath,
      onSelectFolder,
      foldersList,
    } = this.props;

    let requestedTreeFolders, filteredTreeFolders, passedId;

    const treeFoldersLength = treeFolders.length;

    let timerId = setTimeout(() => {
      this.setState({ isInitialLoader: true });
    }, 1000);

    if (treeFoldersLength === 0) {
      try {
        requestedTreeFolders = foldersList
          ? foldersList
          : await this.getRequestFolderTree();

        clearTimeout(timerId);
        timerId = null;
      } catch (e) {
        toastr.error(e);
        clearTimeout(timerId);

        timerId = null;
        this.setState({ isInitialLoader: false });

        return;
      }
    }

    const foldersTree =
      treeFoldersLength > 0 ? treeFolders : requestedTreeFolders;

    if (id || foldersType === "common") {
      passedId = id ? id : foldersTree[0].id;

      onSetBaseFolderPath && onSetBaseFolderPath(passedId);
      onSelectFolder && onSelectFolder(passedId);
    }

    if (
      foldersType === "exceptSortedByTags" ||
      foldersType === "exceptPrivacyTrashFolders"
    ) {
      filteredTreeFolders = this.getExceptionsFolders(foldersTree);
    }

    this.setState({
      resultingFolderTree: filteredTreeFolders || foldersTree,
      isInitialLoader: false,
      ...(foldersType === "common" && { id: passedId }),
    });
  }

  deletedCurrentFolderIdFromPathParts = (pathParts) => {
    pathParts.splice(-1, 1);
  };

  onButtonClick = (e) => {
    const { id } = this.state;
    const {
      onClose,
      onSelectFolder,
      onSetNewFolderPath,
      id: propsId,
    } = this.props;

    if (propsId) {
      if (+propsId !== +id) {
        onSetNewFolderPath && onSetNewFolderPath(id);
        onSelectFolder && onSelectFolder(id);
      }
    } else {
      onSetNewFolderPath && onSetNewFolderPath(id);
      onSelectFolder && onSelectFolder(id);
    }

    onClose && onClose();
  };
  onArrowClickAction = async () => {
    const { pathParts } = this.state;

    const newPathParts = [...pathParts];
    const prevFolderId = newPathParts.pop();
    const isRootFolder = newPathParts.length === 0;

    if (!isRootFolder) {
      try {
        this.setState({
          id: prevFolderId,
          folders: [],
          hasNextPage: true,
          page: 0,
          isDataLoading: true,
        });
      } catch (e) {
        toastr.error(e);
      }
    } else {
      this.setState({
        isDataLoading: false,
        folders: [],
        title: this.rootTitle,
        id: "root",
        pathParts: [],
      });
    }
  };

  onRowClick = (id) => {
    this.setState({
      id,
      folders: [],
      page: 0,
      hasNextPage: true,
      isDataLoading: true,
    });
  };

  _loadNextPage = () => {
    const { id, folders, page } = this.state;
    const { withoutProvider } = this.props;
    let dataWithoutProvider;
    if (this._isLoadNextPage) return;

    this._isLoadNextPage = true;

    const pageCount = 30;
    this.newFilter.page = page;
    this.newFilter.pageCount = pageCount;

    this.setState({ isNextPageLoading: true }, async () => {
      try {
        const data = await getFolder(id, this.newFilter);

        if (
          data.current.rootFolderType === FolderType.COMMON &&
          withoutProvider
        ) {
          dataWithoutProvider = data.folders.filter((value) => {
            if (!value.providerKey) return value;
          });
        }

        const finalData = withoutProvider ? dataWithoutProvider : data.folders;

        const newFoldersList = [...folders].concat(finalData);

        const hasNextPage = finalData.length === pageCount;

        let firstLoadInfo = {};
        if (page === 0) {
          const pathParts = [...data.pathParts];
          this.deletedCurrentFolderIdFromPathParts(pathParts);

          firstLoadInfo = {
            pathParts: ["root", ...pathParts],
            title: data.current.title,
          };
        }

        this._isLoadNextPage = false;
        this.setState((state) => ({
          isDataLoading: false,
          hasNextPage: hasNextPage,
          isNextPageLoading: false,
          page: state.page + 1,
          folders: newFoldersList,
          ...firstLoadInfo,
        }));
      } catch (e) {
        toastr.error(e);
        this.setState({
          isDataLoading: false,
        });
      }
    });
  };

  render() {
    const {
      isPanelVisible,
      onClose,
      t,
      footer: footerChild,
      header: headerChild,
    } = this.props;
    const {
      resultingFolderTree,
      isDataLoading,
      isInitialLoader,
      isNextPageLoading,
      hasNextPage,
      folders,
      id,
      title,
      page,
    } = this.state;

    const isRootPage = id === "root";

    const loadingText = `${t("Common:LoadingProcessing")} ${t(
      "Common:LoadingDescription"
    )}`;
    return (
      <ModalDialog
        visible={isPanelVisible}
        zIndex={310}
        onClose={onClose}
        displayType="aside"
        withoutBodyScroll
        contentHeight="100%"
        contentPaddingBottom="0px"
      >
        <ModalDialog.Header>
          <StyledHeader>
            {!isRootPage ? (
              <div className="dialog-header">
                <IconButton
                  iconName="/static/images/arrow.path.react.svg"
                  size="17"
                  isFill={true}
                  className="arrow-button"
                  onClick={this.onArrowClickAction}
                />
                {title}
              </div>
            ) : (
              title
            )}
          </StyledHeader>
        </ModalDialog.Header>

        <ModalDialog.Body>
          {isRootPage && !isDataLoading ? (
            <StyledRootPage>
              {isInitialLoader ? (
                <div className="root-loader" key="loader">
                  <Loaders.RootFoldersTree />
                </div>
              ) : (
                <RootPage
                  data={resultingFolderTree}
                  onClick={this.onRowClick}
                />
              )}
            </StyledRootPage>
          ) : (
            <StyledBody footerChild={!!footerChild} headerChild={!!headerChild}>
              <div className="select-folder_content-body">
                <div className="select-dialog_header-child">{headerChild}</div>
                <div>
                  <ElementsPage
                    hasNextPage={hasNextPage}
                    isNextPageLoading={isNextPageLoading}
                    id={id}
                    folders={folders}
                    loadNextPage={this._loadNextPage}
                    onClick={this.onRowClick}
                    loadingText={loadingText}
                    page={page}
                    t={t}
                  />
                </div>
                <div className="select-dialog_footer">
                  <div className="select-dialog_footer-child">
                    {footerChild}
                  </div>
                  <div className="select-dialog_buttons">
                    <Button
                      //theme={theme}
                      primary
                      size="small"
                      label={t("SaveHere")}
                      onClick={this.onButtonClick}
                      isDisabled={isDataLoading}
                    />
                    <Button
                      size="small"
                      label={t("Common:CancelButton")}
                      onClick={onClose}
                      isDisabled={isDataLoading}
                    />
                  </div>
                </div>
              </div>
            </StyledBody>
          )}
        </ModalDialog.Body>
      </ModalDialog>
    );
  }
}

SelectFolderDialog.propTypes = {
  onClose: PropTypes.func.isRequired,
  isPanelVisible: PropTypes.bool.isRequired,
  onSelectFolder: PropTypes.func.isRequired,
  foldersType: PropTypes.oneOf([
    "common",
    "third-party",
    "exceptSortedByTags",
    "exceptPrivacyTrashFolders",
  ]).isRequired,
  id: PropTypes.oneOfType([PropTypes.string, PropTypes.number]),
  withoutProvider: PropTypes.bool,
};

SelectFolderDialog.defaultProps = {
  isPanelVisible: false,
  foldersType: "common",
  id: "",
  withoutProvider: false,
};

export default inject(({ treeFoldersStore, filesStore }) => {
  const { treeFolders } = treeFoldersStore;
  const { filter } = filesStore;
  return {
    treeFolders,
    filter,
  };
})(observer(withTranslation(["SelectFolder", "Common"])(SelectFolderDialog)));
