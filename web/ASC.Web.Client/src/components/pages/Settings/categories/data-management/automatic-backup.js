import React from "react";
import Text from "@appserver/components/text";
import { withTranslation } from "react-i18next";

import { inject, observer } from "mobx-react";

import RadioButtonGroup from "@appserver/components/radio-button-group";
import RadioButton from "@appserver/components/radio-button";
import moment from "moment";
import ScheduleComponent from "./sub-components-automatic-backup/scheduleComponent";
import {
  deleteBackupSchedule,
  getBackupProgress,
  getBackupSchedule,
} from "@appserver/common/api/portal";
import SaveCancelButtons from "@appserver/components/save-cancel-buttons";
import toastr from "@appserver/components/toast/toastr";
import SelectedFolder from "files/SelectedFolder";
import ThirdPartyStorageModule from "./sub-components/thirdPartyStorageModule";
import Loader from "@appserver/components/loader";

import FloatingButton from "@appserver/common/components/FloatingButton";
import { StyledModules, StyledComponent } from "./styled-backup";
import ThirdPartyModule from "./sub-components-automatic-backup/thirdPartyModule";
import DocumentsModule from "./sub-components-automatic-backup/documentsModule";

let defaultStorageType = "";

let defaultSelectedOption = "";

let defaultMonthly = false;
let defaultWeekly = false;
let defaultDaily = false;

class AutomaticBackup extends React.PureComponent {
  constructor(props) {
    super(props);
    const { t, language } = props;

    this.lng = language.substring(0, language.indexOf("-"));
    moment.locale(this.lng);

    this._isMounted = false;

    this.state = {
      isShowDocuments: false,
      isShowThirdParty: false,
      isShowThirdPartyStorage: false,

      isCheckedDocuments: false,
      isCheckedThirdParty: false,
      isCheckedThirdPartyStorage: false,

      monthlySchedule: false,
      dailySchedule: false,
      weeklySchedule: false,

      selectedWeekdayOption: "",
      selectedPermission: "disable",
      weekOptions: [],

      defaultSelectedFolder: "",
      defaultStorageType: "",
      defaultHour: "",
      defaultPeriod: "",
      defaultDay: "",
      defaultMaxCopies: "",
      defaultSelectedOption: "",
      defaultSelectedWeekdayOption: "",

      isCopyingToLocal: true,
      isLoadingData: false,
      isLoading: false,
      isDisableOptions: false,

      selectedFolder: "",
      //isPanelVisible: false,
      isChanged: false,
      isSetDefaultFolderPath: false,
      //isError: false,
      downloadingProgress: 100,
    };

    this.periodOptions = [
      {
        key: 1,
        label: t("DailyPeriodSchedule"),
      },
      {
        key: 2,
        label: t("WeeklyPeriodSchedule"),
      },
      {
        key: 3,
        label: t("MonthlyPeriodSchedule"),
      },
    ];

    this.timeOptionsArray = [];
    this.getTimeOptions();
    this.monthNumberOptionsArray = [];
    this.getMonthNumbersOption();

    this.maxNumberCopiesArray = [];
    this.getMaxNumberCopies();

    this.weekdaysOptions = [];
    this.arrayWeekdays = moment.weekdays();
    this._isMounted = false;
  }

  componentDidMount() {
    //const { getCommonThirdPartyList } = this.props;
    this._isMounted = true;
    const { selectedFolder } = this.state;
    this.getWeekdaysOptions();

    getBackupProgress().then((res) => {
      if (res) {
        if (res.progress === 100)
          this.setState({
            isCopyingToLocal: false,
          });
        if (res.progress !== 100)
          this.timerId = setInterval(() => this.getProgress(), 1000);
      } else {
        this._isMounted &&
          this.setState({
            isCopyingToLocal: false,
          });
      }
    });

    this.setState({ isLoading: true }, function () {
      SelectedFolder.getCommonThirdPartyList()
        .then(
          (thirdPartyArray) => (this.commonThirdPartyList = thirdPartyArray)
        )
        .then(() => getBackupSchedule())

        .then((selectedSchedule) => {
          if (selectedSchedule) {
            //debugger;
            const folderId = selectedSchedule.storageParams.folderId;
            this.setState({
              defaultSelectedFolder: [folderId],
              defaultStorageType: `${selectedSchedule.storageType}`,
              defaultHour: `${selectedSchedule.cronParams.hour}:00`,
              defaultPeriod: `${selectedSchedule.cronParams.period}`,
              defaultDay: selectedSchedule.cronParams.day,
              defaultMaxCopies: `${selectedSchedule.backupsStored}`,
            });

            this.onSetDefaultOptions();
          } else {
            this._isMounted &&
              this.setState({
                isLoading: false,
              });
          }
        });
    });
  }

