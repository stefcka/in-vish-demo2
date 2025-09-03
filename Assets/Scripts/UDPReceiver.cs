using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

public class UDPReceiver : MonoBehaviour
{
    [Header("UDP Settings")]
    public int listenPort = 5000;  // Change this to match Jetson sender port
    private UdpClient udpClient;
    private Thread receiveThread;
    private bool running = false;

    // Event triggered when a new UDP message is received
    public event Action<string> OnUDPMessageReceived;

    private void Start()
    {
        StartReceiver();
    }

    public void StartReceiver()
    {
        try
        {
            udpClient = new UdpClient(listenPort);
            receiveThread = new Thread(ReceiveLoop);
            receiveThread.IsBackground = true;
            running = true;
            receiveThread.Start();
            Debug.Log($"UDP Receiver started on port {listenPort}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"UDP Receiver start failed: {ex.Message}");
        }
    }

    private void ReceiveLoop()
    {
        try
        {
            IPEndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, listenPort);

            while (running)
            {
                if (udpClient.Available > 0)
                {
                    byte[] data = udpClient.Receive(ref remoteEndPoint);
                    string message = Encoding.UTF8.GetString(data);

                    // Enqueue to run safely on main thread
                    MainThreadDispatcher.Enqueue(() =>
                    {
                        OnUDPMessageReceived?.Invoke(message);
                    });

                    Debug.Log($"UDP Received: {message}");
                }
                Thread.Sleep(5); // Sleep a little to prevent CPU spike
            }
        }
        catch (SocketException ex)
        {
            if (running)
                Debug.LogError($"UDP Receiver error: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"UDP Receiver unexpected error: {ex.Message}");
        }
    }
     //{"event": "Shoot", "data": {"x": 10, "y": 500, "player": 1}}
private void OnApplicationQuit()
    {
        StopReceiver();
    }

    public void StopReceiver()
    {
        running = false;
        try
        {
            udpClient?.Close();
            if (receiveThread != null && receiveThread.IsAlive)
                receiveThread.Abort();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error shutting down UDP Receiver: {ex.Message}");
        }
    }
}


