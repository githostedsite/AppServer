import React from "react";
import { withRouter } from "react-router";
import TableRow from "@appserver/components/table-container/TableRow";
import TableCell from "@appserver/components/table-container/TableCell";
import withContextOptions from "../../../../../HOCs/withContextOptions";
import withContent from "../../../../../HOCs/withContent";
import Link from "@appserver/components/link";
import Text from "@appserver/components/text";
import styled from "styled-components";
import Checkbox from "@appserver/components/checkbox";
import withBadges from "../../../../../HOCs/withBadges";

const StyledPeopleRow = styled(TableRow)`
  .table-container_cell {
    height: 46px;
    max-height: 46px;
  }

  .table-container_row-checkbox-wrapper {
    padding-left: 4px;
    min-width: 46px;

    .table-container_row-checkbox {
      margin-left: 8px;
    }
  }

  .people-badges {
    height: 22px;
    align-self: center;
    white-space: nowrap;
    margin-left: 8px;
  }
`;

const PeopleTableRow = (props) => {
  const {
    item,
    contextOptionsProps,
    element,
    checkedProps,
    onContentRowSelect,
    groups,
    onEmailClick,
    onUserNameClick,
    isAdmin,
    badgesComponent,
  } = props;
  const { displayName, email, role, statusType, userName } = item;

  const nameColor = statusType === "pending" ? "#A3A9AE" : "#333333";
  const sideInfoColor = statusType === "pending" ? "#D0D5DA" : "#A3A9AE";

  const onChange = (e) => {
    onContentRowSelect && onContentRowSelect(e.target.checked, item);
  };

  return (
    <StyledPeopleRow key={item.id} {...contextOptionsProps}>
      <TableCell>
        <TableCell
          hasAccess={isAdmin}
          className="table-container_row-checkbox-wrapper"
          checked={checkedProps.checked}
        >
          <div className="table-container_element">{element}</div>
          <Checkbox
            className="table-container_row-checkbox"
            onChange={onChange}
            isChecked={checkedProps.checked}
          />
        </TableCell>

        <Link
          type="page"
          title={displayName}
          fontWeight="600"
          fontSize="15px"
          color={nameColor}
          isTextOverflow
          href={`/products/people/view/${userName}`}
          onClick={onUserNameClick}
        >
          {displayName}
        </Link>
        <div className="people-badges">{badgesComponent}</div>
      </TableCell>
      <TableCell>{groups}</TableCell>
      <TableCell>
        <Text
          type="page"
          title={role}
          fontSize="12px"
          fontWeight={400}
          color={sideInfoColor}
          truncate
        >
          {role}
        </Text>
      </TableCell>
      <TableCell>
        <Text
          style={{ display: "none" }} //TODO:
          type="page"
          title={role}
          fontSize="12px"
          fontWeight={400}
          color={sideInfoColor}
          truncate
        >
          Phone
        </Text>
      </TableCell>
      <TableCell>
        <Link
          type="page"
          title={email}
          fontSize="12px"
          fontWeight={400}
          color={sideInfoColor}
          onClick={onEmailClick}
          isTextOverflow
        >
          {email}
        </Link>
      </TableCell>
    </StyledPeopleRow>
  );
};

export default withRouter(
  withContextOptions(withContent(withBadges(PeopleTableRow)))
);
