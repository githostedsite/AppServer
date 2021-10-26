import React from "react";
import Badges from "../components/Badges";

export default function withBadges(WrappedComponent) {
  class WithBadges extends React.Component {
    render() {
      return (
        <WrappedComponent
          badgesComponent={<Badges statusType={this.props.item.statusType} />}
          {...this.props}
        />
      );
    }
  }

  return WithBadges;
}
