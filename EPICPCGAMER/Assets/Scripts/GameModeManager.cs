using UnityEngine;
using System.Collections;

public enum GameMode { None, Campaign, UltimateGamer }

public class GameModeManager : MonoBehaviour
{
    public static GameModeManager Instance { get; private set; }

    [Header("Levels")]
    public LevelData[] campaignLevels;   // Assign Level1, Level2, Level3 assets here

    [Header("UI")]
    public GameObject modeSelectScreen;
    public GameObject gameHUD;
    public GameObject resultsScreen;
    public GameObject levelSelectScreen;

    // State
    public GameMode   CurrentMode    { get; private set; } = GameMode.None;
    public int        CurrentLevel   { get; private set; } = 0;
    public bool       GameActive     { get; private set; } = false;

    // Ultimate Gamer scoring
    public float UltimateScore      { get; private set; } = 0f;
    public float UltimateBestScore  { get; private set; } = 0f;
    public float UltimateTime       { get; private set; } = 0f;

    private NoteSpawner  _spawner;
    private ScoreManager _score;
    private GameManager  _gm;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        _spawner = FindFirstObjectByType<NoteSpawner>();
        _score   = FindFirstObjectByType<ScoreManager>();
        _gm      = GameManager.Instance;

        // Load best score from PlayerPrefs
        UltimateBestScore = PlayerPrefs.GetFloat("UltimateBestScore", 0f);

        ShowModeSelect();
    }

    void Update()
    {
        if (!GameActive) return;

        if (CurrentMode == GameMode.UltimateGamer)
        {
            UltimateTime += Time.deltaTime;
            // Score = accuracy * time * multiplier
            UltimateScore = _score.Accuracy * UltimateTime * 0.1f;
        }
    }

    // ── Mode Select ───────────────────────────────────────────────────────────

    public void ShowModeSelect()
    {
        SetScreen(modeSelectScreen);
        GameActive = false;
    }

    public void SelectCampaign()
    {
        CurrentMode = GameMode.Campaign;
        CurrentLevel = GetHighestUnlockedLevel();
        ShowLevelSelect();
    }

    public void SelectUltimateGamer()
    {
        CurrentMode = GameMode.UltimateGamer;
        StartUltimateGamer();
    }

    // ── Campaign ──────────────────────────────────────────────────────────────

    public void ShowLevelSelect()
    {
        SetScreen(levelSelectScreen);
        // LevelSelectUI reads campaignLevels and unlocked state
    }

    public void StartLevel(int levelIndex)
    {
        if (levelIndex >= campaignLevels.Length) return;
        CurrentLevel = levelIndex;
        _spawner.LoadLevel(campaignLevels[levelIndex]);
        _score.Reset();
        GameActive = true;
        SetScreen(gameHUD);
        _gm.StartGameplay();
    }

    public void EndLevel()
    {
        GameActive = false;
        int stars = GetStars(_score.Accuracy, campaignLevels[CurrentLevel]);

        // Save stars
        int saved = PlayerPrefs.GetInt($"Level{CurrentLevel}Stars", 0);
        if (stars > saved) PlayerPrefs.SetInt($"Level{CurrentLevel}Stars", stars);
        PlayerPrefs.Save();

        // Show results
        var results = resultsScreen.GetComponent<ResultsUI>();
        if (results != null)
            results.Show(stars, _score.Accuracy, CurrentLevel, 
                         stars >= 1 && CurrentLevel < campaignLevels.Length - 1);

        SetScreen(resultsScreen);
    }

    public int GetStars(float accuracy, LevelData level)
    {
        if (accuracy >= level.threeStarAccuracy) return 3;
        if (accuracy >= level.twoStarAccuracy)   return 2;
        if (accuracy >= level.oneStarAccuracy)   return 1;
        return 0;
    }

    public int GetSavedStars(int levelIndex)
        => PlayerPrefs.GetInt($"Level{levelIndex}Stars", 0);

    public bool IsLevelUnlocked(int levelIndex)
    {
        if (levelIndex == 0) return true;
        return GetSavedStars(levelIndex - 1) >= 1;
    }

    int GetHighestUnlockedLevel()
    {
        for (int i = campaignLevels.Length - 1; i >= 0; i--)
            if (IsLevelUnlocked(i)) return i;
        return 0;
    }

    // ── Ultimate Gamer ────────────────────────────────────────────────────────

    void StartUltimateGamer()
    {
        UltimateTime  = 0f;
        UltimateScore = 0f;
        _score.Reset();
        _spawner.LoadInfiniteMode();
        GameActive = true;
        SetScreen(gameHUD);
        _gm.StartGameplay();
    }

    public void EndUltimateGamer()
    {
        GameActive = false;

        // Final score = accuracy * time survived * 0.1
        UltimateScore = _score.Accuracy * UltimateTime * 0.1f;

        if (UltimateScore > UltimateBestScore)
        {
            UltimateBestScore = UltimateScore;
            PlayerPrefs.SetFloat("UltimateBestScore", UltimateBestScore);
            PlayerPrefs.Save();
        }

        var results = resultsScreen.GetComponent<ResultsUI>();
        if (results != null)
            results.ShowUltimate(UltimateScore, UltimateBestScore,
                                 UltimateTime, _score.Accuracy);

        SetScreen(resultsScreen);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    void SetScreen(GameObject show)
    {
        if (modeSelectScreen  != null) modeSelectScreen.SetActive(false);
        if (gameHUD           != null) gameHUD.SetActive(false);
        if (resultsScreen     != null) resultsScreen.SetActive(false);
        if (levelSelectScreen != null) levelSelectScreen.SetActive(false);
        if (show              != null) show.SetActive(true);
    }
}
