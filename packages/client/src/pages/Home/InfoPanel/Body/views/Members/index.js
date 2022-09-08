import React, { useState, useEffect } from "react";

import UserList from "./UserList";
import { StyledUserTypeHeader } from "../../styles/members";

import IconButton from "@docspace/components/icon-button";
import Text from "@docspace/components/text";
import Loaders from "@docspace/common/components/Loaders";

const Members = ({
  t,
  selfId,
  selection,
  setSelection,
  currentRoomMembers,
  setCurrentRoomMembers,
  getRoomMembers,
}) => {
  const [isLoading, setIsLoading] = useState(false);

  useEffect(async () => {
    if (currentRoomMembers) return;

    setIsLoading(true);
    const fetchedMembers = await getRoomMembers(selection.id);
    setCurrentRoomMembers(fetchedMembers);
    setIsLoading(false);
  }, [currentRoomMembers]);

  if (!currentRoomMembers || isLoading)
    return <Loaders.InfoPanelMemberListLoader />;

  return (
    <>
      <StyledUserTypeHeader>
        <Text className="title">
          {t("Users in room")} : {currentRoomMembers.length}
        </Text>
        <IconButton
          className={"icon"}
          title={t("Common:AddUsers")}
          iconName="/static/images/person+.react.svg"
          isFill={true}
          onClick={() => {}}
          size={16}
          color={"#A3A9AE"}
        />
      </StyledUserTypeHeader>

      <UserList t={t} users={currentRoomMembers} selfId={selfId} />

      {/* <StyledUserTypeHeader>
        <Text className="title">{`${t("Expect people")}:`}</Text>
        <IconButton
          className={"icon"}
          title={t("Repeat invitation")}
          iconName="/static/images/e-mail+.react.svg"
          isFill={true}
          onClick={() => {}}
          size={16}
          color={"#316DAA"}
        />
      </StyledUserTypeHeader>

      <UserList t={t} users={data.members.expect} isExpect /> */}
    </>
  );
};

export default Members;
