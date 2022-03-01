import styled from "styled-components";

export const Tile = styled.div`
  display: flex;
  flex-direction: column;
  width: 100%;
  height: 220px;
  border: 1px solid #d0d5da;
  box-sizing: border-box;
  border-radius: 6px;
  align-items: center;

  .tile-icon {
    height: 96px;
    width: 96px;
    padding-bottom: 10px;
  }
`;

export const TopPanel = styled.div`
  position: relative;
  display: flex;
  width: 100%;
  height: 32px;
  align-items: center;
  padding: 8px;
  margin-left: 8px;

  .button {
    position: absolute;
    display: flex;
    align-items: center;
    justify-content: center;
    width: 32px;
    height: 32px;
    box-shadow: 0px 2px 4px rgba(4, 15, 27, 0.16);
    border-radius: 3px;
  }

  .button:hover {
    cursor: pointer;
  }

  .sharing-btn {
    position: absolute;
    right: 0;
    margin-right: 16px;
  }
`;

export const BottomPanel = styled.div`
  position: relative;
  display: flex;
  width: 100%;
  height: 64px;
  align-items: center;

  .panel-icon {
    padding: 16px;
    height: 32px;
    width: 32px;
  }

  .panel-btn {
    position: absolute;
    padding: 16px;
    right: 0;
  }
`;