  changedDefaultOptions = (
    defaultSelectedFolder,
    defaultStorageType,
    defaultHour,
    defaultPeriod,
    defaultDay,
    defaultMaxCopies
  ) => {
    this._isMounted &&
      this.setState({
        defaultSelectedFolder: defaultSelectedFolder,
        defaultStorageType: defaultStorageType,
        defaultHour: defaultHour,
        defaultPeriod: defaultPeriod,
        defaultDay: defaultDay,
        defaultMaxCopies: defaultMaxCopies,
      });
  };

  componentWillUnmount() {
    this._isMounted = false;
    clearInterval(this.timerId);
  }

  onSetDefaultOptions = () => {
    const {
      isLoading,
      defaultPeriod,
      defaultStorageType,
      defaultDay,
      defaultHour,
      defaultMaxCopies,
    } = this.state;
    //debugger;

    console.log("onSetDefaultOptions");
    if (defaultStorageType === "0") {
      // Documents Module
      this._isMounted &&
        this.setState({
          selectedPermission: "enable",
          isShowedStorageTypes: true,

          isShowDocuments: true,
          isCheckedDocuments: true,
          // selectedTimeOption: defaultHour,
          // selectedMaxCopies: defaultMaxCopies,
          // selectedNumberMaxCopies: defaultMaxCopies,
        });
    }
    if (defaultStorageType === "1") {
      // ThirdPartyResource Module
      this._isMounted &&
        this.setState({
          selectedPermission: "enable",
          isShowedStorageTypes: true,

          isShowThirdParty: true,
          isCheckedThirdParty: true,
          // selectedTimeOption: defaultHour,
          // selectedMaxCopies: defaultMaxCopies,
          // selectedNumberMaxCopies: defaultMaxCopies,
        });
    }

    if (defaultStorageType === "5") {
      // ThirdPartyStorage Module
      this._isMounted &&
        this.setState({
          selectedPermission: "enable",
          isShowedStorageTypes: true,

          isShowThirdPartyStorage: true,
          isCheckedThirdPartyStorage: true,
          // selectedTimeOption: defaultHour,
          // selectedMaxCopies: defaultMaxCopies,
          // selectedNumberMaxCopies: defaultMaxCopies,
        });
    }

    if (+defaultPeriod === 1) {
      //Every Week option
      const arrayIndex = this.lng === "en" ? defaultDay - 1 : defaultDay - 2; //selected number of week
      this._isMounted &&
        this.setState({
          defaultSelectedOption: this.periodOptions[1].label,
          selectedWeekdayOption: this.weekdaysOptions[arrayIndex].label,
          defaultSelectedWeekdayOption: defaultDay,
          weeklySchedule: true,
        });

      defaultWeekly = true;
      // this.setState({
      //   selectedOption: defaultSelectedOption,
      //   weeklySchedule: true,
      //   selectedWeekdayOption: this.weekdaysOptions[arrayIndex].label,
      //   selectedNumberWeekdayOption: defaultDay,
      // });
    } else {
      if (+defaultPeriod === 2) {
        //Every Month option
        this._isMounted &&
          this.setState({
            defaultSelectedOption: this.periodOptions[2].label,
            monthlySchedule: true,
          });
        // defaultSelectedOption = this.periodOptions[2].label;
        defaultMonthly = true;
        // this.setState({
        //   selectedOption: defaultSelectedOption,
        //   monthlySchedule: true,
        //   selectedMonthOption: `${defaultDay}`, //selected day of month
        // });
      } else {
        defaultDaily = true;
        this._isMounted &&
          this.setState({
            defaultSelectedOption: this.periodOptions[0].label,
          });
        //defaultSelectedOption = this.periodOptions[0].label;
        // this.setState({
        //   selectedOption: defaultSelectedOption,
        // });
      }
    }

    this._isMounted &&
      isLoading &&
      this.setState({
        isLoading: false,
      });
  };
  getProgress = () => {
    const { t } = this.props;

    getBackupProgress()
      .then((res) => {
        if (res) {
          if (res.error.length > 0 && res.progress !== 100) {
            clearInterval(this.timerId);
            this.timerId && toastr.error(`${res.error}`);
            console.log("error", res.error);
            this.timerId = null;
            this.setState({
              isCopyingToLocal: true,
            });
            return;
          }
          if (this._isMounted) {
            this.setState({
              downloadingProgress: res.progress,
            });
          }
          if (res.progress === 100) {
            clearInterval(this.timerId);

            this.timerId && toastr.success(`${t("SuccessCopied")}`);
            this.timerId = null;
            if (this._isMounted) {
              this.setState({
                isCopyingToLocal: false,
              });
            }
          }
        }
      })
      .catch((err) => {
        clearInterval(timerId);
        timerId && toastr.error(err);
        console.log("err", err);

        timerId = null;
        if (this._isMounted) {
          this.setState({
            downloadingProgress: 100,
          });
        }
      });
  };

