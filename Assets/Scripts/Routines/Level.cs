using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;
//using UnionAssets.FLE;

public enum RoundOutcome { Complete, Slow, Sick }

public class Level : MonoBehaviour {
	public TimerScript timer;
	[Tooltip("The NoseWipeScript on the Player prefab's Nose child.")]
	public NoseWipeScript nose;
	[Tooltip("Scene-only sneeze pieces; the nose lives on the Player prefab and cannot reference them, so they are handed over as the shift begins.")]
	public ParticleSystem noseSnot, noseSweat;
	public GameObject noseSnotImage, noseSneezeTutor;
	public CharacterDialog dialog;
	public PlateScript plate;

	// Optional toast shown before the boss intro; toasts.txt "work_enter" decides whether (and which) one plays.
	public HomeToast toast;
	[HideInInspector] public string welcomeToast, cateringToast, returnToast, lateReturnToast;   // superseded by toasts.txt

	public Animator gameElements;
	public Animator HUD;

	public AudioClip gamePlayBackground;
	private bool levelBegan = false;
	private bool roundEnded = false;
	private int currentLevel;
	public int retries;
	private float timeWorked;
	public ReadySetGo readySetGo;
	public GameObject inGameGui; // if not already
	public GameObject countdownUiRoot; 
	private bool _bossIntroHandled = false;
	private Text hudClock; // HUD/Clock/Time

	void Awake()
	{
		// The tutorial must not start at scene load: CameraSequencing brings it up after the boss intro.
		// Done here rather than in the scene file, whose saved active flag Unity keeps rewriting.
		// Awake runs for every object before any Start, so TutorScript.Start never fires early.
		var tutor = FindFirstObjectByType<TutorScript>();
		if (tutor != null) tutor.gameObject.SetActive(false);

		// The HUD stays off until the player reaches the table (AfterBossExit); switching it on plays its slide-in.
		if (inGameGui) inGameGui.SetActive(false);
		if (HUD != null) HudAnim.Prepare(HUD.gameObject);
		if (inGameGui)
		{
			var t = inGameGui.transform.Find("Clock/Time");
			if (t != null) hudClock = t.GetComponent<Text>();
		}
	}

	/// <summary>The HUD's clock text, kept in step with the Pause Panel's.</summary>
	public void SetHudClock(string text, bool fade = false)
	{
		if (!hudClock) return;
		if (fade) AnimUtil.Get<ClockAnim>(hudClock.gameObject).ChangeTo(text);
		else hudClock.text = text;
	}

