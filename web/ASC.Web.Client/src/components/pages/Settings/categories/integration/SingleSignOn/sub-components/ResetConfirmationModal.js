import React from "react";
import { inject, observer } from "mobx-react";
import { useTranslation } from "react-i18next";

import Box from "@appserver/components/box";
import Button from "@appserver/components/button";
import ModalDialog from "@appserver/components/modal-dialog";
import Text from "@appserver/components/text";

import StyledModalDialog from "../styled-containers/StyledModalDialog";

const ResetConfirmationModal = (props) => {
  const { t } = useTranslation(["SingleSignOn", "Common"]);
  const { onCloseResetModal, confirmationResetModal, onConfirmReset } = props;

  return (
    <StyledModalDialog
      contentHeight="100%"
      displayType="modal"
      onClose={onCloseResetModal}
      visible={confirmationResetModal}
    >
      <ModalDialog.Header>{t("Common:Confirmation")}</ModalDialog.Header>

      <ModalDialog.Body>
        <Text noSelect>{t("ConfirmationText")}</Text>
      </ModalDialog.Body>

      <ModalDialog.Footer>
        <Box displayProp="flex" marginProp="12px 0 4px 0">
          <Button
            className="ok-button"
            label={t("Common:OKButton")}
            onClick={onConfirmReset}
            primary
            size="small"
          />
          <Button
            label={t("Common:CancelButton")}
            onClick={onClose}
            size="small"
          />
        </Box>
      </ModalDialog.Footer>
    </StyledModalDialog>
  );
};

export default inject(({ ssoStore }) => {
  const {
    onCloseResetModal,
    confirmationResetModal,
    onConfirmReset,
  } = ssoStore;

  return { onCloseResetModal, confirmationResetModal, onConfirmReset };
})(observer(ResetConfirmationModal));
