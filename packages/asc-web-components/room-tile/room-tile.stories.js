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

const Template = (args) => <RoomTile {...args} />;

export const Default = Template.bind({});

Default.args = {};