	// Use this for initialization
	void Start()
	{
		// The scene opens black: the toast (if any) reads on the black, then the scene fades in as it leaves.
		ScreenFade.Black(toast != null ? toast.RootCanvas : null);

		// No ingredient picking behind the boss dialogue or the countdown (BeginGamePlay unlocks).
		if (GameplayGate.Instance != null) GameplayGate.Instance.Lock(GameplayGate.GateLocks.BossFlow);

		// Boss dialogue plays zoomed out on the whole kitchen with the gameplay area hidden (CameraDirector's Dialogue shot);
		// the move back to the Gameplay shot reveals it.
		CameraDirector.Cut(CameraDirector.Dialogue);

		// The Boss Canvas can be saved inactive in the scene; the intro cannot run (or finish) without it.
		// When a toast opens the day, the canvas stays hidden until the toast is gone (ToastThenSay shows it).
		var toastStats = new Dictionary<string, float>
		{
			{ "place", 3 },
			{ "catering", PlayerPrefs.HasKey("catering") ? 1 : 0 },
			{ "tutorial_done", PlayerPrefs.HasKey("tutorial") ? 1 : 0 },
		};
		string toastText = toast != null ? ToastData.Pick("work_enter", toastStats) : null;
		bool playToast = toastText != null;
		if (dialog != null) dialog.gameObject.SetActive(!playToast);

		currentLevel = PlayerPrefs.GetInt("current level");
		retries = TacoLedger.Trust;
		TrustTacoHud.Attach();


		// Arrival at work: 9 AM, up to an hour later when late. Fixed now so the boss's mood and the clock agree.
		float lateness = PlayerPrefs.GetInt("late effect", 0);
		int minutesLate = lateness > 0 ? UnityEngine.Random.Range(10, 61) : 0;
		timer.setWorkday(new TimeSpan(9, minutesLate, 0));

		string introKey;
		if (PlayerPrefs.HasKey("catering"))
		{
			introKey = "catering";
		}
		else if (!PlayerPrefs.HasKey("tutorial"))
		{
			// The tutorial hasn't been done yet: the boss's welcome leads into it, whatever the day number says.
			Debug.Log("First Day of Work!");
			introKey = "welcome";
		}
		else if (PlayerPrefs.GetInt("late effect", 0) > 0)
		{
			Debug.Log("Late to work");
			introKey = "late_return";
		}
		else
		{
			Debug.Log("Returned to work on time");
			introKey = "return";
		}

		Debug.Log($"[Level] intro '{introKey}' (current level={currentLevel}, tutorial done={PlayerPrefs.HasKey("tutorial")}, late effect={PlayerPrefs.GetInt("late effect", 0)}, catering={PlayerPrefs.HasKey("catering")})");
		// The boss slides in and speaks; OnBossIntroComplete follows his last line.
		_bossIntroHandled = false;
		if (playToast) StartCoroutine(ToastThenSay(toastText, introKey));
		else
		{
			ScreenFade.FadeIn(HomeToast.RevealSeconds);
			dialog.Say(introKey, OnBossIntroComplete);
		}

		if (countdownUiRoot) countdownUiRoot.SetActive(false);
	}


	private IEnumerator ToastThenSay(string text, string introKey)
	{
		yield return toast.ShowAndWait(text);
		dialog.gameObject.SetActive(true);
		dialog.Say(introKey, OnBossIntroComplete);
	}

	private void OnBossIntroComplete()
	{
		if (_bossIntroHandled) return;
		_bossIntroHandled = true;

		StartCoroutine(AfterBossExit(() =>
		{
			// Shows the tutorial on a first run, otherwise activates the countdown, which starts itself.
			if (Camera.main)
				AnimUtil.Get<CameraSequencing>(Camera.main.gameObject).Prep();
		}, CameraDirector.Gameplay));
	}

	private IEnumerator AfterBossExit(Action next, string thenShot = null)
	{
		// Wait for the boss's exit fade itself rather than a guessed duration.
		if (dialog.HideTween != null) yield return dialog.HideTween;
		if (thenShot == CameraDirector.Gameplay)
		{
			// The player is at the table: the HUD slides in showing the time they arrived.
			SetHudClock(timer.getArrivalClock());
			if (inGameGui) inGameGui.SetActive(true);
			// No camera move: cut to the gameplay framing and let the trays, plate and nose fade in.
			var reveal = CameraDirector.CutAndReveal(thenShot);
			if (reveal != null) yield return reveal;
		}
		else if (thenShot != null)
		{
			var move = CameraDirector.Go(thenShot);
			if (move != null) yield return move;
		}
		next();
	}

	/// <summary>Called when the tutorial is finished: the boss returns, then the countdown follows his Continue.</summary>
	public void BeginPostTutorial()
	{
		TutorModal.CloseAll();
		if (GameplayGate.Instance != null) GameplayGate.Instance.Lock(GameplayGate.GateLocks.BossFlow);
		// Back to the boss: the gameplay HUD goes away until the countdown (AfterBossExit brings it back).
		if (HUD != null && HUD.gameObject.activeInHierarchy) AnimUtil.Get<HudAnim>(HUD.gameObject).Hide();
		CameraDirector.Go(CameraDirector.Dialogue);
		dialog.Say("post_tutorial", OnPostTutorialComplete);
	}

