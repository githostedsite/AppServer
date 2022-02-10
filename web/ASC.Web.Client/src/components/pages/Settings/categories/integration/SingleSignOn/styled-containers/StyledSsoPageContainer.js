import styled from "styled-components";

const StyledSsoPage = styled.div`
  box-sizing: border-box;
  outline: none;
  width: 100%;

  .toggle {
    position: static;
  }

  .tooltip-button,
  .icon-button {
    padding: 0 5px;
  }

  .hide-button {
    margin-left: 16px;
  }

  .hide-additional-button {
    margin-left: 8px;
  }

  .field-label-icon {
    align-items: center;
    margin-bottom: 4px;
    max-width: 350px;
  }

  .field-label {
    height: auto;
    font-weight: 600;
    line-height: 20px;
    overflow: visible;
    white-space: normal;
  }

  .xml-input {
    .field-label-icon {
      margin-bottom: 8px;
      max-width: 350px;
    }

    .field-label {
      font-weight: 400;
    }
  }

  .or-text {
    margin: 0 24px;
  }

  .radio-button-group {
    margin-left: 25px;
  }

  .combo-button-label {
    max-width: 100%;
  }

  .checkbox-input {
    margin: 6px 8px 6px 0;
  }

  .upload-button {
    margin-left: 9px;
    overflow: inherit;

    & > div {
      margin-top: 2px;
    }
  }

  .save-button {
    margin-right: 8px;
  }

  .download-button {
    max-width: 404px;
    width: 100%;
  }
`;

export default StyledSsoPage;