using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UDPMessageManager : MonoBehaviour
{
    public UDPReceiver udpReceiver; // Assign via Inspector or code

    [System.Serializable]
    public class ShootData
    {
        public int x;
        public int y;
        public int player;
    }

    [System.Serializable]
    public class EventMessage
    {
        public string eventName; // use 'eventName' instead of 'event' (Unity reserves 'event' as a keyword)
        public ShootData data;
    }

    private void Start()
    {
        udpReceiver.OnUDPMessageReceived += HandleMessage;
    }

    private void OnDestroy()
    {
        udpReceiver.OnUDPMessageReceived -= HandleMessage;
    }

    private void HandleMessage(string message)
    {
        Debug.Log("Handling UDP Message: " + message);

        // Split on spaces; expect at least 4 parts now: "HIT", "CAM0", "1000", "500"
        string[] parts = message.Split(' ');
        if (parts.Length < 4 || parts[0] != "HIT")
        {
            Debug.LogWarning("Invalid message format: " + message);
            return;
        }

        string cameraId = parts[1]; // CAM0 or CAM1

        // Changed: parse X and Y from parts[2] and parts[3] instead of splitting a single "1000,500" string
        if (!int.TryParse(parts[2], out int x) || !int.TryParse(parts[3], out int y))
        {
            Debug.LogWarning($"Invalid coordinate values: {parts[2]} {parts[3]}");
            return;
        }

        if (cameraId != "CAM0" && cameraId != "CAM1")
        {
            Debug.LogWarning("Invalid camera ID: " + cameraId);
            return;
        }

        // Create a ShootData object
        ShootData shootData = new ShootData
        {
            //only works on 4k resolution screen. may not work with dlss.
            x = 2 * x,
            y = 2160 - 2 * y,
            player = cameraId == "CAM0" ? 1 : 2 // Assuming CAM0 is player 1 and CAM1 is player 2
        };

        Debug.Log($"Parsed ShootData: x={shootData.x}, y={shootData.y}, player={shootData.player}");

        // Handle the shoot event
        ShotManager.Instance.HandleShoot(shootData);
    }



    void StartGame()
    {
        Debug.Log("Game Started!");
        // Start game logic here
    }

}