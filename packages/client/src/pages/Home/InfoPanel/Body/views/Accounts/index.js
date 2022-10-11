import React from "react";
import { withTranslation } from "react-i18next";

import withLoader from "@docspace/client/src/HOCs/withLoader";
import Loaders from "@docspace/common/components/Loaders";

import Text from "@docspace/components/text";
import ComboBox from "@docspace/components/combobox";

import { getUserStatus } from "SRC_DIR/helpers/people-helpers";
import { StyledAccountContent } from "../../styles/accounts";

const Accounts = ({
  t,
  selection,
  isOwner,
  isAdmin,
  changeUserType,
  selfId,
}) => {
  const [statusLabel, setStatusLabel] = React.useState("");

  const { role, id } = selection;

  React.useEffect(() => {
    getStatusLabel();
  }, [selection, getStatusLabel]);

  const getStatusLabel = React.useCallback(() => {
    const status = getUserStatus(selection);
    switch (status) {
      case "active":
        return setStatusLabel(t("Common:Active"));
      case "pending":
        return setStatusLabel(t("PeopleTranslations:PendingTitle"));
      case "disabled":
        return setStatusLabel(t("Settings:Disabled"));
      default:
        return setStatusLabel(t("Common:Active"));
    }
  }, [selection]);

  const getUserTypeLabel = React.useCallback((role) => {
    switch (role) {
      case "owner":
        return t("Common:Owner");
      case "admin":
        return t("People:DocSpaceAdmin");
      case "manager":
        return t("People:RoomAdmin");
      case "user":
        return t("Common:User");
    }
  }, []);

  const getTypesOptions = React.useCallback(() => {
    const options = [];

    const adminOption = {
      key: "admin",
      title: t("People:DocSpaceAdmin"),
      label: t("People:DocSpaceAdmin"),
      action: "admin",
    };
    const managerOption = {
      key: "manager",
      title: t("People:RoomAdmin"),
      label: t("People:RoomAdmin"),
      action: "manager",
    };
    const userOption = {
      key: "user",
      title: t("Common:User"),
      label: t("Common:User"),
      action: "user",
    };

    isOwner && options.push(adminOption);

    isAdmin && options.push(managerOption);

    // TODO: add check on manager type
    options.push(userOption);

    return options;
  }, [t, isAdmin, isOwner]);

  const onTypeChange = React.useCallback(
    ({ action }) => {
      changeUserType(action, [selection], t, false);
    },
    [selection, changeUserType, t]
  );

  const typeLabel = getUserTypeLabel(role);

  const renderTypeData = () => {
    const typesOptions = getTypesOptions();

    const combobox = (
      <ComboBox
        className="type-combobox"
        selectedOption={
          typesOptions.find((option) => option.key === role) || {}
        }
        options={typesOptions}
        onSelect={onTypeChange}
        scaled={false}
        size="content"
        displaySelectedOption
        modernView
      />
    );

    const text = (
      <Text
        type="page"
        title={typeLabel}
        fontSize="13px"
        fontWeight={600}
        truncate
        noSelect
      >
        {typeLabel}
      </Text>
    );

    if (selfId === id) return text;

    switch (role) {
      case "owner":
        return text;

      case "admin":
      case "manager":
        if (isOwner) {
          return combobox;
        } else {
          return text;
        }

      case "user":
        if (isOwner || isAdmin) {
          return combobox;
        } else {
          return text;
        }

      default:
        return text;
    }
  };

  const typeData = renderTypeData();

  return (
    <>
      <StyledAccountContent>
        <div className="data__header">
          <Text className={"header__text"} noSelect title={t("Data")}>
            {t("InfoPanel:Data")}
          </Text>
        </div>
        <div className="data__body">
          <Text className={"info_field first-row"} noSelect title={t("Data")}>
            {t("ConnectDialog:Account")}
          </Text>
          <Text
            className={"info_data first-row"}
            fontSize={"13px"}
            fontWeight={600}
            noSelect
            title={statusLabel}
          >
            {statusLabel}
          </Text>

          <Text className={"info_field"} noSelect title={t("Common:Type")}>
            {t("Common:Type")}
          </Text>
          {typeData}

          <Text className={"info_field"} noSelect title={t("UserStatus")}>
            {t("UserStatus")}
          </Text>
          <Text
            className={"info_data first-row"}
            fontSize={"13px"}
            fontWeight={600}
            noSelect
            title={statusLabel}
          >
            {t("SmartBanner:Price")}
          </Text>
          {/* <Text className={"info_field"} noSelect title={t("Common:Room")}>
            {t("Common:Room")}
          </Text>
          <div>Rooms list</div> */}
        </div>
      </StyledAccountContent>
    </>
  );
};

export default withTranslation([
  "People",
  "InfoPanel",
  "ConnectDialog",
  "Common",
  "PeopleTranslations",
  "People",
  "Settings",
  "SmartBanner",
  "DeleteProfileEverDialog",
  "Translations",
])(withLoader(Accounts)(<Loaders.InfoPanelViewLoader view="accounts" />));
