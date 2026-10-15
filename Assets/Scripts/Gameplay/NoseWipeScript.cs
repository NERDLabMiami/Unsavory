using UnityEngine;
using System.Collections;

public class NoseWipeScript : MonoBehaviour {
	private float timeBetweenSneezes = 10;
	private float timeBeforeSneezeWarning = 1.5f;
	public float sweatingBeforeSneezeWarning = 1f;
	private float sneezeTimer;
	public GameObject player;
	public GameObject sneezeTutor;
	private bool canStopSneeze = false;
	public bool sneezeAllowed = true;
	private bool isSnotBurstActive = false;
	public ParticleSystem snot;
	public GameObject snotImage;
	public ParticleSystem sweat;
	bool _loggedAllowed;  // TEMP
	float _scrubScore;
	int _lastSign;

	private bool sweating = false;
	float health;
	public float startingHealth;

	void Start () {
		float healthEffect = PlayerPrefs.GetFloat("health effect", 0);
		health = PlayerPrefs.GetFloat("health");
		health = health - healthEffect;
		startingHealth = health;
		Debug.Log("Starting Health: " + health);
		Debug.Log($"[Sneeze] start health={health} sneezeAllowed={sneezeAllowed}");  // TEMP
		initializeSneezing();
		sweatingBeforeSneezeWarning+= timeBeforeSneezeWarning;
		canStopSneeze = true;
	}

	public void initializeSneezing() {
		sneezeTimer = health;
		resetSneezeTimer();
	}

	// Update is called once per frame
	void Update () {
		// A sneeze burst has finished playing: resume the countdown to the next one.
		if (isSnotBurstActive && snot.isStopped)
		{
			isSnotBurstActive = false;
			snotImage.SetActive(false);
			Camera.main.GetComponent<CameraShakeScript>().resetCameraToOriginalPosition();

			sneezeTimer = timeBetweenSneezes;
			sweating = false;
			sweat.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}

		// Runs whenever sneezing is allowed (Level.BeginGamePlay turns it on once the shift starts).
		if (!sneezeAllowed) return;

		if (!_loggedAllowed) { _loggedAllowed = true; Debug.Log($"[Sneeze] counting down from {sneezeTimer:0.0}s (health={health}, startingHealth={startingHealth})"); }  // TEMP
		sneezeTimer -= Time.deltaTime;
		if (sneezeTimer <= 0)
		{
			Debug.Log("Sneezing still");
			Camera.main.GetComponent<CameraShakeScript>().sneeze(timeBetweenSneezes);
			sneezeTimer = timeBetweenSneezes;
			snot.Play();
			snotImage.SetActive(true);
			isSnotBurstActive = true;
			//TAINT INGREDIENTS
			GameObject[] trays = GameObject.FindGameObjectsWithTag("Tray");
			for (int i = 0; i < trays.Length; i++)
			{
				trays[i].GetComponent<AddIngredientScript>().setTainted(true);
			}
		}
		if (sneezeTimer <= sweatingBeforeSneezeWarning && !sweating)
		{
			ParticleSystem.MainModule main = sweat.main;
			main.loop = true;
			sweat.Play();
			sweating = true;
			Debug.Log("Sweating!");
			GetComponent<Animator>().SetTrigger("twitch");
		}
		if (sneezeTimer <= timeBeforeSneezeWarning)
		{
			Camera.main.GetComponent<CameraShakeScript>().giveSneezeWarning(timeBeforeSneezeWarning);
		}
	}

	public void checkSneezeTutor() {
		if (PlayerPrefs.HasKey("sneeze tutor")) {
			//TODO: Launch Sneeze Tutor
			Time.timeScale = 0f;
			sneezeTutor.SetActive(true);
			PlayerPrefs.DeleteKey("sneeze tutor");
		}

	}

	// A wipe is one stroke one way followed by a stroke back within about a second
	// (A/D taps, a right-stick flick, or a mouse/touch drag). The old touch build needed a single swipe.
	const float ScrubStrokeThreshold = 0.5f;
	const float ScrubDecayPerSecond = 0.6f;
	const float ScrubSuccessScore = 1.5f;
	const float ScrubIdleReset = 0.6f;
	float _scrubIdle;

	public void SubmitScrub(float axisX, float dt)
	{
		if (!sneezeAllowed) return;

		_scrubScore = Mathf.Max(0f, _scrubScore - dt * ScrubDecayPerSecond);

		int sign = (axisX > ScrubStrokeThreshold) ? 1 : (axisX < -ScrubStrokeThreshold) ? -1 : 0;
		if (sign == 0)
		{
			// After a pause the next stroke starts a fresh wipe.
			_scrubIdle += dt;
			if (_scrubIdle > ScrubIdleReset) _lastSign = 0;
			return;
		}
		_scrubIdle = 0f;

		// Each new stroke direction counts: the first stroke, then every reversal.
		if (sign != _lastSign)
		{
			Debug.Log($"[Sneeze] stroke {sign} score={_scrubScore + 1f:0.00}");  // TEMP
			_scrubScore += 1f;
			_lastSign = sign;
		}

		if (_scrubScore >= ScrubSuccessScore)
		{
			Wipe(_lastSign > 0);
			_scrubScore = 0f;
			_lastSign = 0;
		}
	}

	bool _swipeLeft;

	/// <summary>One press of the swipe button: a full wipe, alternating sides.</summary>
	public void SwipeFinger()
	{
		if (!sneezeAllowed) return;
		_swipeLeft = !_swipeLeft;
		Wipe(_swipeLeft);
		_scrubScore = 0f;
		_lastSign = 0;
	}

	void Wipe(bool left)
	{
		var anim = GetComponent<Animator>();
		if (anim) anim.SetTrigger(left ? "wipe left" : "wipe right");
		var audio = GetComponent<AudioSource>();
		if (audio) audio.Play();
		resetSneezeTimer();
	}

	public void endSneezeTutorial() {
		Time.timeScale = 1f;
	}
	private void resetSneezeTimer() {
		Debug.Log("Reset Sneeze");
//		GetComponent<Animator>().SetTrigger("relief");
		sweating = false;
        ParticleSystem.MainModule main = sweat.main;
        main.loop = false;
		if (canStopSneeze) {
			health = startingHealth;
			sneezeTimer = health;
			Debug.Log("Sneeze Timer now : " + sneezeTimer);
		}
        /*
        if (Social.localUser.authenticated) {
			GKAchievementReporter.ReportAchievement( Achievements.SNEEZED, 100.0f, true);
		}

*/
    }

}
