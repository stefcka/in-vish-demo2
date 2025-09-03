// =============================
// File: GameStateManager.cs
// =============================
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    Home,
    GameRunning,
    GameOver
}

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance;

    public GameState CurrentState { get; private set; } = GameState.Home;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetState(GameState newState)
    {
        CurrentState = newState;
        Debug.Log($"Game state changed to: {newState}");

        switch (newState)
        {
            case GameState.Home:
                //SceneManager.LoadScene("Home");
                UI_Manager.Instance.ShowHomeScreen();
                break;
            case GameState.GameRunning:
                //SceneManager.LoadScene("Game");
                UI_Manager.Instance.ShowGameScreen();
                break;
            case GameState.GameOver:
                //SceneManager.LoadScene("GameOver");
                UI_Manager.Instance.ShowGameOverScreen();
                break;
        }
    }

    public void GameEnded()
    {
        UI_Manager.Instance.ShowGameOverScreen();
        if (WebSocketClientManager.Instance != null)
        {
            WebSocketClientManager.Instance.SendMessageToServer("{\"status\":\"GameOver\"}");
        }
        SetState(GameState.GameOver);
    }
}
