import React from "react";
import Box from "@appserver/components/box";
import Button from "@appserver/components/button";
import FieldContainer from "@appserver/components/field-container";
import HelpButton from "@appserver/components/help-button";
import SimpleCheckbox from "./sub-components/SimpleCheckbox";
import SimpleFormField from "./sub-components/SimpleFormField";
import Text from "@appserver/components/text";
import { observer } from "mobx-react";

const FieldMapping = ({ FormStore, t }) => {
  return (
    <Box>
      <Box alignItems="center" displayProp="flex" flexDirection="row">
        <Text as="h2" fontWeight={600}>
          {t("AttributeMatching")}
        </Text>

        <HelpButton
          offsetRight={0}
          tooltipContent={t("AttributeMatchingTooltip")}
        />
      </Box>

      <SimpleFormField
        FormStore={FormStore}
        labelText={t("FirstName")}
        name="firstName"
        placeholder="givenName"
        tabIndex={16}
      />

      <SimpleFormField
        FormStore={FormStore}
        labelText={t("LastName")}
        name="lastName"
        placeholder="sn"
        tabIndex={17}
      />

      <SimpleFormField
        FormStore={FormStore}
        labelText={t("Email", { ns: "Common" })}
        name="email"
        placeholder="sn"
        tabIndex={18}
      />

      <SimpleFormField
        FormStore={FormStore}
        labelText={t("Location")}
        name="location"
        placeholder="sn"
        tabIndex={19}
      />

      <SimpleFormField
        FormStore={FormStore}
        labelText={t("Title")}
        name="title"
        placeholder="sn"
        tabIndex={20}
      />

      <SimpleFormField
        FormStore={FormStore}
        labelText={t("Phone", { ns: "Common" })}
        name="phone"
        placeholder="sn"
        tabIndex={21}
      />

      <FieldContainer
        inlineHelpButton
        isVertical
        labelText={t("AdvancedSettings")}
        place="top"
        tooltipContent={t("AdvancedSettingsTooltip")}
      >
        <SimpleCheckbox
          FormStore={FormStore}
          label={t("HideAuthPage")}
          name="hideAuthPage"
          tabIndex={22}
        />
      </FieldContainer>

      <Box alignItems="center" displayProp="flex" flexDirection="row">
        <Button
          className="save-button"
          label={t("SaveButton", { ns: "Common" })}
          primary
          size="medium"
          tabIndex={23}
        />
        <Button label={t("ResetSettings")} size="medium" tabIndex={24} />
      </Box>
    </Box>
  );
};

export default observer(FieldMapping);