import React from "react";
import { Row, LinkWithDropdown, ToggleButton } from "asc-web-components";
import { StyledLinkRow } from "../StyledPanels";
import { constants } from "asc-web-common";

const { ShareAceLink } = constants;

class LinkRow extends React.Component {
  constructor(props) {
    super(props);
  }

  render() {
    const {
      linkText,
      options,
      index,
      t,
      embeddedComponentRender,
      accessOptions,
      item,
      withToggle,
      onToggleLink,
    } = this.props;

    const isChecked =
      item.access !== ShareAceLink.Restrict &&
      item.access !== ShareAceLink.None;

    const isDisabled = withToggle ? !isChecked : false;

    return (
      <StyledLinkRow withToggle={withToggle} isDisabled={isDisabled}>
        <Row
          className="link-row"
          key={`${linkText}-key_${index}`}
          element={embeddedComponentRender(accessOptions, item, isDisabled)}
          contextButtonSpacerWidth="0px"
        >
          <>
            <LinkWithDropdown
              className="sharing_panel-link"
              color="#333333"
              dropdownType="alwaysDashed"
              data={options}
              fontSize="14px"
              fontWeight={600}
              isDisabled={isDisabled}
            >
              {t(linkText)}
            </LinkWithDropdown>
            {withToggle && (
              <div>
                <ToggleButton
                  isChecked={isChecked}
                  onChange={() => onToggleLink(item)}
                />
              </div>
            )}
          </>
        </Row>
      </StyledLinkRow>
    );
  }
}

export default LinkRow;
