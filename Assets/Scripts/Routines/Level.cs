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
	public GameObject nose;
	public CharacterDialog dialog;
	public PlateScript plate;

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
	public void SetHudClock(string text)
	{
		if (hudClock) hudClock.text = text;
	}

	// Use this for initialization
	void Start()
	{
		// No ingredient picking behind the boss dialogue or the countdown (BeginGamePlay unlocks).
		if (GameplayGate.Instance != null) GameplayGate.Instance.Lock(GameplayGate.GateLocks.BossFlow);

		// Boss dialogue plays zoomed out on the whole kitchen with the gameplay area hidden (CameraDirector's Dialogue shot);
		// the move back to the Gameplay shot reveals it.
		CameraDirector.Cut(CameraDirector.Dialogue);

		// The Boss Canvas can be saved inactive in the scene; the intro cannot run (or finish) without it.
		if (dialog != null && !dialog.gameObject.activeSelf) dialog.gameObject.SetActive(true);

		currentLevel = PlayerPrefs.GetInt("current level");
		retries = PlayerPrefs.GetInt("retries", 0);


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
		dialog.Say(introKey, OnBossIntroComplete);

		if (countdownUiRoot) countdownUiRoot.SetActive(false);
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
			nose.GetComponent<NoseWipeScript>().sneezeAllowed = true;

		}

		Debug.Log("Begin Level");
		levelBegan = true;
	}

	private bool IsCatering { get { return PlayerPrefs.HasKey("catering"); } }

	/// <summary>The one way a round ends: the timer ran out, too many orders backed up, or the player sneezed.
	/// State is settled first, then the boss speaks the outcome and the shift is left from his last line.</summary>
	public void EndRound(RoundOutcome outcome) {
		if (roundEnded) return;
		roundEnded = true;

		TutorModal.CloseAll();
		PlayerPrefs.SetInt("tutorial", 1);   // no more tutorial
		Time.timeScale = 0;
		cleanupKitchen();
		CameraDirector.Go(CameraDirector.Dialogue);
		Camera.main.GetComponent<AudioSource>().Stop();

		var analytics = GetComponent<UnityAnalyticsIntegration>();
		string key;
		float wages;
		int score = 0;
		bool fired = false;

		if (outcome == RoundOutcome.Complete) {
			score = ScoreRound();
			wages = PlayerState.AddWagesHours(8f);
			retries = Mathf.Min(retries + score, 5);
			PlayerPrefs.SetInt("retries", retries);
			analytics.careerModeLevelFinished(currentLevel, "full day", Time.time);
			key = IsCatering ? "catering_complete" : (currentLevel % 5 == 0 ? "weekend" : "complete" + score);
		}
		else {
			bool sick = outcome == RoundOutcome.Sick;
			wages = PlayerState.PotentialWagesHours(timeWorked);
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
		var extra = new Dictionary<string, float> { { "retries", retries } };
		dialog.Say(key, choice => OnRoundDialogueDone(outcome, fired, choice), extra, vars);
	}

	private int ScoreRound() {
		int score = 1;
		if (plate.bestMatchedAggregate >= 10) score++;   // longest streak
		if (plate.perfectLevel) score++;
		return score;
	}

	// The boss's last line has been answered. A failed round is paid and warned about only if the player leaves it, not if they retry.
	private void OnRoundDialogueDone(RoundOutcome outcome, bool fired, string choice) {
		if (choice == "retry" && retries > 0) {
			retries--;
			PlayerPrefs.SetInt("retries", retries);
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