  getTimeOptions = () => {
    for (let item = 0; item < 24; item++) {
      let obj = {
        key: item,
        label: `${item}:00`,
      };
      this.timeOptionsArray.push(obj);
    }
  };

  getMonthNumbersOption = () => {
    for (let item = 1; item <= 31; item++) {
      let obj = {
        key: item + 24,
        label: `${item}`,
      };
      this.monthNumberOptionsArray.push(obj);
    }
  };

  getMaxNumberCopies = () => {
    const { t } = this.props;
    for (let item = 1; item <= 30; item++) {
      let obj = {
        key: `${item}`,
        label: `${item} ${t("MaxCopies")}`,
      };
      this.maxNumberCopiesArray.push(obj);
    }
  };
  getWeekdaysOptions = () => {
    for (let item = 0; item < this.arrayWeekdays.length; item++) {
      let obj = {
        key: `${item + 1}`,
        label: `${this.arrayWeekdays[item]}`,
      };
      this.weekdaysOptions.push(obj);
    }
    const isEnglishLanguage = this.lng === "en";

    if (!isEnglishLanguage) {
      const startWeek = this.weekdaysOptions[0];
      this.weekdaysOptions.shift();
      this.weekdaysOptions.push(startWeek);
    }
    console.log("this.weekdaysOptions", this.weekdaysOptions);
    this._isMounted &&
      this.setState({
        weekOptions: this.weekdaysOptions,
        selectedWeekdayOption: this.weekdaysOptions[0].label,
      });
  };

  onCancelModuleSettings = () => {
    const {
      isShowDocuments,
      isShowThirdParty,
      selectedPermission,
      isCheckedDocuments,
      monthlySchedule,
      weeklySchedule,
      defaultSelectedFolder,
      defaultStorageType,
      //isError,
    } = this.state;

    this.setState({
      isChanged: false,
      // isSetDefaultFolderPath: true,
      selectedFolder: defaultSelectedFolder,
    });

    //if (isError) this.setState({ isError: false });
    if (defaultStorageType) {
      //debugger;
      selectedPermission === "disable" &&
        this.setState({
          selectedPermission: "enable",
          isShowedStorageTypes: true,
        });

      this.setState({
        selectedOption: defaultSelectedOption,
      });

      if (
        monthlySchedule === defaultWeekly ||
        monthlySchedule === defaultDaily
      ) {
        this.setState({
          monthlySchedule: false,
        });
      }

      if (
        weeklySchedule === defaultMonthly ||
        weeklySchedule === defaultDaily
      ) {
        this.setState({
          weeklySchedule: false,
        });
      }

      this.onSetDefaultOptions();

      if (+defaultStorageType === 0) {
        // Documents Module
        isShowThirdParty
          ? this.setState({
              isShowThirdParty: false,
              isCheckedThirdParty: false,
            })
          : this.setState({
              isShowThirdPartyStorage: false,
              isCheckedThirdPartyStorage: false,
            });
      }

      if (+defaultStorageType === 1) {
        // ThirdPartyResource Module
        isShowDocuments || isCheckedDocuments
          ? this.setState({
              isShowDocuments: false,
              isCheckedDocuments: false,
            })
          : this.setState({
              isShowThirdPartyStorage: false,
              isCheckedThirdPartyStorage: false,
            });
      }

      if (+defaultStorageType === 5) {
        // ThirdPartyStorage Module
        isShowDocuments
          ? this.setState({
              isShowDocuments: false,
              isCheckedDocuments: false,
            })
          : this.setState({
              isShowThirdParty: false,
              isCheckedThirdParty: false,
            });
      }
    } else {
      this.setState({
        selectedPermission: "disable",
        isShowedStorageTypes: false,

        isCheckedDocuments: false,
        isCheckedThirdParty: false,
        isCheckedThirdPartyStorage: false,

        isShowDocuments: false,
        isShowThirdParty: false,
        isShowThirdPartyStorage: false,
      });
    }
  };
  onClickShowStorage = (e) => {
    const name = e.target.name;

    //debugger;

    +name === 0
      ? this.setState(
          {
            isShowDocuments: true,
            isCheckedDocuments: true,
            isShowThirdParty: false,
            isCheckedThirdParty: false,
            isShowThirdPartyStorage: false,
            isCheckedThirdPartyStorage: false,
          },
          function () {
            // this.checkChanges();
          }
        )
      : +name === 1
      ? this.setState(
          {
            isShowDocuments: false,
            isCheckedDocuments: false,
            isShowThirdParty: true,
            isCheckedThirdParty: true,
            isShowThirdPartyStorage: false,
            isCheckedThirdPartyStorage: false,
          },
          function () {
            // this.checkChanges();
          }
        )
      : this.setState(
          {
            isShowDocuments: false,
            isCheckedDocuments: false,
            isShowThirdParty: false,
            isCheckedThirdParty: false,
            isShowThirdPartyStorage: true,
            isCheckedThirdPartyStorage: true,
          },
          function () {
            //this.checkChanges();
          }
        );
  };

