import React from "react";
import { Consumer } from "@appserver/components/utils/context";
import { inject, observer } from "mobx-react";
import { withTranslation } from "react-i18next";
import Headline from "@appserver/common/components/Headline";
import { withRouter } from "react-router";
import styled, { css } from "styled-components";
import { tablet, desktop } from "@appserver/components/utils/device";
import { isMobile } from "react-device-detect";
import ContextMenuButton from "@appserver/components/context-menu-button";
import DropDownItem from "@appserver/components/drop-down-item";
import GroupButtonsMenu from "@appserver/components/group-buttons-menu";
import api from "@appserver/common/api";
const { ProjectsFilter, TasksFilter } = api;

const StyledContainer = styled.div`
  .header-container {
    position: relative;
    ${(props) =>
      props.title &&
      css`
        display: grid;
        grid-template-columns: ${(props) =>
          props.isRootFolder
            ? "auto auto 1fr"
            : props.canCreate
            ? "auto auto auto auto 1fr"
            : "auto auto auto 1fr"};

        @media ${tablet} {
          grid-template-columns: ${(props) =>
            props.isRootFolder
              ? "1fr auto"
              : props.canCreate
              ? "auto 1fr auto auto"
              : "auto 1fr auto"};
        }
      `}
    align-items: center;
    max-width: calc(100vw - 32px);

    @media ${tablet} {
      .headline-header {
        margin-left: -1px;
      }
    }
    .arrow-button {
      margin-right: 15px;
      min-width: 17px;

      @media ${tablet} {
        padding: 8px 0 8px 8px;
        margin-left: -8px;
        margin-right: 16px;
      }
    }

    .add-button {
      margin-bottom: -1px;
      margin-left: 16px;

      @media ${tablet} {
        margin-left: auto;

        & > div:first-child {
          padding: 8px 8px 8px 8px;
          margin-right: -8px;
        }
      }
    }

    .option-button {
      margin-bottom: -1px;

      @media (min-width: 1024px) {
        margin-left: 8px;
      }

      @media ${tablet} {
        & > div:first-child {
          padding: 8px 8px 8px 8px;
          margin-right: -8px;
        }
      }
    }
  }

  .group-button-menu-container {
    margin: 0 -16px;
    -webkit-tap-highlight-color: rgba(0, 0, 0, 0);
    padding-bottom: 56px;

    ${isMobile &&
    css`
      position: sticky;
    `}

    ${(props) =>
      !props.isTabletView
        ? props.width &&
          isMobile &&
          css`
            width: ${props.width + 40 + "px"};
          `
        : props.width &&
          isMobile &&
          css`
            width: ${props.width + 32 + "px"};
          `}

    @media ${tablet} {
      padding-bottom: 0;
      ${!isMobile &&
      css`
        height: 56px;
      `}
      & > div:first-child {
        ${(props) =>
          !isMobile &&
          props.width &&
          css`
            width: ${props.width + 16 + "px"};
          `}

        position: absolute;
        ${(props) =>
          !props.isDesktop &&
          css`
            top: 56px;
          `}
        z-index: 180;
      }
    }

    @media ${desktop} {
      margin: 0 -24px;
    }
  }
`;

const PureSectionHeaderContent = (props) => {
  const {
    setSelected,
    t,
    isHeaderChecked,
    isHeaderVisible,
    isHeaderIndeterminate,
    filter,
  } = props;

  const onCheck = (checked) => {
    setSelected(checked ? "all" : "none");
  };

  const onSelect = (item) => {
    setSelected(item.key);
  };

  const onClose = () => {
    setSelected("close");
  };

  const getChildren = () => {
    if (filter instanceof ProjectsFilter) {
      return [
        <DropDownItem key="active" label={t("Active")} data-index={0} />,
        <DropDownItem key="paused" label={t("Paused")} data-index={1} />,
        <DropDownItem key="closed" label={t("Closed")} data-index={2} />,
      ];
    }

    if (filter instanceof TasksFilter) {
      return [
        <DropDownItem key="active" label={t("TaskOpen")} data-index={0} />,
        <DropDownItem key="closed" label={t("TaskClosed")} data-index={1} />,
      ];
    }
  };

  const getItems = () => {
    if (filter instanceof ProjectsFilter) {
      return [
        {
          label: t("Common:Delete"),
          disabled: true,
        },
      ];
    }

    if (filter instanceof TasksFilter) {
      return [
        {
          label: t("Close"),
          disabled: true,
        },
        {
          label: t("Move"),
          disabled: true,
        },
        {
          label: t("Common:Delete"),
          disabled: true,
        },
      ];
    }
  };

  const getMenuItems = () => {
    let menu = [
      {
        label: t("Common:Select"),
        isDropdown: true,
        isSeparator: true,
        isSelect: true,
        fontWeight: "bold",
        children: getChildren(),
        onSelect: onSelect,
      },
      ...getItems(),
    ];

    return menu;
  };

  const menuItems = getMenuItems();

  const getContextOptionsPlus = () => {
    const { t } = props;
    return [
      {
        key: "new-project",
        label: t("Article:NewProject"),
      },
      {
        key: "new-milestone",
        label: t("Article:NewMilestone"),
      },
      {
        key: "new-task",
        label: t("Article:NewTask"),
      },
      {
        key: "new-discussion",
        label: t("Article:NewDiscussion"),
      },
    ];
  };

  return (
    <Consumer>
      {(context) => (
        <StyledContainer width={context.sectionWidth} title={"test"}>
          {isHeaderVisible ? (
            <div className="group-button-menu-container">
              <GroupButtonsMenu
                checked={isHeaderChecked}
                isIndeterminate={isHeaderIndeterminate}
                onChange={onCheck}
                onClose={onClose}
                menuItems={menuItems}
                selected={menuItems[0].label}
                moreLabel={t("Common:More")}
                closeTitle={t("Common:CloseButton")}
                sectionWidth={context.sectionWidth}
              />
            </div>
          ) : (
            <div className="header-container">
              <Headline
                className="headline-header"
                type="content"
                truncate={true}
              >
                title
              </Headline>
              <ContextMenuButton
                className="add-button"
                directionX="right"
                iconName="images/plus.svg"
                size={17}
                color="#A3A9AE"
                hoverColor="#657077"
                isFill
                getData={getContextOptionsPlus}
                isDisabled={false}
              />
            </div>
          )}
        </StyledContainer>
      )}
    </Consumer>
  );
};

const SectionHeaderContent = withTranslation(["Home", "Article", "Common"])(
  withRouter(PureSectionHeaderContent)
);

export default inject(({ auth, projectsStore }) => {
  const {
    setSelected,
    isHeaderVisible,
    isHeaderChecked,
    isHeaderIndeterminate,
    filter,
  } = projectsStore;
  return {
    auth,
    projectsStore,
    setSelected,
    isHeaderVisible,
    isHeaderChecked,
    isHeaderIndeterminate,
    filter,
  };
})(observer(SectionHeaderContent));
