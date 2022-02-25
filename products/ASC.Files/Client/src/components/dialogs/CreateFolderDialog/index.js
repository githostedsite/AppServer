import React from "react";
import { withRouter } from "react-router";
import ModalDialogContainer from "../ModalDialogContainer";
import TextInput from "@appserver/components/text-input";
import Button from "@appserver/components/button";
import ModalDialog from "@appserver/components/modal-dialog";
import { withTranslation } from "react-i18next";
import { inject, observer } from "mobx-react";

const CreateFolderDialogComponent = (props) => {
  const { visible, t, tReady, isLoading, setCreateFolderDialogVisible } = props;

  const onClose = () => setCreateFolderDialogVisible(false);

  const onCreate = () => {
    onClose();
  };

  return (
    <ModalDialogContainer
      isLoading={!tReady}
      visible={visible}
      onClose={onClose}
    >
      <ModalDialog.Header>{t("Home:NewFolder")}</ModalDialog.Header>
      <ModalDialog.Body>
        <TextInput
          scale={true}
          id="folder-name"
          name="folder-name"
          onBlur={function noRefCheck() {}}
          onChange={function noRefCheck() {}}
          onFocus={function noRefCheck() {}}
          placeholder={t("Home:NewFolder")}
          tabIndex={1}
        />
      </ModalDialog.Body>
      <ModalDialog.Footer>
        <Button
          key="OkButton"
          label={t("Common:Create")}
          size="medium"
          primary
          onClick={onCreate}
          isLoading={isLoading}
        />
        <Button
          className="button-dialog"
          key="CancelButton"
          label={t("Common:CancelButton")}
          size="medium"
          onClick={onClose}
          isLoading={isLoading}
        />
      </ModalDialog.Footer>
    </ModalDialogContainer>
  );
};

const CreateFolderDialog = withTranslation(["Common", "Translations", "Home"])(
  CreateFolderDialogComponent
);

export default inject(({ filesStore, dialogsStore }) => {
  const { fetchFiles, filter, isLoading } = filesStore;

  const {
    createFolderDialogVisible: visible,
    setCreateFolderDialogVisible,
  } = dialogsStore;

  return {
    isLoading,
    filter,
    visible,
    setCreateFolderDialogVisible,
    fetchFiles,
  };
})(withRouter(observer(CreateFolderDialog)));