// VishPlayerManager.cs
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VishPlayerManager : MonoBehaviour
{
    public static VishPlayerManager instance;

    // --- your raw data ---
    private List<string> playerNames = new List<string>();
    private List<int> playerScores = new List<int>();

    // --- circular sequence ---
    private List<int> sequence = new List<int>();
    private int sequencePos = 0;

    // --- this round’s picks ---
    private int currentDriverIndex;
    private int currentGunnerIndex;

    // --- scene references ---
    private GameObject inputFieldsParent;
    private GameObject nextPlayersCanvas;
    private GameObject startCanvas;
    private GameObject carCamera;
    private ShotManager shotManager;
    private MouseShooting mouseShooting;

    // --- state flags ---
    private bool gameStarted = false;
    private bool namesEntered = false;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        BindSceneObjects();
        Time.timeScale = 0f;

        // hide everything except name-entry
        carCamera?.SetActive(false);
        if (shotManager != null) shotManager.enabled = false;
        if (mouseShooting != null) mouseShooting.enabled = false;
        if (nextPlayersCanvas != null) nextPlayersCanvas.SetActive(false);
        // startCanvas should be active by default
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        BindSceneObjects();
        if (!gameStarted) Time.timeScale = 0f;

        // re‑bind your buttons
        var startBtn = GameObject.Find("Start Btn")?.GetComponent<UnityEngine.UI.Button>();
        if (startBtn != null)
        {
            startBtn.onClick.RemoveAllListeners();
            startBtn.onClick.AddListener(startGame);
        }
        var readyBtn = GameObject.Find("Ready Btn")?.GetComponent<UnityEngine.UI.Button>();
        if (readyBtn != null)
        {
            readyBtn.onClick.RemoveAllListeners();
            readyBtn.onClick.AddListener(ready);
        }

        // if we’ve already entered names once, skip entry UI & pick immediately
        if (namesEntered)
        {
            startCanvas?.SetActive(false);
            startGame();
        }
    }

    private void BindSceneObjects()
    {
        inputFieldsParent = GameObject.Find("Player Names");
        nextPlayersCanvas = GameObject.Find("NextPlayersCanvas");
        startCanvas = GameObject.Find("StartCanvas");
        carCamera = GameObject.Find("Main Camera");
        shotManager = GameObject.Find("ShotManager")?.GetComponent<ShotManager>();
        mouseShooting = GameObject.Find("MouseShooting")?.GetComponent<MouseShooting>();
    }

    public void startGame()
    {
        // first-ever Start: gather names & build sequence
        if (!namesEntered)
        {
            foreach (var input in inputFieldsParent.GetComponentsInChildren<TMP_InputField>())
                if (!string.IsNullOrWhiteSpace(input.text))
                    playerNames.Add(input.text);

            if (playerNames.Count < 2)
            {
                Debug.LogError("Need at least 2 players!");
                return;
            }

            playerScores = Enumerable.Repeat(0, playerNames.Count).ToList();
            BuildNewSequence();
            namesEntered = true;
        }

        // pick driver+gunner from our circular list
        int n = sequence.Count;
        currentDriverIndex = sequence[sequencePos];
        currentGunnerIndex = sequence[(sequencePos + 1) % n];

        // advance & reshuffle at end
        sequencePos++;
        if (sequencePos >= n)
            BuildNewSequence();

        // swap UIs
        startCanvas?.SetActive(false);
        nextPlayersCanvas?.SetActive(true);

        // update display
        var dTxt = GameObject.Find("DriverName")?.GetComponent<TextMeshProUGUI>();
        if (dTxt != null) dTxt.text = playerNames[currentDriverIndex];
        var gTxt = GameObject.Find("GunnerName")?.GetComponent<TextMeshProUGUI>();
        if (gTxt != null) gTxt.text = playerNames[currentGunnerIndex];
    }

    void BuildNewSequence()
    {
        sequence = Enumerable.Range(0, playerNames.Count)
                             .OrderBy(_ => Random.value)
                             .ToList();
        sequencePos = 0;
    }

    public void ready()
    {
        Camera.main?.gameObject.SetActive(false);
        carCamera?.SetActive(true);
        if (shotManager != null) shotManager.enabled = true;
        if (mouseShooting != null) mouseShooting.enabled = true;

        nextPlayersCanvas?.SetActive(false);

        Time.timeScale = 1f;
        gameStarted = true;
    }

    public void AddPointsToCurrentPlayers(int pts)
    {
        // DEBUG LOG
        Debug.Log($"Adding {pts} points to driver={playerNames[currentDriverIndex]} and gunner={playerNames[currentGunnerIndex]}");

        playerScores[currentDriverIndex] += pts;
        playerScores[currentGunnerIndex] += pts;
    }

    public List<(string name, int score)> GetLeaderboard()
        => playerNames
           .Select((name, i) => (name: name, score: playerScores[i]))
           .OrderByDescending(x => x.score)
           .ToList();

    /// <summary>
    /// Called from GameManager when “Play Again” is clicked.
    /// </summary>
    public void PrepareForNextRound()
    {
        gameStarted = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
