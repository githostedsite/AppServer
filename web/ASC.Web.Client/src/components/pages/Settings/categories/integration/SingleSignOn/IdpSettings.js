import React from "react";
import Box from "@appserver/components/box";
import FieldContainer from "@appserver/components/field-container";
import RadioButtonGroup from "@appserver/components/radio-button-group";
import SimpleComboBox from "./sub-components/SimpleComboBox";
import SimpleFormField from "./sub-components/SimpleFormField";
import SimpleTextInput from "./sub-components/SimpleTextInput";
import Text from "@appserver/components/text";
import { bindingOptions, nameIdOptions } from "./sub-components/constants";
import { observer } from "mobx-react";
import HideButton from "./sub-components/HideButton";
import UploadXML from "./sub-components/UploadXML";

const IdpSettings = ({ FormStore, t }) => {
  return (
    <Box>
      <HideButton FormStore={FormStore} label="ServiceProviderSettings" t={t} />

      <UploadXML FormStore={FormStore} t={t} />

      <SimpleFormField
        FormStore={FormStore}
        labelText={t("CustomEntryButton")}
        name="spLoginLabel"
        placeholder="Single Sign-on"
        tabIndex={4}
        tooltipContent={t("CustomEntryTooltip")}
      />

      <SimpleFormField
        FormStore={FormStore}
        labelText={t("ProviderURL")}
        name="entityId"
        placeholder="https://www.test.com"
        tabIndex={5}
        tooltipContent={t("ProviderURLTooltip")}
      />

      <FieldContainer
        inlineHelpButton
        isVertical
        labelText={t("EndpointURL")}
        place="top"
        tooltipContent={t("EndpointURLTooltip")}
      >
        <Box displayProp="flex" flexDirection="row" marginProp="0 0 4px 0">
          <Text>{t("Binding")}</Text>

          <RadioButtonGroup
            className="radio-button-group"
            name="binding"
            onClick={FormStore.onBindingChange}
            options={bindingOptions}
            selected={FormStore.ssoBinding}
            spacing="21px"
            tabIndex={6}
          />
        </Box>

        <SimpleTextInput
          FormStore={FormStore}
          name="ssoUrl"
          placeholder="https://www.test.com/saml/login"
          tabIndex={7}
        />
      </FieldContainer>

      <SimpleComboBox
        FormStore={FormStore}
        labelText={t("NameIDFormat")}
        name="nameIdFormat"
        options={nameIdOptions}
        tabIndex={8}
      />
    </Box>
  );
};

export default observer(IdpSettings);