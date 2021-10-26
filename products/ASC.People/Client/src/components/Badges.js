import React from "react";
import styled from "styled-components";
import commonIconsStyles from "@appserver/components/utils/common-icons-style";
import SendClockIcon from "../../public/images/send.clock.react.svg";
import CatalogSpamIcon from "../../public/images/catalog.spam.react.svg";

const StyledSendClockIcon = styled(SendClockIcon)`
  ${commonIconsStyles}
  path {
    fill: #3b72a7;
  }
`;
const StyledCatalogSpamIcon = styled(CatalogSpamIcon)`
  ${commonIconsStyles}
  path {
    fill: #3b72a7;
  }
`;

const Badges = ({ statusType }) => {
  return (
    <>
      {statusType === "pending" && <StyledSendClockIcon size="small" />}
      {statusType === "disabled" && <StyledCatalogSpamIcon size="small" />}
    </>
  );
};

export default Badges;
