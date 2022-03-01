# Room Tile

Room Tile

### Usage

```js
import RoomTile from "@appserver/components/room-tile";
```

```jsx
<RoomTile roomName="Test room" badgeLabel="3" />
```

### Properties

| Props          |      Type      | Required | Values | Default | Description                                     |
| -------------- | :------------: | :------: | :----: | :-----: | ----------------------------------------------- |
| `className`    |    `string`    |    -     |   -    |    -    | Accepts class                                   |
| `id`           |    `string`    |    -     |   -    |    -    | Accepts id                                      |
| `style`        | `obj`, `array` |    -     |   -    |    -    | Accepts css style                               |
| `roomName`     |    `string`    |    -     |   -    |    -    | Tile text                                       |
| `badgeLabel`   |    `string`    |    -     |   -    |    -    | Badge text                                      |
| `onClick`      |     `func`     |    -     |   -    |    -    | What the tile will trigger when clicked         |
| `onBadgeClick` |     `func`     |    -     |   -    |    -    | What the badge will trigger when clicked        |
| `onShareClick` |     `func`     |    -     |   -    |    -    | What the share button will trigger when clicked |
