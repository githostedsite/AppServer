import React from "react";
import PropTypes from "prop-types";
import { Tile, TopPanel, BottomPanel } from "./styled-room-tile";
import Avatar from "../avatar";
import Link from "../link";
import ContextMenuButton from "../context-menu-button";
import Badge from "../badge";
import IconButton from "../icon-button";

const RoomTile = (props) => {
  const { roomName, badgeLabel, onClick, onBadgeClick, onShareClick } = props;

  return (
    <Tile>
      <TopPanel>
        <Badge
          className="button"
          label={badgeLabel}
          noHover={true}
          onClick={onBadgeClick}
        />

        <IconButton
          className="button sharing-btn"
          iconName="/static/images/catalog.share.small.react.svg"
          hoverColor="#3B72A7"
          onClick={onShareClick}
        />
      </TopPanel>
      <Avatar className="tile-icon" role="user" userName={roomName} />
      <BottomPanel>
        <Avatar className="panel-icon" role="user" userName={roomName} />
        <Link fontSize="13px" fontWeight="600" onClick={onClick}>
          {roomName}
        </Link>
        <ContextMenuButton className="panel-btn" />
      </BottomPanel>
    </Tile>
  );
};

RoomTile.propTypes = {
  roomName: PropTypes.string,
  badgeLabel: PropTypes.string,
  onClick: PropTypes.func,
  onBadgeClick: PropTypes.func,
  onShareClick: PropTypes.func,
};

export default RoomTile;
