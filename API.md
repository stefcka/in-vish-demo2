# Game Communication API

This document outlines the WebSocket (TCP) and UDP communication protocols used in the game.

## WebSocket (TCP) Communication

The game uses a TCP client to communicate with a server for game state management.

**Connection:**

- **IP Address:** `127.0.0.1`
- **Port:** `5050`

### Messages Sent by the Client

| Message                  | Format | Purpose                                                              |
| ------------------------ | ------ | -------------------------------------------------------------------- |
| **Client Game Started**       | JSON   | Sent when the client receives a "Start" command from the server.     |
| **Client Game Over** | JSON   | Sent when the client receives an "ExitGame" command from the server. |
| **Client Disconnecting** | String | Sent when the application is quitting.                               |

**Example `Client Started` Message:**

```json
{ "status": "Started" }
```

**Example `Client Disconnecting` Message:**

```json
{ "status": "Disconnecting" }
```

### Messages Received by the Client

| Message      | Format | Purpose                                                                                                                 |
| ------------ | ------ | ----------------------------------------------------------------------------------------------------------------------- |
| **Start**    | String | Tells the client to start the game. The client will respond with a `Client Started` message.                            |
| **GameOver** | String | Tells the client that the game has ended.                                                                               |
| **ExitGame** | String | Tells the client to disconnect and quit the application. The client will respond with a `Client Disconnecting` message. |

## UDP Communication

The game uses a UDP listener to receive real-time game events.

**Connection:**

- **Port:** `5000`

### Messages Received by the Client

All UDP messages are expected to be in JSON format.

| Event Name | `data` Object | Purpose                                       |
| ---------- | ------------- | --------------------------------------------- |
| **Shoot**  | `ShootData`   | Instructs the client to register a shot.      |
| **START**  | `null`        | Instructs the client to start the game logic. |

#### `ShootData` Object

| Field    | Type    | Description                      |
| -------- | ------- | -------------------------------- |
| `x`      | integer | The x-coordinate of the shot.    |
| `y`      | integer | The y-coordinate of the shot.    |
| `player` | integer | The player ID who made the shot. |

**Example `Shoot` Message:**

```json
{ "eventName": "Shoot", "data": { "x": 10, "y": 500, "player": 1 } }
```

**Example `START` Message:**

```json
{ "eventName": "START", "data": null }
```
