import React from "react";
import { useTranslation } from "react-i18next";
import { observer, inject } from "mobx-react";

import Text from "@docspace/components/text";
import { ColorTheme, ThemeType } from "@docspace/common/components/ColorTheme";

import { StyledComponent } from "../StyledComponent";

const OfficialDocumentation = () => {
  const { t } = useTranslation("PaymentsEnterprise");

  return (
    <StyledComponent>
      <div className="official-documentation">
        {"—"}
        <Text fontWeight={600}>
          {t("UpgradeToProBannerInstructionItemDocker")}{" "}
          <ColorTheme
            tag="a"
            themeId={ThemeType.Link}
            fontSize="13px"
            fontWeight="600"
            href={""}
            target="_blank"
          >
            {t("UpgradeToProBannerInstructionReadNow")}
          </ColorTheme>
        </Text>

        {"—"}
        <Text fontWeight={600}>
          {t("UpgradeToProBannerInstructionItemLinux")}{" "}
          <ColorTheme
            tag="a"
            themeId={ThemeType.Link}
            fontSize="13px"
            fontWeight="600"
            href={""}
            target="_blank"
          >
            {t("UpgradeToProBannerInstructionReadNow")}
          </ColorTheme>
        </Text>

        {"—"}
        <Text fontWeight={600}>
          {t("UpgradeToProBannerInstructionItemWindows")}{" "}
          <ColorTheme
            tag="a"
            themeId={ThemeType.Link}
            fontSize="13px"
            fontWeight="600"
            href={""}
            target="_blank"
          >
            {t("UpgradeToProBannerInstructionReadNow")}
          </ColorTheme>
        </Text>
      </div>
    </StyledComponent>
  );
};

export default inject(({ auth }) => {
  const { settingsStore } = auth;
  const { theme } = settingsStore;

  return {};
})(observer(OfficialDocumentation));
