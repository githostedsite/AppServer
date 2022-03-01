import React from "react";

import RoomTile from "./";

export default {
  title: "Components/RoomTile",
  component: RoomTile,
  parameters: {
    docs: {
      description: {
        component: "RoomTile",
      },
    },
  },
};

const Wrapper = (props) => (
  <div
    style={{
      display: "grid",
      gridTemplateColumns: "repeat(auto-fill, minmax(220px, 1fr))",
      gridGap: "16px",
    }}
  >
    {props.children}
  </div>
);

const Template = (args) => (
  <Wrapper>
    <RoomTile {...args} />
    <RoomTile {...args} />
  </Wrapper>
);

export const Default = Template.bind({});

Default.args = {
  roomName: "Test room",
  badgeLabel: "3",
};
