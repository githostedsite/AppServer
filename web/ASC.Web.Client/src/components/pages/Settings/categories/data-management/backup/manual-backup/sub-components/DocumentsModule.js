import React from "react";
import { withTranslation } from "react-i18next";
import SelectFolderInput from "files/SelectFolderInput";
import Button from "@appserver/components/button";
import { getFromSessionStorage } from "../../../../../utils";
import { BackupStorageType } from "@appserver/common/constants";

let folderPath = "";
let folder = "";
class DocumentsModule extends React.Component {
  constructor(props) {
    super(props);

    folder = getFromSessionStorage("LocalCopyFolder");

    this.state = {
      isStartCopy: false,
      selectedFolder: folder || "",
      isPanelVisible: false,
    };
  }

  onSelectFolder = (folderId) => {
    this.setState({
      selectedFolder: folderId,
    });
  };

  onClickInput = () => {
    this.setState({
      isPanelVisible: true,
    });
  };

  onClose = () => {
    this.setState({
      isPanelVisible: false,
    });
  };

  onMakeCopy = async () => {
    const { onMakeCopy } = this.props;
    const { selectedFolder } = this.state;
    const { DocumentModuleType } = BackupStorageType;

    this.setState({
      isStartCopy: true,
    });

    await onMakeCopy(
      selectedFolder,
      "Documents",
      `${DocumentModuleType}`,
      "folderId"
    );

    this.setState({
      isStartCopy: false,
    });
  };

  render() {
    const { isMaxProgress, t } = this.props;
    const { isPanelVisible, isStartCopy, selectedFolder } = this.state;

    const isModuleDisabled = !isMaxProgress || isStartCopy || !selectedFolder;
    return (
      <>
        <div className="manual-backup_folder-input">
          <SelectFolderInput
            onSelectFolder={this.onSelectFolder}
            name={"common"}
            onClose={this.onClose}
            onClickInput={this.onClickInput}
            isPanelVisible={isPanelVisible}
            isDisabled={isModuleDisabled}
            foldersType="common"
            withoutProvider
            fontSizeInput={"13px"}
            id={selectedFolder}
          />
        </div>
        <div className="manual-backup_buttons">
          <Button
            label={t("Common:Duplicate")}
            onClick={this.onMakeCopy}
            primary
            isDisabled={isModuleDisabled}
            size="medium"
          />
          {!isMaxProgress && (
            <Button
              label={t("Common:CopyOperation") + "..."}
              isDisabled={true}
              size="medium"
              style={{ marginLeft: "8px" }}
            />
          )}
        </div>
      </>
    );
  }
}
export default withTranslation(["Settings", "Common"])(DocumentsModule);
