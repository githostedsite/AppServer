import React from "react";
import { withTranslation } from "react-i18next";
import { inject, observer } from "mobx-react";
import AmazonSettings from "../../../consumer-storage-settings/AmazonSettings";
import ScheduleComponent from "../ScheduleComponent";
import { StyledStoragesModule } from "../../../StyledBackup";

class AmazonStorage extends React.Component {
  constructor(props) {
    super(props);
    const { selectedStorage, setCompletedFormFields } = this.props;

    setCompletedFormFields(AmazonSettings.formNames(), "s3");

    this.isDisabled = !selectedStorage?.isSet;
  }

  render() {
    const { t, isLoadingData, selectedStorage, ...rest } = this.props;

    return (
      <StyledStoragesModule>
        <AmazonSettings
          isLoadingData={isLoadingData}
          selectedStorage={selectedStorage}
          t={t}
        />

        <ScheduleComponent isLoadingData={isLoadingData} {...rest} />
      </StyledStoragesModule>
    );
  }
}

export default inject(({ backup }) => {
  const { setCompletedFormFields } = backup;

  return {
    setCompletedFormFields,
  };
})(observer(withTranslation(["Settings", "Common"])(AmazonStorage)));