	private void OnPostTutorialComplete()
	{
		StartCoroutine(AfterBossExit(() =>
		{
			if (countdownUiRoot) countdownUiRoot.SetActive(true);
		}, CameraDirector.Gameplay));
	}

	public void setTimeWorked(float hours) {
		timeWorked = hours;
	}

	public void BeginGamePlay() {
		TutorModal.CloseAll();
		if (ModalFocus.HasFocus) Debug.LogWarning("[Level] a modal still owns input as the shift begins; see the [ModalFocus] log lines above.");
		if (inGameGui) inGameGui.SetActive(true);
		if (GameplayGate.Instance != null) GameplayGate.Instance.Unlock(GameplayGate.GateLocks.BossFlow);

		if (!levelBegan) {
			Camera.main.GetComponent<AudioSource>().clip = gamePlayBackground;
			Camera.main.GetComponent<AudioSource>().Play();
			if (nose != null) {
				nose.Bind(noseSnot, noseSweat, noseSnotImage, noseSneezeTutor, timer.TotalOrders, timer.WipeWindow);
				nose.sneezeAllowed = true;
			}
			else Debug.LogWarning("[Level] nose is not wired to the Player's NoseWipeScript: no sneezing this shift.");

		}

		Debug.Log("Begin Level");
		levelBegan = true;
	}

	// A finished day counts as a sick one when the player is actually ill (health is only dropped below this by the first weekend,
	// so early days never qualify) and the nose nearly ran this many times. Both are guesses to tune in playtest.
	private const float SickHealth = NoseWipeScript.SickHealth;
	private const int SickNearSneezes = 2;

	private const float EndOfDayMusicLevel = 0.25f;   // fraction of the player's music volume left under the boss dialogue
	private const float EndOfDayMusicFade = 1.5f;     // seconds

	private bool IsCatering { get { return PlayerPrefs.HasKey("catering"); } }

	/// <summary>The one way a round ends: the timer ran out, too many orders backed up, or the player sneezed.
	/// State is settled first, then the boss speaks the outcome and the shift is left from his last line.</summary>
	public void EndRound(RoundOutcome outcome) {
		if (roundEnded) return;
		roundEnded = true;

		// The boss is arriving: whatever stage the sneeze is at, it stops here (nearSneezes is kept for the verdict below).
		if (nose != null) nose.StopSneezing();
		TutorModal.CloseAll();
		PlayerPrefs.SetInt("tutorial", 1);   // no more tutorial
		Time.timeScale = 0;
		cleanupKitchen();
		CameraDirector.Go(CameraDirector.Dialogue);
		var music = Camera.main.GetComponent<AudioSource>();
		Tween.Volume(music, music.volume * EndOfDayMusicLevel, EndOfDayMusicFade);

		var analytics = GetComponent<UnityAnalyticsIntegration>();
		string key;
		float wages;
		int score = 0;
		bool fired = false;
		bool taco = false;
		bool sick = false;

		if (outcome == RoundOutcome.Complete) {
			score = ScoreRound();
			wages = PlayerState.AddWagesHours(8f);
			// The boss only hands out a good will taco for a flawless day on which the player was neither late nor sick.
			sick = PlayerPrefs.GetFloat("health", 60f) < SickHealth && nose != null && nose.nearSneezes >= SickNearSneezes;
			taco = score == 3 && PlayerPrefs.GetInt("late effect", 0) == 0 && !sick;
			if (!IsCatering) TacoLedger.RecordResult(taco);
			retries = TacoLedger.Trust;
			analytics.careerModeLevelFinished(currentLevel, "full day", Time.time);
			key = IsCatering ? "catering_complete" : (currentLevel % 5 == 0 ? "weekend" : CompleteKey(score));
		}
		else {
			sick = outcome == RoundOutcome.Sick;
			wages = PlayerState.PotentialWagesHours(timeWorked);
			if (!IsCatering) TacoLedger.RecordResult(false);   // a failed day is a missed chance too
			retries = TacoLedger.Trust;
			fired = PlayerPrefs.GetInt("warnings") + 1 > PlayerPrefs.GetInt("max warnings");
			analytics.careerModeLevelFinished(currentLevel, sick ? "sick" : "mismatches", Time.time);
			if (sick) PlayerPrefs.SetInt("letter", PlayerPrefs.GetInt("sneezes", 1) > 1 ? 2 : 1);
			string kind = sick ? "sick" : "slow";
			key = fired ? "fired_" + kind : (IsCatering ? "catering_" + kind : (sick ? "sneezed" : "slow"));
		}

		var vars = new Dictionary<string, string> {
			{ "wages", wages.ToString("$0.00") },
			{ "score", score.ToString() },
			{ "retries", retries.ToString() },
			{ "orders", plate.getOrders().ToString() },
			{ "day", currentLevel.ToString() },
		};
		var extra = new Dictionary<string, float> { { "retries", retries }, { "trust_tacos", retries }, { "score", score }, { "taco", taco ? 1 : 0 }, { "sick", sick ? 1 : 0 } };
		dialog.Say(key, choice => OnRoundDialogueDone(outcome, fired, choice), extra, vars);
		// Say has already picked its variant, so this only affects later days.
		if (key == "complete_satisfactory") PlayerPrefs.SetInt("satisfactory played", 1);
	}

