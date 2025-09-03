# 🎮 Obsidian Game Base Template

This Unity project serves as a **starter/template** for creating games that integrate with the **Obsidian Host System**. Designed for rapid prototyping and deployment, it handles core functionality like host communication, shot event handling, state-driven UI, and demo testing support.

---

## 📦 Features

- 🚀 **WebSocket Integration with Obsidian Host**  
  Automatically connects to the Obsidian Host System after launch.  
  Uses `WebSocketClientManager.cs` to establish and manage a WebSocket connection on **port 5050**.

- 🎯 **Shot Detection via UDP**  
  Listens for "Shoot" event messages sent to **port 5000** using a UDP receiver.  
  `ShotManager.cs` processes incoming shot messages and manages raycast interactions.

- 🧠 **Game State Management**  
  Three main UI states: `Home`, `GameView`, and `GameOver`.  
  Controlled using the `GameStateManager.cs` script.  
  Each state corresponds to a Canvas GameObject for clear scene separation.

- 🕹️ **Gameplay View (GameView)**  
  The main interaction area for gameplay.  
  Handles raycast shooting into the 3D environment based on incoming shot events.  
  Developers can customize raycast hit detection, effects, and scoring here.

- 🧪 **Demo Controls for Offline Development**  
  When not connected to the Obsidian Host system, developers can use built-in UI buttons to simulate all critical actions:
  - **Home Screen**
    - Add/Remove Player 1
    - Add/Remove Player 2
  - **GameView**
    - Simulate Shot Hit for Player 1
    - Simulate Shot Hit for Player 2  
      This allows full game logic and visual feedback testing without any backend dependency.

---

## 🛠️ Getting Started

### Prerequisites

- Unity (2021.3 LTS or newer recommended)
- .NET support for WebSockets and UDP (included by default in Unity)

### Setup

1. Clone or download this repository.
2. Open the project in Unity.
3. (Optional) Run the Obsidian Host System if you want live socket interaction.
4. Press **Play** in Unity Editor:
   - If the host is active, the game connects automatically to **port 5050**.
   - Shot events will be received on **port 5005**.
   - Otherwise, use the **demo controls** to simulate gameplay actions and state transitions manually.

---

## 🧩 Core Scripts

| Script                      | Description                                                                |
| --------------------------- | -------------------------------------------------------------------------- |
| `WebSocketClientManager.cs` | Manages WebSocket connection to the host on **port 5050**.                 |
| `ShotManager.cs`            | Handles UDP-based shot events on **port 5005** and triggers raycast shots. |
| `GameStateManager.cs`       | Switches between Home, GameView, and GameOver canvases.                    |

---

## 📖 API Documentation

For detailed information on the communication protocols, message formats, and how to use them, please see the [API.md](API.md) file.

---

## 🚨 Ports Summary

| Function       | Port | Protocol |
| -------------- | ---- | -------- |
| WebSocket Host | 5050 | TCP      |
| UDP Shots      | 5005 | UDP      |

---

## 🧪 Developing Without Obsidian Host

If you are developing without a live connection to the Obsidian Host system, no worries! You can still test and iterate on your game logic using the **demo control buttons**. These allow you to:

- Add or remove players manually
- Simulate shot hits
- Navigate between game states

This makes local development and debugging simple, fast, and independent of backend availability.

---
