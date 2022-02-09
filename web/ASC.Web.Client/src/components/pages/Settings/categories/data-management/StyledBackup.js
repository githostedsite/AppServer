import styled, { css } from "styled-components";
import commonSettingsStyles from "../../utils/commonSettingsStyles";
import globalColors from "@appserver/components/utils/globalColors";

const linkColor = globalColors.black;

const INPUT_LENGTH = "350px";
const TEXT_LENGTH = "700px";

const commonStyles = css`
  margin-right: 16px;

  .radio-button_text {
    margin-right: 7px;
    margin-left: 4px;
    font-size: 13px;
    font-weight: 600;
  }
  .backup_radio-button {
    margin-bottom: 4px;
  }

  .backup_combo {
    margin-top: 16px;
    width: 100%;
    max-width: ${INPUT_LENGTH};
    .combo-button-label {
      width: 100%;
      max-width: ${INPUT_LENGTH};
    }
  }
  .backup_text-input {
    margin: 10px 0;
    width: 100%;
    max-width: ${INPUT_LENGTH};
    font-size: 13px;
  }

  .backup-description {
    max-width: ${TEXT_LENGTH};
    font-size: 12px;
    line-height: 15px;
    margin-left: 24px;
  }
  .select-folder_file-input {
    margin-left: 24px;
    margin-top: 16px !important;
  }
`;

const StyledManualBackup = styled.div`
  ${commonStyles}
  .manual-backup_buttons {
    margin-top: 16px;
    margin-left: 24px;
  }

  .manual-backup_storages-module {
    margin-left: 24px;
    .manual-backup_buttons {
      margin-left: 0px;
    }
  }
`;

const StyledAutoBackup = styled.div`
  ${commonStyles}

  .automatic-backup_main {
    margin-bottom: 30px;
    .radio-button_text {
      font-size: 13px;
    }
  }
  .backup_toggle-btn {
    position: static;
  }
  .backup_toggle-btn-description {
    margin-left: 37px;
    max-width: 1024px;
  }
  .toggle-button-text {
    font-weight: 600;
    margin-bottom: 4px;
  }

  .input-with-folder-path {
    margin-top: 16px;
    margin-bottom: 8px;
    width: 100%;
    max-width: ${INPUT_LENGTH};
  }
  .save-button {
    margin-right: 8px;
  }
  .backup_modules {
    margin-left: 37px;
  }
  .auto-backup_storages-module {
    margin-left: 24px;
    .backup_schedule-component {
      margin-left: 0;
    }
  }

  .backup_toggle-wrapper {
    margin-bottom: 16px;
    background-color: #f8f9f9;
    padding: 12px;
    max-width: 1144px;
  }
`;

const StyledStoragesModule = styled.div`
  .backup_storages-buttons {
    margin-left: -63px;
    margin-top: 40px;
  }
`;
const StyledRestoreBackup = styled.div`
  ${commonStyles}

  .restore-description {
    max-width: ${TEXT_LENGTH};
    font-size: 12px;
    line-height: 15px;
  }

  .restore-source {
    font-weight: 600;
    margin-top: 30px;
    margin-bottom: 15px;
    font-size: 15px;
  }
  .restore-warning {
    font-weight: 600;
    margin-top: 18px;
    margin-bottom: 15px;
    font-size: 16px;
    color: #f21c0e;
  }
  .restore-warning_link {
    margin: 15px 0;
  }
  .restore-backup-checkbox {
    margin-bottom: 24px;
  }
  .restore-backup-checkbox_notification {
    margin-top: 15px;
    max-width: ${TEXT_LENGTH};
    .checkbox-text {
      white-space: normal;
    }
  }
  .restore-backup_list {
    color: ${linkColor};
    text-decoration: underline dotted;
    cursor: pointer;
    font-weight: 600;
  }

  .restore-backup_input {
    margin: 16px 0;
    max-width: ${INPUT_LENGTH};
  }
  .restore-description {
    margin-bottom: 24px;
  }

  .restore-backup_modules {
    margin-top: 24px;
  }

  .backup_radio-button {
    margin-bottom: 16px;
  }
`;

const StyledModules = styled.div`
  margin-bottom: 24px;

  .backup-description {
    ${(props) => props.isDisabled && `color: #A3A9AE`}
  }
`;

const StyledScheduleComponent = styled.div`
  margin-left: 24px;
  .additional_options {
    max-width: ${INPUT_LENGTH};
    display: grid;
    grid-template-columns: ${(props) =>
      props.weeklySchedule || props.monthlySchedule ? "1fr 1fr" : "1fr"};
    grid-gap: 8px;
  }

  .weekly_option,
  .month_options {
    max-width: 171px;
    width: 100%;
    .drop-down_variable-size {
      max-width: 171px !important;
      width: 100% !important;
    }
  }
  .schedule-backup_combobox {
    display: inline-block;
    margin-top: 8px;
  }

  .main_options {
    max-width: 363px;
  }
  .days_option,
  .time_options {
    max-width: ${INPUT_LENGTH};
    width: 100%;
    .drop-down_variable-size {
      max-width: ${INPUT_LENGTH} !important;
      width: 100% !important;
    }
  }

  .max_copies {
    width: 100%;
    max-width: ${INPUT_LENGTH};
    .drop-down_variable-size {
      max-width: ${INPUT_LENGTH};
      width: 100% !important;
    }
  }
  .combo-button {
    width: 100% !important;
  }
  .combo-button-label {
    max-width: 100% !important;
    font-weight: 400;
  }
  .schedule_description {
    font-weight: 600;
  }
