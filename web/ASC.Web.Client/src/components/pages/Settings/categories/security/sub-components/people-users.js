import React, { Component } from "react";
import { withRouter } from "react-router";
import { withTranslation } from "react-i18next";
import styled from "styled-components";
import Text from "@appserver/components/text";
import RadioButtonGroup from "@appserver/components/radio-button-group";
import Button from "@appserver/components/button";
import ToggleButton from "@appserver/components/toggle-button";
import SearchInput from "@appserver/components/search-input";
import TabContainer from "@appserver/components/tabs-container";
import PeopleSelector from "people/PeopleSelector";

import toastr from "@appserver/components/toast/toastr";
import Loader from "@appserver/components/loader";
import { showLoader, hideLoader } from "@appserver/common/utils";

import { setDocumentTitle } from "../../../../../../helpers/utils";
import { inject } from "mobx-react";

const MainContainer = styled.div`
  width: 100%;

  .access-for-all-wrapper {
    background-color: #f8f9f9;
    padding: 14px;
    margin-bottom: 24px;
  }

  .toggle-btn {
    padding-top: 3px;
  }

  .text-wrapper {
    margin-left: 48px;
  }

  .search_container {
    margin: 12px 0;
  }

  .page_loader {
    position: fixed;
    left: 50%;
  }
`;

class PeopleUsers extends Component {
  constructor(props) {
    super(props);

    const { t } = props;
    this.state = {
      isLoaded: false,
      accessForAll: false,
      searchValue: "",
    };
  }

  async componentDidMount() {
    const { setAddUsers } = this.props;
    showLoader();

    setAddUsers(this.addUsers);

    hideLoader();
    this.setState({ isLoaded: true });
  }

  componentWillUnmount() {
    const { setAddUsers } = this.props;
    setAddUsers("");
  }

  onAccessForAllClick() {
    console.log("Access for all users");

    const { accessForAll } = this.state;
    this.setState({ accessForAll: !accessForAll });
  }

  onSearchChange = (value) => {
    if (this.state.searchValue === value) return false;

    this.setState({
      searchValue: value,
    });
  };

  onSelect = (items) => {
    const { toggleSelector } = this.props;

    toggleSelector(false);
    this.addUsers(items);
  };

  onCancelSelector = () => {
    const { toggleSelector } = this.props;

    toggleSelector(false);
  };

  addUsers = (users) => {
    console.log("addUsers", users);
  };

  render() {
    const { isLoaded, accessForAll, searchValue } = this.state;
    const { t, selectorIsOpen, groupsCaption } = this.props;

    const tabItems = [
      {
        key: "0",
        title: t("Users"),
        content: <h1>Users</h1>,
      },
      {
        key: "1",
        title: t("Groups"),
        content: <h1>Groups</h1>,
      },
    ];

    return !isLoaded ? (
      <Loader className="pageLoader" type="rombs" size="40px" />
    ) : (
      <MainContainer>
        <div className="access-for-all-wrapper">
          <div>
            <ToggleButton
              className="toggle-btn"
              isChecked={accessForAll}
              onChange={() => this.onAccessForAllClick()}
              isDisabled={false}
            />
          </div>
          <div className="text-wrapper">
            <Text isBold={true} fontWeight="600">
              {t("AccessForAllUsers")}
            </Text>
            <Text fontSize="12px">{t("AccessForAllUsersDescription")}</Text>
          </div>
        </div>

        <Text isBold={true} fontWeight="600" fontSize="16px">
          {t("AccessList")}
        </Text>

        <SearchInput
          className="search_container"
          placeholder={t("Common:Search")}
          onChange={this.onSearchChange}
          onClearSearch={this.onSearchChange}
          value={searchValue}
        />
        <PeopleSelector
          isMultiSelect={true}
          displayType="aside"
          isOpen={!!selectorIsOpen}
          onSelect={this.onSelect}
          groupsCaption={groupsCaption}
          onCancel={this.onCancelSelector}
        />

        <TabContainer elements={tabItems} />
      </MainContainer>
    );
  }
}

export default inject(({ auth, setup }) => {
  const { moduleStore } = auth;
  const { modules } = moduleStore;
  const { setAddUsers, toggleSelector, getUsersByIds } = setup;
  const { selectorIsOpen } = setup.security.accessRight;

  return {
    organizationName: auth.settingsStore.organizationName,
    modules,
    setAddUsers,
    toggleSelector,
    getUsersByIds,
    selectorIsOpen,
    groupsCaption: auth.settingsStore.customNames.groupsCaption,
  };
})(withTranslation(["Settings", "Common"])(withRouter(PeopleUsers)));
