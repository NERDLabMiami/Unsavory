using UnityEngine;
using UnityEngine.UI;

public class ReadySetGo : MonoBehaviour
{
    private float nextTime;
    private int wordIndex = 0;
    private bool finished = false;
    private bool armed = false;

    public Text display;
    public string[] words;
    public float secondsBetweenWords;
    public GameObject timer;
    public Level level;
    public Button pauseButton;
    public CurrentOrderScript currentOrder;
    public MusicLibrary music;

    void OnEnable()
    {
        // Activated by CameraSequencing.startCountdown (camera pan) or TutorialComplete.startCountdown (tutorial finished).
        if (!armed && !finished) BeginCountdown();
    }

    void Start()
    {
        // Don't begin automatically. This object starts inactive, so Start() first runs the frame after
        // Level activates it, which is after BeginCountdown() has armed it; don't clobber that.
        if (armed) return;

        display.text = "";
        finished = false;

        if (pauseButton) pauseButton.enabled = false;
    }

    /// <summary>
    /// Starts the Ready-Set-Go countdown; runs automatically when this object is activated.
    /// </summary>
    public void BeginCountdown()
    {
        wordIndex = 0;
        finished = false;
        armed = true;

        nextTime = Time.realtimeSinceStartup + secondsBetweenWords;

        if (display && words != null && words.Length > 0)
            display.text = words[wordIndex];

        // Your original audio/pause behavior:
        if (Camera.main)
        {
            var a = Camera.main.GetComponent<AudioSource>();
            if (a) a.Stop();
        }

        if (pauseButton) pauseButton.enabled = false;
    }

    public void startGame()
    {
        timer.GetComponent<PauseScript>().gamePlayStarted = true;
        // A failure in BeginGamePlay must not stop the first order from appearing.
        try { level.BeginGamePlay(); }
        catch (System.Exception e) { Debug.LogException(e); }

        timer.GetComponent<TimerScript>().running = true;
        Time.timeScale = 1f;
        // The first order of the shift starts on a fresh tray, whatever was left on the table before.
        currentOrder.plate.NewTray(false);
        currentOrder.newOrder();
        if (currentOrder.GetActiveOrdersUnsafe().TrueForAll(o => o == null)) currentOrder.newOrder();
        Debug.Log($"[Start] orders={currentOrder.GetActiveOrdersUnsafe().Count} timeScale={Time.timeScale}");

        var a = GetComponent<AudioSource>();
        if (a) a.Stop();

        music.countdownFinished();
        if (pauseButton) pauseButton.enabled = true;
    }

    void Update()
    {
        if (!armed || finished) return;

        if (nextTime <= Time.realtimeSinceStartup)
        {
            nextTime = Time.realtimeSinceStartup + secondsBetweenWords;

            wordIndex++;

            if (wordIndex >= words.Length)
            {
                var anim = GetComponentInChildren<Animator>();
                if (anim) AnimUtil.Get<CountdownWordAnim>(anim.gameObject).Finished();

                display.text = "Get to work.";
                finished = true;

                startGame();
            }
            else
            {
                display.text = words[wordIndex];
                var anim = GetComponentInChildren<Animator>();
                if (anim) AnimUtil.Get<CountdownWordAnim>(anim.gameObject).Wobble();
            }
        }
    }
}
