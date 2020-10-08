import React, { useEffect } from "react";
import { connect } from "react-redux";
import { withRouter } from "react-router";
import { TreeMenu, TreeNode, Icons } from "asc-web-components";
import styled from "styled-components";
import { history, utils, store as initStore } from "asc-web-common";
import { withTranslation, I18nextProvider } from "react-i18next";
import { createI18N } from "../../../helpers/i18n";

import {
  setSelectedNode,
  setExpandSettingsTree,
  setIsErrorSettings,
  getFilesSettings,
  setSelectedFolder,
} from "../../../store/files/actions";
import {
  getIsLoading,
  getSettingsSelectedTreeNode,
  getExpandedSetting,
  getEnableThirdParty,
} from "../../../store/files/selectors";
const { isAdmin } = initStore.auth.selectors;

const i18n = createI18N({
  page: "Settings",
  localesPath: "pages/Settings",
});

const { changeLanguage } = utils;

const StyledTreeMenu = styled(TreeMenu)`
  margin-top: 20px !important;

  .rc-tree-node-selected {
    background: #dfe2e3 !important;
  }

  .rc-tree-treenode-disabled > span:not(.rc-tree-switcher),
  .rc-tree-treenode-disabled > a,
  .rc-tree-treenode-disabled > a span {
    cursor: wait;
  }

  .rc-tree-child-tree .rc-tree-node-content-wrapper > .rc-tree-title {
    width: 99% !important;
    padding-left: 4px !important;
  }

  .rc-tree-child-tree {
    margin-left: 24px;
  }
`;

const PureTreeSettings = ({
  match,
  enableThirdParty,
  isAdmin,
  selectedTreeNode,
  expandedSetting,
  isLoading,
  setSelectedNode,
  setExpandSettingsTree,
  setIsErrorSettings,
  getFilesSettings,
  setSelectedFolder,
  t,
}) => {
  useEffect(() => {
    const { setting } = match.params;
    setSelectedNode([setting]);
    if (setting) setExpandSettingsTree(["settings"]);
  }, [match]);

  useEffect(() => {
    getFilesSettings().catch((e) => setIsErrorSettings(true));
  }, []);

  const switcherIcon = (obj) => {
    if (obj.isLeaf) {
      return null;
    }
    if (obj.expanded) {
      return <Icons.ExpanderDownIcon size="scale" isfill color="dimgray" />;
    } else {
      return <Icons.ExpanderRightIcon size="scale" isfill color="dimgray" />;
    }
  };

  const onSelect = (section) => {
    const path = section[0];

    if (path === "settings") {
      setSelectedNode(["common"]);
      setSelectedFolder({ id: "common" });
      setExpandSettingsTree(section);
      return history.push("/products/files/settings/common");
    }

    setSelectedNode(section);
    setSelectedFolder({ id: section[0] });
    return history.push(`/products/files/settings/${path}`);
  };

  const onExpand = (data) => {
    setExpandSettingsTree(data);
  };

  const renderTreeNode = () => {
    return (
      <TreeNode
        id="settings"
        key="settings"
        title={t("TreeSettingsMenuTitle")}
        isLeaf={false}
        icon={<Icons.SettingsIcon size="scale" isfill color="dimgray" />}
      >
        <TreeNode
          className="settings-node"
          id="common-settings"
          key="common"
          isLeaf={true}
          title={t("TreeSettingsCommonSettings")}
        />
        {isAdmin ? (
          <TreeNode
            className="settings-node"
            id="admin-settings"
            key="admin"
            isLeaf={true}
            title={t("TreeSettingsAdminSettings")}
          />
        ) : null}
        {enableThirdParty ? (
          <TreeNode
            selectable={true}
            className="settings-node"
            id="connected-clouds"
            key="thirdParty"
            isLeaf={true}
            title={t("TreeSettingsConnectedCloud")}
          />
        ) : null}
      </TreeNode>
    );
  };

  const nodes = renderTreeNode();

  return (
    <StyledTreeMenu
      expandedKeys={expandedSetting}
      selectedKeys={selectedTreeNode}
      defaultExpandParent={false}
      disabled={isLoading}
      className="settings-tree-menu"
      switcherIcon={switcherIcon}
      onSelect={onSelect}
      showIcon={true}
      onExpand={onExpand}
      isFullFillSelection={false}
      gapBetweenNodes="22"
      gapBetweenNodesTablet="26"
    >
      {nodes}
    </StyledTreeMenu>
  );
};

const TreeSettingsContainer = withTranslation()(PureTreeSettings);

const TreeSettings = (props) => {
  useEffect(() => {
    changeLanguage(i18n);
  }, []);
  return (
    <I18nextProvider i18n={i18n}>
      <TreeSettingsContainer {...props} />
    </I18nextProvider>
  );
};

function mapStateToProps(state) {
  return {
    selectedTreeNode: getSettingsSelectedTreeNode(state),
    expandedSetting: getExpandedSetting(state),
    enableThirdParty: getEnableThirdParty(state),
    isAdmin: isAdmin(state),
    isLoading: getIsLoading(state),
  };
}

export default connect(mapStateToProps, {
  setSelectedNode,
  setExpandSettingsTree,
  setIsErrorSettings,
  getFilesSettings,
  setSelectedFolder,
})(withRouter(TreeSettings));