  onClickPermissions = (e) => {
    const {
      isCheckedDocuments,
      isCheckedThirdParty,
      isCheckedThirdPartyStorage,
      defaultStorageType,
    } = this.state;
    const name = e.target.defaultValue;
    if (name === "enable") {
      this.setState({
        isShowedStorageTypes: true,
        selectedPermission: "enable",
      });

      //this.checkChanges();

      if (isCheckedDocuments) {
        this.setState({
          isShowDocuments: true,
        });
      } else {
        if (isCheckedThirdParty) {
          this.setState({
            isShowThirdParty: true,
          });
        } else {
          if (isCheckedThirdPartyStorage) {
            this.setState({
              isShowThirdPartyStorage: true,
            });
          } else {
            this.setState({
              isCheckedDocuments: true,
              isShowDocuments: true,
              isChanged: true,
            });
          }
        }
      }
    } else {
      this.setState({
        isShowedStorageTypes: false,
        isShowDocuments: false,
        isShowThirdParty: false,
        isShowThirdPartyStorage: false,
        isCheckedThirdParty: false,
        isCheckedThirdPartyStorage: false,
        isChanged: !defaultStorageType ? false : true,
        selectedPermission: "disable",
      });
    }
    //debugger;
  };
  onClickDeleteSchedule = () => {
    const { t } = this.props;
    this.setState({ isLoadingData: true }, function () {
      deleteBackupSchedule()
        .then(() => toastr.success(t("SuccessfullySaveSettingsMessage")))
        .then(() => getBackupSchedule())
        .then(() => {
          this.setState({
            defaultStorageType: "",
          });
        })
        .catch((error) => toastr.error(error))
        .finally(() =>
          this.setState({ isLoadingData: false, isChanged: false })
        );
    });
  };

  fillStorageFields = (moduleName = "", storageFiledValue = "") => {
    //debugger;
    this.moduleName = moduleName;
    this.storageFiledValue = storageFiledValue;
    this.onSaveModuleSettings();
  };

