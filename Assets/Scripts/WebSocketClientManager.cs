// =============================
// File: WebSocketClientManager.cs
// =============================
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

using UnityEngine;

public class WebSocketClientManager : MonoBehaviour
{
    public static WebSocketClientManager Instance;
    private TcpClient client;
    private NetworkStream stream;
    private Thread clientThread;

    private string serverIP = "127.0.0.1";
    private int serverPort = 5050;

    private static readonly Queue<Action> actions = new Queue<Action>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartClient();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void StartClient()
    {
        clientThread = new Thread(() =>
        {
            try
            {
                client = new TcpClient(serverIP, serverPort);
                stream = client.GetStream();
                Debug.Log("Client connected to server.");

                byte[] buffer = new byte[1024];
                while (true)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        string message = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                        Debug.Log("Received from server: " + message);
                        HandleCommand(message);
                    }
                    Thread.Sleep(10);
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("Client connection error: " + ex.Message);

            }
        });
        clientThread.IsBackground = true;
        clientThread.Start();
    }

    private void HandleCommand(string message)
    {
        if (message.Contains("Start"))
        {
           
            MainThreadDispatcher.Enqueue(() => {
                GameStateManager.Instance.SetState(GameState.GameRunning);
                SendMessageToServer("{\"status\":\"Game Started\"}");
            });
        }
        else if (message.Contains("GameOver"))
        {
          
            MainThreadDispatcher.Enqueue(() => {
                GameStateManager.Instance.GameEnded();
                SendMessageToServer("{\"status\":\"Game Over\"}");
            });
        }
        else if (message.Contains("ExitGame"))
        {
            MainThreadDispatcher.Enqueue(() =>
            {
                Debug.Log("Exit command received. Preparing to disconnect...");

                // 🚀 Step 1: Send Disconnecting message
                SendMessageToServer("{\"status\":\"Disconnecting\"}");

                // 🚀 Step 2: Wait a moment to allow server to receive message
                Thread.Sleep(100); // 100ms small delay

                // 🚀 Step 3: Cleanly disconnect and quit
                CleanDisconnect();


                Debug.Log("Exiting game.");
                Application.Quit();
            });
        }

    }

    private void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                var action = actions.Dequeue();
                action.Invoke();
            }
        }
    }

    private void CleanDisconnect()
    {
        try
        {
            if (stream != null)
            {
                stream.Close();
                stream = null;
            }

            if (client != null)
            {
                client.Close();
                client = null;
            }

            if (clientThread != null && clientThread.IsAlive)
            {
                clientThread.Abort();
                clientThread = null;
            }
            SendMessageToServer("Disconnecting");
            Debug.Log("Client disconnected cleanly.");
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Exception during clean disconnect: {ex.Message}");
        }
    }


    public void SendMessageToServer(string message)
    {
        if (stream != null && stream.CanWrite)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            stream.Write(buffer, 0, buffer.Length);
            Debug.Log("Sent to server: " + message);
        }
    }

    private void OnApplicationQuit()
    {
        SendMessageToServer("Disconnecting");
        try
        {
            stream?.Close();
            client?.Close();
            CleanDisconnect();
            if (clientThread != null && clientThread.IsAlive)
                clientThread.Abort();
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Exception during client shutdown: " + ex.Message);
        }
    }
}
