import React, { useState, useCallback, memo } from "react";
import PropTypes from "prop-types";
import DropDown from "./DropDown";
import {
  StyledPhoneInput,
  StyledInputBox,
  StyledDialCode,
  StyledFlagBoxWrapper,
} from "./StyledPhoneInput";
import Box from "../box";
import Text from "../text";
import TextInput from "../text-input";
import { Base } from "../themes";
import { options, countryCodes } from "./options";

const PhoneInput = memo(
  ({ searchEmptyMessage, searchPlaceholderText, onChange, ...props }) => {
    const [country, setCountry] = useState(props.locale);

    const onChangeCountry = useCallback((country) => setCountry(country), [
      country,
    ]);

    const onChangeWrapper = (e) => {
      const value = e.target.value.trim();
      const dialCode = getLocaleCode(country);
      const fullNumber = dialCode.concat(value);
      const locale = country;
      const mask = getMask(country);
      const isValid = mask
        ? !!value.length && !Array.isArray(value.match(/_/gm))
        : !!value.length && Array.isArray(value.match(/^[0-9]+$/));

      onChange &&
        onChange({
          value,
          dialCode,
          fullNumber,
          locale,
          isValid,
        });
    };

    const getLocaleCode = (locale) =>
      options.find((o) => o.code === locale).dialCode;

    const getMask = (locale) => options.find((o) => o.code === locale).mask;

    const getPlaceholder = (locale) =>
      options.find((o) => o.code === locale).mask === null
        ? "Enter phone number"
        : options
            .find((o) => o.code === locale)
            .mask.join("")
            .replace(/[\/|\\]/g, "")
            .replace(/[d]/gi, "X");

    return (
      <Box displayProp="flex" className="input-container">
        <StyledFlagBoxWrapper>
          <DropDown
            value={country}
            onChange={onChangeCountry}
            options={options}
            theme={props.theme}
            searchPlaceholderText={searchPlaceholderText}
            searchEmptyMessage={searchEmptyMessage}
            size={props.size}
          />
        </StyledFlagBoxWrapper>
        <StyledDialCode size={props.size}>
          <Text className="dial-code-text">{getLocaleCode(country)}</Text>
        </StyledDialCode>
        <StyledPhoneInput
          mask={getMask(country)}
          placeholder={getPlaceholder(country)}
          onChange={onChangeWrapper}
          {...props}
        />
      </Box>
    );
  }
);

PhoneInput.propTypes = {
  locale: PropTypes.oneOf(countryCodes),
  getLocaleCode: PropTypes.func,
  getMask: PropTypes.func,
  getPlaceholder: PropTypes.func,
  onChange: PropTypes.func,
  value: PropTypes.string,
  theme: PropTypes.object,
  searchPlaceholderText: PropTypes.string,
  searchEmptyMessage: PropTypes.string,
};

PhoneInput.defaultProps = {
  locale: "RU",
  type: "text",
  value: "",
  theme: Base,
  searchPlaceholderText: "Type to search country",
  searchEmptyMessage: "Nothing found",
};

PhoneInput.displayName = "PhoneInput";

export default PhoneInput;