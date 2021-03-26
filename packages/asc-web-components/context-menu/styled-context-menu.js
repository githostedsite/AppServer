import styled from "styled-components";
import Base from "../themes/base";

const StyledContextMenu = styled.div`
  .p-contextmenu {
    position: absolute;
  }

  .p-contextmenu ul {
    margin: 0;
    padding: 0;
    list-style: none;
  }

  .p-contextmenu .p-submenu-list {
    position: absolute;
    min-width: 100%;
    z-index: 1;
  }

  .p-contextmenu .p-menuitem-link {
    cursor: pointer;
    display: flex;
    align-items: center;
    text-decoration: none;
    overflow: hidden;
    position: relative;
  }

  .p-contextmenu .p-menuitem-text {
    line-height: 1;
  }

  .p-contextmenu .p-menuitem {
    position: relative;
  }

  .p-contextmenu .p-menuitem-link .p-submenu-icon {
    margin-left: auto;
  }

  .p-contextmenu-enter {
    opacity: 0;
  }

  .p-contextmenu-enter-active {
    opacity: 1;
    transition: opacity 250ms;
  }
`;

StyledComboBox.defaultProps = {
  theme: Base,
};

export default StyledContextMenu;
