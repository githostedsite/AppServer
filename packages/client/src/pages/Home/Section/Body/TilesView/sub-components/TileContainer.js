import React from "react";
import { inject, observer } from "mobx-react";
import { withTranslation } from "react-i18next";
import styled, { css } from "styled-components";
import PropTypes from "prop-types";
import Heading from "@docspace/components/heading";
import ContextMenu from "@docspace/components/context-menu";
import CustomScrollbarsVirtualList from "@docspace/components/scrollbar";
import { isMobile } from "react-device-detect";
import {
  tablet,
  desktop,
  isTablet,
  isMobile as isMobileUtils,
} from "@docspace/components/utils/device";

import { Base } from "@docspace/components/themes";
import InfiniteGrid from "./InfiniteGrid";

const paddingCss = css`
  @media ${desktop} {
    margin-left: 1px;
    padding-right: 0px;
  }

  @media ${tablet} {
    margin-left: -1px;
  }
`;

const StyledGridWrapper = styled.div`
  display: grid;
  grid-template-columns: ${(props) =>
    props.isRooms
      ? "repeat(auto-fill, minmax(274px, 1fr))"
      : "repeat(auto-fill, minmax(216px, 1fr))"};
  width: 100%;
  margin-bottom: ${(props) => (props.isFolders || props.isRooms ? "23px" : 0)};
  box-sizing: border-box;
  ${paddingCss};

  grid-gap: 14px 16px;

  @media ${tablet} {
    grid-gap: 14px;
  }

  .tiles-loader {
    padding-top: 14px;

    &:nth-of-type(n + 3) {
      display: block;
    }
  }
`;

const StyledTileContainer = styled.div`
  position: relative;
  height: 100%;

  .tile-item-wrapper {
    position: relative;
    width: 100%;

    &.file {
      padding: 0;
    }
    &.folder {
      padding: 0;
    }
  }

  .tile-items-heading {
    margin: 0;
    margin-bottom: 15px;

    display: flex;
    align-items: center;
    justify-content: space-between;

    div {
      cursor: pointer !important;

      .sort-combo-box {
        margin-right: 3px;
        .dropdown-container {
          top: 104%;
          bottom: auto;
          min-width: 200px;
          margin-top: 3px;

          .option-item {
            display: flex;
            align-items: center;
            justify-content: space-between;

            min-width: 200px;

            svg {
              width: 16px;
              height: 16px;
            }

            .option-item__icon {
              display: none;
              cursor: pointer;
              ${(props) =>
                props.isDesc &&
                css`
                  transform: rotate(180deg);
                `}

              path {
                fill: ${(props) => props.theme.filterInput.sort.sortFill};
              }
            }

            :hover {
              .option-item__icon {
                display: flex;
              }
            }
          }

          .selected-option-item {
            background: ${(props) =>
              props.theme.filterInput.sort.hoverBackground};
            cursor: auto;

            .selected-option-item__icon {
              display: flex;
            }
          }
        }

        .optionalBlock {
          display: flex;
          flex-direction: row;
          align-items: center;

          font-size: 12px;
          font-weight: 600;

          color: ${(props) => props.theme.filterInput.sort.tileSortColor};

          .sort-icon {
            margin-right: 8px;
            svg {
              path {
                fill: ${(props) => props.theme.filterInput.sort.tileSortFill};
              }
            }
          }
        }

        .combo-buttons_arrow-icon {
          display: none;
        }
      }
    }
  }

  @media ${tablet} {
    margin-right: -3px;
  }
`;

StyledTileContainer.defaultProps = { theme: Base };

class TileContainer extends React.PureComponent {
  constructor(props) {
    super(props);

    this.state = {
      selectedFilterData: {
        sortId: props.filter.sortBy,
        sortDirection: props.filter.sortOrder,
      },
    };
  }

  render() {
    const {
      children,
      useReactWindow,
      id,
      className,
      style,
      headingFolders,
      headingFiles,
    } = this.props;

    const { selectedFilterData } = this.state;

    const Rooms = [];
    const Folders = [];
    const Files = [];

    React.Children.map(children, (item) => {
      const { isFolder, isRoom, fileExst, id } = item.props.item;
      if ((isFolder || id === -1) && !fileExst && !isRoom) {
        Folders.push(
          <div className="tile-item-wrapper folder" key={id}>
            {item}
          </div>
        );
      } else if (isRoom) {
        Rooms.push(
          <div
            className="tile-item-wrapper folder"
            key={index}
            onContextMenu={this.onRowContextClick.bind(
              this,
              item.props.contextOptions
            )}
          >
            {item}
          </div>
        );
      } else {
        Files.push(
          <div className="tile-item-wrapper file" key={id}>
            {item}
          </div>
        );
      }
    });

    const renderTile = (
      <>
        {Rooms.length > 0 && (
          <Heading
            size="xsmall"
            id={"room-tile-heading"}
            className="tile-items-heading"
          >
            {"Rooms"}
          </Heading>
        )}

        {Folders.length > 0 ? (
          useReactWindow ? (
            Rooms
          ) : (
            <StyledGridWrapper isRooms>{Rooms}</StyledGridWrapper>
          )
        ) : null}

        {Folders.length > 0 && (
          <Heading
            size="xsmall"
            id={"folder-tile-heading"}
            className="tile-items-heading"
          >
            {headingFolders}
          </Heading>
        )}
        {Folders.length > 0 ? (
          useReactWindow ? (
            Folders
          ) : (
            <StyledGridWrapper isFolders>{Folders}</StyledGridWrapper>
          )
        ) : null}

        {Files.length > 0 && (
          <Heading size="xsmall" className="tile-items-heading">
            {headingFiles}
          </Heading>
        )}
        {Files.length > 0 ? (
          useReactWindow ? (
            Files
          ) : (
            <StyledGridWrapper>{Files}</StyledGridWrapper>
          )
        ) : null}
      </>
    );

    return (
      <StyledTileContainer
        id={id}
        className={className}
        style={style}
        useReactWindow={useReactWindow}
        isDesc={selectedFilterData.sortDirection === "desc"}
      >
        {useReactWindow ? (
          <InfiniteGrid>{renderTile}</InfiniteGrid>
        ) : (
          renderTile
        )}
      </StyledTileContainer>
    );
  }
}

TileContainer.propTypes = {
  children: PropTypes.any.isRequired,
  useReactWindow: PropTypes.bool,
  className: PropTypes.string,
  id: PropTypes.string,
  style: PropTypes.oneOfType([PropTypes.object, PropTypes.array]),
};

TileContainer.defaultProps = {
  id: "tileContainer",
};

export default inject(
  ({ auth, filesStore, treeFoldersStore, selectedFolderStore }) => {
    const { personal } = auth.settingsStore;
    const { fetchFiles, filter, setIsLoading } = filesStore;
    const { isFavoritesFolder, isRecentFolder } = treeFoldersStore;

    return {
      personal,
      fetchFiles,
      filter,
      setIsLoading,
      isFavoritesFolder,
      isRecentFolder,
      selectedFolderId: selectedFolderStore.id,
    };
  }
)(observer(withTranslation(["Files", "Common"])(TileContainer)));