  onSetLoadingData = (isLoading) => {
    this._isMounted &&
      this.setState({
        isLoadingData: isLoading,
      });
  };
  onSetDisableOptions = (isDisable) => {
    this.setState({
      isDisableOptions: isDisable,
    });
  };
  render() {
    const { t } = this.props;
    const {
      isShowedStorageTypes,
      isCheckedDocuments,
      isCheckedThirdParty,
      isCheckedThirdPartyStorage,
      isShowDocuments,
      isShowThirdParty,
      isShowThirdPartyStorage,
      weeklySchedule,
      monthlySchedule,
      weekOptions,
      selectedOption,
      selectedWeekdayOption,
      selectedTimeOption,
      selectedMonthOption,
      selectedMaxCopies,
      isCopyingToLocal,
      isLoadingData,

      selectedPermission,
      isLoading,
      isChanged,

      isDisableOptions,
      downloadingProgress,

      defaultStorageType,
      defaultHour,
      defaultMaxCopies,
      defaultPeriod,
      defaultDay,
      defaultSelectedWeekdayOption,
      defaultSelectedFolder,
      defaultSelectedOption,
      dailySchedule,
    } = this.state;

    //console.log("commonThirdPartyList auto", this.commonThirdPartyList);
    //console.log("this.props", this.props);
    //console.log("__________________");
    console.log("selectedPermission", selectedPermission);
    return isLoading ? (
      <Loader className="pageLoader" type="rombs" size="40px" />
    ) : (
      <StyledComponent>
        <RadioButtonGroup
          className="automatic-backup_main "
          name={"0"}
          options={[
            {
              label: t("DisableAutomaticBackup"),
              value: "disable",
            },
            {
              label: t("EnableAutomaticBackup"),
              value: "enable",
            },
          ]}
          isDisabled={isLoadingData}
          onClick={this.onClickPermissions}
          orientation="vertical"
          selected={selectedPermission}
        />

        {isShowedStorageTypes && (
          <>
            <StyledModules>
              <RadioButton
                fontSize="13px"
                fontWeight="400"
                label={t("DocumentsModule")}
                name={"0"}
                key={0}
                onClick={this.onClickShowStorage}
                isChecked={isCheckedDocuments}
                isDisabled={isLoadingData}
                value="value"
                className="automatic-backup_radio-button"
              />
              <Text className="category-item-description">
                {t("DocumentsModuleDescription")}
              </Text>
              {isShowDocuments && (
                <>
                  <DocumentsModule
                    isLoadingData={isLoadingData}
                    weeklySchedule={weeklySchedule}
                    monthlySchedule={monthlySchedule}
                    weekOptions={weekOptions}
                    selectedWeekdayOption={selectedWeekdayOption}
                    weekdaysOptions={this.weekdaysOptions}
                    periodOptions={this.periodOptions}
                    lng={this.lng}
                    monthNumberOptionsArray={this.monthNumberOptionsArray}
                    timeOptionsArray={this.timeOptionsArray}
                    maxNumberCopiesArray={this.maxNumberCopiesArray}
                    defaultStorageType={defaultStorageType}
                    defaultSelectedFolder={defaultSelectedFolder}
                    defaultSelectedOption={defaultSelectedOption}
                    defaultHour={defaultHour}
                    defaultMaxCopies={defaultMaxCopies}
                    defaultPeriod={defaultPeriod}
                    defaultDay={defaultDay}
                    defaultSelectedWeekdayOption={defaultSelectedWeekdayOption}
                    defaultWeekly={defaultWeekly}
                    defaultDaily={defaultDaily}
                    defaultMonthly={defaultMonthly}
                    monthlySchedule={monthlySchedule}
                    dailySchedule={dailySchedule}
                    weeklySchedule={weeklySchedule}
                    onCancelModuleSettings={this.onCancelModuleSettings}
                    changedDefaultOptions={this.changedDefaultOptions}
                    onSetDefaultOptions={this.onSetDefaultOptions}
                    onSetLoadingData={this.onSetLoadingData}
                  />
                </>
              )}
            </StyledModules>

            <StyledModules
              isDisabled={
                this.commonThirdPartyList &&
                this.commonThirdPartyList.length === 0
              }
            >
              <RadioButton
                fontSize="13px"
                fontWeight="400"
                label={t("ThirdPartyResource")}
                name={"1"}
                onClick={this.onClickShowStorage}
                isChecked={isCheckedThirdParty}
                isDisabled={
                  isLoadingData ||
                  (this.commonThirdPartyList &&
                    this.commonThirdPartyList.length === 0)
                }
                value="value"
                className="automatic-backup_radio-button"
              />
              <Text className="category-item-description">
                {t("ThirdPartyResourceDescription")}
              </Text>
              <Text className="category-item-description note_description">
                {t("ThirdPartyResourceNoteDescription")}
              </Text>

              {isShowThirdParty && (
                <ThirdPartyModule
                  isLoadingData={isLoadingData}
                  weeklySchedule={weeklySchedule}
                  monthlySchedule={monthlySchedule}
                  weekOptions={weekOptions}
                  selectedWeekdayOption={selectedWeekdayOption}
                  weekdaysOptions={this.weekdaysOptions}
                  periodOptions={this.periodOptions}
                  lng={this.lng}
                  monthNumberOptionsArray={this.monthNumberOptionsArray}
                  timeOptionsArray={this.timeOptionsArray}
                  maxNumberCopiesArray={this.maxNumberCopiesArray}
                  defaultStorageType={defaultStorageType}
                  defaultSelectedFolder={defaultSelectedFolder}
                  defaultSelectedOption={defaultSelectedOption}
                  defaultHour={defaultHour}
                  defaultMaxCopies={defaultMaxCopies}
                  defaultPeriod={defaultPeriod}
                  defaultDay={defaultDay}
                  defaultSelectedWeekdayOption={defaultSelectedWeekdayOption}
                  defaultWeekly={defaultWeekly}
                  defaultDaily={defaultDaily}
                  defaultMonthly={defaultMonthly}
                  monthlySchedule={monthlySchedule}
                  dailySchedule={dailySchedule}
                  weeklySchedule={weeklySchedule}
                  onCancelModuleSettings={this.onCancelModuleSettings}
                  changedDefaultOptions={this.changedDefaultOptions}
                  onSetDefaultOptions={this.onSetDefaultOptions}
                  onSetLoadingData={this.onSetLoadingData}
                />
              )}
            </StyledModules>

            <StyledModules>
              <RadioButton
                fontSize="13px"
                fontWeight="400"
                label={t("ThirdPartyStorage")}
                name={"2"}
                onClick={this.onClickShowStorage}
                isChecked={isCheckedThirdPartyStorage}
                isDisabled={isLoadingData}
                value="value"
                className="automatic-backup_radio-button"
              />
              <Text className="category-item-description">
                {t("ThirdPartyStorageDescription")}
              </Text>
              <Text className="category-item-description note_description">
                {t("ThirdPartyStorageNoteDescription")}
              </Text>
              {isShowThirdPartyStorage && (
                <>
                  <ThirdPartyStorageModule
                    fillStorageFields={this.fillStorageFields}
                    onCancelModuleSettings={this.onCancelModuleSettings}
                    isCopyingToLocal={isCopyingToLocal}
                    isLoadingData={isLoadingData}
                    isDisableOptions={isDisableOptions}
                    isChanged={isChanged}
                    onSetDisableOptions={this.onSetDisableOptions}
                  />
                  <ScheduleComponent
                    weeklySchedule={weeklySchedule}
                    monthlySchedule={monthlySchedule}
                    weekOptions={weekOptions}
                    selectedOption={selectedOption}
                    selectedWeekdayOption={selectedWeekdayOption}
                    selectedTimeOption={selectedTimeOption}
                    selectedMonthOption={selectedMonthOption}
                    selectedMaxCopies={selectedMaxCopies}
                    isLoadingData={isLoadingData}
                    isDisableOptions={isDisableOptions}
                    periodOptions={this.periodOptions}
                    monthNumberOptionsArray={this.monthNumberOptionsArray}
                    timeOptionsArray={this.timeOptionsArray}
                    maxNumberCopiesArray={this.maxNumberCopiesArray}
                    onClickCheckbox={this.onClickCheckbox}
                    onSelectMaxCopies={this.onSelectMaxCopies}
                    onSelectMonthNumberAndTimeOptions={
                      this.onSelectMonthNumberAndTimeOptions
                    }
                    onSelectWeedDay={this.onSelectWeedDay}
                    onSelectPeriod={this.onSelectPeriod}
                  />
                </>
              )}
            </StyledModules>
            {isChanged && !isShowThirdPartyStorage && !isShowThirdParty && (
              <SaveCancelButtons
                className="team-template_buttons"
                onSaveClick={this.onSaveModuleSettings}
                onCancelClick={this.onCancelModuleSettings}
                showReminder={false}
                reminderTest={t("YouHaveUnsavedChanges")}
                saveButtonLabel={t("SaveButton")}
                cancelButtonLabel={t("CancelButton")}
                isDisabled={isCopyingToLocal || isLoadingData}
              />
            )}
          </>
        )}

        {!isShowedStorageTypes && isChanged && (
          <SaveCancelButtons
            className="team-template_buttons"
            onSaveClick={this.onClickDeleteSchedule}
            onCancelClick={this.onCancelModuleSettings}
            showReminder={false}
            reminderTest={t("YouHaveUnsavedChanges")}
            saveButtonLabel={t("SaveButton")}
            cancelButtonLabel={t("CancelButton")}
            isDisabled={isCopyingToLocal || isLoadingData}
          />
        )}

        {downloadingProgress > 0 && downloadingProgress !== 100 && (
          <FloatingButton
            className="layout-progress-bar"
            icon="upload"
            alert={false}
            percent={downloadingProgress}
          />
        )}
      </StyledComponent>
    );
  }
}
export default inject(({ auth }) => {
  const { language } = auth;

  return {
    language,
  };
})(withTranslation("Settings")(observer(AutomaticBackup)));
