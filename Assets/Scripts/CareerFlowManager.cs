using UnityEngine;
using UnityEngine.SceneManagement;
using SimpleJSON;

public sealed class CareerFlowManager : MonoBehaviour
{
    public static CareerFlowManager Instance { get; private set; }

    [Header("Scene Names")]
    public string mainMenuSceneName = "MainMenu";
    public string homeSceneName     = "Home";
    public string workSceneName     = "Game";
    public string creditsSceneName  = "Credits";

    [Header("Career Defaults")]
    public int defaultMaxWarnings   = 3;
    public int defaultDaysUntilComplete = 20;
    public float defaultStartingHealth = 60f;
    public float defaultHourlyRate = 8.05f;
    public int defaultRetries = 3;   // unused: trust tacos now start at 0 (TacoLedger.Reset)

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("[CareerFlowManager]");
        go.AddComponent<CareerFlowManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Leaving a paused/frozen scene (pause menu -> quit, end-of-level, tutorials) must not carry
    // timeScale = 0 into the next one, or its Animators (e.g. the main menu intro) never play.
    // Runs before the new scene's Start(), so scenes that deliberately freeze time in Start still work.
    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Time.timeScale = 1f;
    }

    private void Start()
    {
        EnsureCareerInitialized();
    }

    /// <summary>
    /// Handles one-time bootstrap and post-run reset semantics ("fired"/"reset").
    /// This replaces StartCareer.Start().
    /// </summary>
    public void EnsureCareerInitialized()
    {
        // If fired, you were resetting data but not actually clearing "started career".
        // Keep behavior (reset player data), but you may also want to clear started career
        // depending on desired semantics.
        if (PlayerPrefs.HasKey("fired"))
        {
            CustomFunctionScript.resetPlayerData(defaultStartingHealth, 0);
            Debug.Log("Resetting Game (fired)");
            // Optional: PlayerPrefs.DeleteKey("fired");
        }

        // Global reset flag behavior
        if (PlayerPrefs.HasKey("reset"))
        {
            Debug.Log("Resetting Player (reset flag)");
            CustomFunctionScript.resetPlayerData(defaultStartingHealth, 0);
            PlayerPrefs.DeleteKey("reset");
        }

        // Ensure a career exists
        if (!PlayerPrefs.HasKey("started career"))
        {
            StartNewCareer(); // creates prefs
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Fully defines a new run. This is StartCareer.beginCareer().
    /// Call this from MainMenu "New Game".
    /// </summary>
    public void StartNewCareer()
    {
        // Wipe the previous career (tutorial/sneeze-tutor flags, money, pills, bills, ...) but keep the audio
        // settings. resetPlayerData also keeps evaluation group, times played and the paid-sick-day options.
        float sfxVolume = PlayerPrefs.GetFloat("sfx volume", 1f);
        float backgroundVolume = PlayerPrefs.GetFloat("background volume", 1f);
        string metro = ColaData.CurrentMetroId();   // chosen at New Game, before the reset
        CustomFunctionScript.resetPlayerData(defaultStartingHealth, 0);
        PlayerPrefs.SetFloat("sfx volume", sfxVolume);
        PlayerPrefs.SetFloat("background volume", backgroundVolume);
        PlayerPrefs.SetString("metro", metro);

        PlayerPrefs.SetInt("started career", 1);
        PlayerPrefs.SetInt("current level", 1);
        PlayerPrefs.SetInt("max warnings", defaultMaxWarnings);
        PlayerPrefs.SetInt("final day", defaultDaysUntilComplete);

        PlayerPrefs.SetFloat("health", defaultStartingHealth);
        PlayerPrefs.SetFloat("paycheck", 0);
        PlayerPrefs.SetFloat("earned wages", 0);
        PlayerState.ResetPaychecks();
        MailArchive.Reset();
        MailArchive.DeliverDue();   // day-1 mail
        PlayerPrefs.SetFloat("bank account", 0);
        float wage = ColaData.CurrentMinimumWage();   // the metro's minimum wage, else the national one
        PlayerPrefs.SetFloat("hourly rate", wage > 0f ? wage : defaultHourlyRate);
        TacoLedger.Reset();

        // Effects
        PlayerPrefs.SetInt("health effect", 0);
        PlayerPrefs.SetInt("late effect", 0);
        PlayerPrefs.SetInt("overtime effect", 0);
        PlayerPrefs.SetInt("electricity effect", 0);
        PlayerPrefs.SetInt("phone effect", 0);
        PlayerPrefs.SetInt("insurance effect", 0);

        PlayerPrefs.SetInt("fruit", 5);

        // Bills reset
        ResetBillsFromResource();

        // Clear run-end flags so they don't keep triggering reset logic
        PlayerPrefs.DeleteKey("fired");
        PlayerPrefs.DeleteKey("survived");
        PlayerPrefs.DeleteKey("welcomed home");
        PlayerPrefs.DeleteKey("seen bedroom");
        PlayerPrefs.DeleteKey("seen door");
        ToastData.ResetPlayed();
        PlayerPrefs.DeleteKey("catering");

        PlayerPrefs.Save();
        Debug.Log("Career Started");

        LoadHome();
    }

    private void ResetBillsFromResource()
    {
        var billsText = Resources.Load<TextAsset>("bills");
        if (billsText == null) return;

        JSONNode bills = JSON.Parse(billsText.ToString());
        if (bills == null || bills["bills"] == null) return;

        for (int i = 0; i < bills["bills"].Count; i++)
            PlayerPrefs.SetInt("bill" + i, 0);
    }

    public void ContinueCareer()
    {
        EnsureCareerInitialized();
        LoadHome();
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void LoadHome()
    {
        PlayerPrefs.SetString("resume game", "home");
        PlayerPrefs.Save();
        SceneManager.LoadScene(homeSceneName);
    }

    public void BeginShift()
    {
        PlayerPrefs.SetString("resume game", "game");
        PlayerPrefs.Save();
        SceneManager.LoadScene(workSceneName);
    }

    public void ResumeFromPrefs()
    {
        string resume = PlayerPrefs.GetString("resume game", "home");
        PlayerPrefs.DeleteKey("catering");
        PlayerPrefs.Save();
        if (resume == "home") LoadHome(); else BeginShift();
    }

    /// <summary>True once the month is over (laid off) or the player was evicted; the door then leads to the credits.</summary>
    public bool RunEnded => MailArchive.RunEnd().Length > 0;

    /// <summary>Marks the run as over ("layoff" or "eviction") and delivers the matching ending mail.</summary>
    public void EndRun(string how)
    {
        PlayerPrefs.SetString(MailArchive.RunEndKey, how);
        MailArchive.DeliverDue();
        PlayerPrefs.Save();
    }

    public void LoadCredits()
    {
        SceneManager.LoadScene(creditsSceneName);
    }

    /// <summary>Credits are done: schedule a fresh run and go back to the main menu.</summary>
    public void FinishRun()
    {
        PlayerPrefs.SetInt("survived", 1);
        PlayerPrefs.SetInt("reset", 1);
        PlayerPrefs.DeleteKey(MailArchive.RunEndKey);
        PlayerPrefs.SetString("resume game", "home");
        PlayerPrefs.Save();
        LoadMainMenu();
    }
}