	// A finished day has 0-2 mistakes (a third ends the shift): wrong ingredients, timeouts and trashed orders all count.
	private int ScoreRound() {
		return Mathf.Clamp(3 - plate.current.numberOfUnmatchedOrders, 1, 3);
	}

	private static string CompleteKey(int score) {
		switch (score) {
			case 3: return "complete_above_average";
			case 2: return "complete_satisfactory";
			default: return "complete_unsatisfactory";
		}
	}

	// The boss's last line has been answered. A failed round is paid and warned about only if the player leaves it, not if they retry.
	private void OnRoundDialogueDone(RoundOutcome outcome, bool fired, string choice) {
		if (choice == "retry" && TacoLedger.SpendTrust()) {
			retries = TacoLedger.Trust;
			StartCoroutine(LeaveAfterBoss(RetryShift));
			return;
		}
		if (outcome != RoundOutcome.Complete) {
			PlayerState.AddWagesHours(timeWorked);
			PlayerPrefs.SetInt("warnings", PlayerPrefs.GetInt("warnings") + 1);
			if (outcome == RoundOutcome.Sick) PlayerPrefs.SetInt("activated", 1);
			if (fired) {
				PlayerPrefs.SetInt("fired", 1);
				PlayerPrefs.SetInt("can cater", 1);
				GetComponent<UnityAnalyticsIntegration>().fired();
			}
		}
		StartCoroutine(LeaveAfterBoss(fired ? (Action)GoToMainMenu : GoHome));
	}

	private void cleanupKitchen() {
		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
	
		for (int i = 0; i < current_recipes.Length; i++) {
			Destroy(current_recipes[i]);
		}
		Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
		gameElements.SetTrigger("fade orders");
		if (HUD != null && HUD.gameObject.activeInHierarchy) AnimUtil.Get<HudAnim>(HUD.gameObject).Hide();
	}

	private IEnumerator LeaveAfterBoss(Action leave) {
		if (dialog.HideTween != null) yield return dialog.HideTween;
		leave();
	}

	private void GoHome() {
		if (CareerFlowManager.Instance != null) CareerFlowManager.Instance.LoadHome();
		else UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
	}

	private void GoToMainMenu() {
		if (CareerFlowManager.Instance != null) CareerFlowManager.Instance.LoadMainMenu();
		else UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
	}

	private void RetryShift() {
		UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
	}
}
