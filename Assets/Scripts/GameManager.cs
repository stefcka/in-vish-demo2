using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("End-Round UI")]
    public GameObject winScreen;
    public Button playAgainButton;

    [Header("Scoring")]
    public int pointsPerEnemy = 50;
    public int pointsPerProjectileDestroyed = 5;
    public int pointsPerProjectileHitPlayer = -25;
    public int pointsPerCoinCollected = 100;

    [Header("Time Mode")]
    public bool useTimeLimit = false;
    public float timeLimit = 120f;

    [HideInInspector] public int points = 0;
    private bool pointsAdded = false;
    [HideInInspector] public float timeElapsed = 0f;

    private float timeRemaining;
    private int initialEnemyCount;
    public int currentEnemyCount;

    //private GameObject map, mirror;
    private Camera mapCamera, mirrorCamera;

    private TextMeshPro timeCarText;
    private TextMeshPro pointsCarText;

    void Start()
    {
        // mini‑map cameras
        //mapCamera = GameObject.Find("MapCamera").GetComponent<Camera>();
        //mirrorCamera = GameObject.Find("Mirror Camera").GetComponent<Camera>();
        //map = GameObject.Find("Tablet Map Render Texture");
        //mirror = GameObject.Find("Interior mirror");
        //map.SetActive(false);
        //mirror.SetActive(false);
        //mapCamera.enabled = false;
        //mirrorCamera.enabled = false;

        initialEnemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        currentEnemyCount = initialEnemyCount;

        timeCarText = GameObject.Find("Time Car Text").GetComponent<TextMeshPro>();
        timeRemaining = useTimeLimit ? timeLimit : 0f;

        playAgainButton.onClick.RemoveAllListeners();
        playAgainButton.onClick.AddListener(PlayAgain);
    }

    void Update()
    {
        if (points < 0) points = 0;

        if (Input.GetKeyDown(KeyCode.K))
            timeRemaining = 1f;

        /*if (Input.GetKeyDown(KeyCode.M))
        {
            bool show = map.activeInHierarchy;
            map.SetActive(!show);
            mirror.SetActive(!show);
            mapCamera.enabled = !show;
            mirrorCamera.enabled = !show;
        }*/

        // timer
        string displayTime;
        if (useTimeLimit)
        {
            timeRemaining = Mathf.Max(0f, timeRemaining - Time.deltaTime);
            var tRem = TimeSpan.FromSeconds(timeRemaining);
            displayTime = $"{tRem.Minutes:00}:{tRem.Seconds:00}";
        }
        else
        {
            timeElapsed += Time.deltaTime;
            var tEl = TimeSpan.FromSeconds(timeElapsed);
            displayTime = $"{tEl.Minutes:00}:{tEl.Seconds:00}";
        }
        timeCarText.text = displayTime;

        // points HUD
        pointsCarText = GameObject.Find("Points Text").GetComponent<TextMeshPro>();
        pointsCarText.text = points.ToString();

        // end‐round?
        bool timeUp = useTimeLimit && timeRemaining <= 0f && currentEnemyCount > 0;
        bool allDead = currentEnemyCount <= 0;
        if (timeUp || allDead)
            EndRound();
    }

    void EndRound()
    {
        if (!pointsAdded && VishPlayerManager.instance != null)
        {
            VishPlayerManager.instance.AddPointsToCurrentPlayers(points);
            pointsAdded = true;
        }

        winScreen.SetActive(true);

        // show earned points
        var earnedUI = GameObject.Find("Points Earned Text")
                           .GetComponent<TextMeshProUGUI>();
        earnedUI.text = $"+{points}";

        // leaderboard
        var board = VishPlayerManager.instance.GetLeaderboard();
        var lbText = GameObject.Find("LeaderboardText")
                        .GetComponent<TextMeshProUGUI>();
        lbText.text = string.Join("\n",
            board.Select((pair, idx) => $"{idx + 1}. {pair.name}: {pair.score}"));

        Time.timeScale = 0f;
    }

    public void decreaseEnemyCount()
    {
        currentEnemyCount--;
        points += pointsPerEnemy;
    }

    public void PlayAgain()
    {
        if (VishPlayerManager.instance != null)
            VishPlayerManager.instance.PrepareForNextRound();
    }

}
