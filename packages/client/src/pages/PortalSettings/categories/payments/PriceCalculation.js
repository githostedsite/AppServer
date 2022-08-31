import React, { useEffect } from "react";
import styled from "styled-components";
import Text from "@docspace/components/text";
import { inject, observer } from "mobx-react";
import SelectUsersCountContainer from "./sub-components/SelectUsersCountContainer";
import TotalTariffContainer from "./sub-components/TotalTariffContainer";
import { smallTablet } from "@docspace/components/utils/device";
import toastr from "client/toastr";
import AppServerConfig from "@docspace/common/constants/AppServerConfig";
import axios from "axios";
import { combineUrl } from "@docspace/common/utils";
import api from "@docspace/common/api";
import ButtonContainer from "./sub-components/ButtonContainer";

const StyledBody = styled.div`
  border-radius: 12px;
  border: 1px solid #d0d5da;
  max-width: 320px;

  @media ${smallTablet} {
    max-width: 600px;
  }

  padding: 24px;
  box-sizing: border-box;
  p {
    margin-bottom: 24px;
  }
`;

let timeout = null,
  CancelToken,
  source;

const PriceCalculation = ({
  t,
  rights,
  theme,
  setPaymentLink,
  portalQuota,
  setIsLoading,
  setTotalPrice,
  pricePerManager,
  minManagersCount,
  setManagersCount,
  maxAvailableManagersCount,
  isFreeTariff,
}) => {
  const { countAdmin, price } = portalQuota;

  const isAlreadyPaid = !isFreeTariff;

  const initialUsersCount = isAlreadyPaid ? countAdmin : minManagersCount;

  const setStartLink = async () => {
    if (isAlreadyPaid) return;
    const link = await api.portal.getPaymentLink(initialUsersCount);
    setPaymentLink(link);
  };

  useEffect(() => {
    setStartLink();
    setTotalPrice(isAlreadyPaid ? price : minManagersCount * pricePerManager);
    setManagersCount(initialUsersCount);
    return () => {
      timeout && clearTimeout(timeout);
      timeout = null;
    };
  }, []);

  const setShoppingLink = (value) => {
    if (isAlreadyPaid || value > maxAvailableManagersCount) {
      timeout && clearTimeout(timeout);
      setIsLoading(false);
      return;
    }

    setIsLoading(true);

    timeout && clearTimeout(timeout);
    timeout = setTimeout(async () => {
      if (source) {
        source.cancel();
      }

      CancelToken = axios.CancelToken;
      source = CancelToken.source();

      await axios
        .put(
          combineUrl(AppServerConfig.apiPrefixURL, "/portal/payment/url"),
          { quantity: { admin: value } },
          {
            cancelToken: source.token,
          }
        )
        .then((response) => {
          setPaymentLink(response.data.response);
          setIsLoading(false);
        })
        .catch((thrown) => {
          setIsLoading(false);
          if (axios.isCancel(thrown)) {
            console.log("Request canceled", thrown.message);
          } else {
            console.error(thrown);
            toastr.error(thrown);
          }
          return;
        });
    }, 1000);
  };

  const isDisabled = rights === "3" || rights === "2" ? true : false;

  const color = isDisabled ? { color: theme.text.disableColor } : {};

  return (
    <StyledBody>
      <Text fontSize="16px" fontWeight={600} noSelect {...color}>
        {t("PriceCalculation")}
      </Text>
      <SelectUsersCountContainer
        isDisabled={isDisabled}
        setShoppingLink={setShoppingLink}
        isAlreadyPaid={isAlreadyPaid}
      />
      <TotalTariffContainer t={t} isDisabled={isDisabled} />
      <ButtonContainer
        isDisabled={isDisabled}
        t={t}
        isAlreadyPaid={isAlreadyPaid}
      />
    </StyledBody>
  );
};

export default inject(({ auth, payments }) => {
  const {
    tariffsInfo,
    setPaymentLink,
    setIsLoading,
    setTotalPrice,
    maxManagersCount,
    minManagersCount,
    setManagersCount,
    maxAvailableManagersCount,
  } = payments;
  const { theme } = auth.settingsStore;
  const { portalQuota, pricePerManager, isFreeTariff } = auth;
  //const rights = "2";
  //const rights = "3";
  const rights = "1";
  return {
    isFreeTariff,
    setManagersCount,
    tariffsInfo,
    rights,
    theme,
    setPaymentLink,
    portalQuota,
    setIsLoading,
    pricePerManager,
    setTotalPrice,
    maxManagersCount,
    minManagersCount,
    maxAvailableManagersCount,
  };
})(observer(PriceCalculation));
