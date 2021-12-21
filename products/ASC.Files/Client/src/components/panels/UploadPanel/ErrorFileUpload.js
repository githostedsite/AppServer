import PasswordInput from "./PasswordInput";
import Tooltip from "@appserver/components/tooltip";
import LoadErrorIcon from "../../../../public/images/load.error.react.svg";
import styled from "styled-components";
import Text from "@appserver/components/text";
import React from "react";

const StyledLoadErrorIcon = styled(LoadErrorIcon)`
  outline: none !important;
`;

const ErrorFileUpload = ({ item, onTextClick }) => (
  <>
    <div className="upload_panel-icon">
      {item.needPassword && (
        <Text
          className="enter-password"
          fontWeight="600"
          color="#A3A9AE"
          onClick={onTextClick}
        >
          {"Enter Password"}
        </Text>
      )}
      <StyledLoadErrorIcon
        size="medium"
        data-for="errorTooltip"
        data-tip={item.error || t("Common:UnknownError")}
      />
      <Tooltip
        id="errorTooltip"
        offsetTop={0}
        getContent={(dataTip) => (
          <Text fontSize="13px" noSelect>
            {dataTip}
          </Text>
        )}
        effect="float"
        place="left"
        maxWidth={320}
        color="#f8f7bf"
      />
    </div>
  </>
);
export default ErrorFileUpload;