`;

const StyledBackup = styled.div`
  .schedule-information {
    font-size: 13px;
    font-weight: 600;
    margin-bottom: 8px;
  }
  ${commonSettingsStyles}
`;

const StyledBackupList = styled.div`
  height: 100%;
  .loader {
    height: ${(props) => (props.height <= 187 ? `${props.height}px` : "187px")};
  }
  .backup-list_modal-header_wrapper_description {
    margin-bottom: 16px;
  }
  .backup-list-row-list {
    height: ${(props) =>
      props.displayType === "aside"
        ? "100vh"
        : props.height <= 187
        ? `${props.height}px`
        : "187px"};
    width: 100%;
  }
  .backup-list_modal-dialog_body {
    min-height: 94px;
    max-height: 294px;
    //height: 210px;
  }
  .restore_dialog-button {
    ${(props) =>
      props.displayType === "aside" &&
      css`
        width: 293px;
      `}
  }
  .backup-list_options {
    display: flex;
  }

  .backup-list_trash-icon {
    width: 16px;
    height: 16px;
  }
  .backup-list_aside-header_title {
    margin: 0px;
    line-height: 56px;
    max-width: 474px;
    width: 400px;
    white-space: nowrap;
    text-overflow: ellipsis;
    overflow: hidden;
    padding-left: 16px;
    border-bottom: 1px solid #eceef1;
    margin-bottom: 16px;
  }
  .backup-list_aside-header {
    margin-bottom: 16px;
  }
  .backup-list_aside-header_description {
    //margin-top: 16px;
  }
  .backup-list_modal-header_description {
    //margin-bottom: 16px;
  }
  .backup-list_modal-header_description,
  .backup-list_aside-header_description {
    margin-bottom: 4px;
    /* display: flex;
    display: contents;
    overflow-wrap: break-word; */
  }
  .backup-list_clear-link {
    text-decoration: underline dotted;
  }
  .backup-list_aside-body_wrapper {
    height: calc(100% - 200px);
    width: 300px;
    padding: 0 16px;
  }
  .backup-list_aside_body {
    margin-top: 16px;
    height: 100%;
    width: 290px;
  }
  .backup-list_restore-link {
    font-size: 12px;
    margin-right: 16px;
    color: #a3a9ae;
    max-width: 200px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    ${(props) =>
      props.isCopyingToLocal &&
      "pointer-events: none; touch-action: none; color: #d0d5da"}
  }

  .backup-list_restore-link:hover {
    text-decoration: none;
    font-weight: 600;
    color: #657077;
  }
  .backup-list_trash-icon:hover {
    path {
      fill: #657077;
    }
  }
  .backup-list_trash-icon,
  .backup-list_restore-link {
    cursor: pointer;
    text-decoration: underline;
  }
  .backup-list_trash-icon {
    margin-top: 1px;
  }
  .restore_context-options {
    margin-top: 16px;
    ${(props) =>
      props.isCopyingToLocal && "pointer-events: none; touch-action: none; "}

    .icon-button_svg {
      svg {
        path {
          ${(props) =>
            props.isCopyingToLocal &&
            `
              fill: #d0d5da;
            `};
        }
      }
    }
  }

  .backup-list_icon {
    grid-area: icon-name;
  }

  .backup-list_entry-title {
    font-weight: 600;

    ${(props) =>
      props.displayType === "aside" &&
      css`
        max-width: 206px;
      `}
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
  .backup-list_file-exst {
    color: #a3a9ae;
    grid-area: ext;
  }

  .backup-list_full-name {
    grid-area: full-name;

    display: flex;
    ${(props) =>
      props.displayType === "aside" &&
      css`
        padding-top: 4px;
      `}
  }
  .backup-list_children {
    grid-area: children;
    margin-right: 16px;
    ${(props) =>
      props.displayType === "aside" &&
      css`
        margin-top: -17px;
      `}
  }
  .backup-list_file-name {
    border-radius: 3px;

    cursor: default;
    border-bottom: 1px solid #eceef1;
    align-items: center;
    display: grid;

    ${(props) =>
      props.displayType === "aside"
        ? css`
            height: 40px;
            grid-template-areas: "icon-name full-name ext children";
          `
        : css`
            height: 40px;
            grid-template-areas: "icon-name full-name children";
          `}
    grid-template-columns: 32px 1fr;
  }
`;
export {
  StyledModules,
  StyledRestoreBackup,
  StyledScheduleComponent,
  StyledBackup,
  StyledBackupList,
  StyledManualBackup,
  StyledAutoBackup,
  StyledStoragesModule,
};
