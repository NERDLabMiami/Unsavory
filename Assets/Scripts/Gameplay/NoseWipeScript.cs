using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Sneezes are scheduled on the order clock, not in seconds: a sick player gets at least one sneeze per shift however fast
// they work, and a busy day (more orders) has more chances. Each sneeze gives the player one order's pace to wipe their nose.
public class NoseWipeScript : MonoBehaviour {
	/// <summary>Below this health the player is sick: at least one sneeze per shift, and the boss reads the day as "sick".</summary>
	public const float SickHealth = 30f;
	const float HealthFloor = 10f;   // health never decays below this (ReturnHomeScript)

	private float timeBetweenSneezes = 10;   // catering only, which has no order clock
	private float timeBeforeSneezeWarning = 1.5f;
	private float sneezeTimer;
	// The scene-only pieces below are handed over by Level.BeginGamePlay through Bind: this component lives on the Player prefab,
	// which cannot reference scene objects.
	GameObject sneezeTutor;
	public bool sneezeAllowed = false;
	private bool isSnotBurstActive = false;
	ParticleSystem snot;
	GameObject snotImage;
	ParticleSystem sweat;
	bool bound;       // Bind has run: the countdown and the wipe do nothing before that
	bool finishing;   // the last order is in: no new sneeze starts, but one already under way plays out
	bool stopped;     // the round has ended
	float _scrubScore;
	int _lastSign;

	bool sneezeActive;           // a sneeze is building: twitching and sweating until it is wiped or bursts
	public int nearSneezes;      // times this shift the nose was about to run; the boss reads it as "sick" after a finished day
	public float effectiveHealth;
	[Tooltip("Chance that a given order slot sneezes when health is just under the sick threshold.")]
	public float chanceAtThreshold = 0.12f;
	[Tooltip("Chance that a given order slot sneezes at the health floor.")]
	public float chanceAtFloor = 0.40f;
	[Tooltip("Sneezes are at least this many orders apart.")]
	public int minOrdersBetweenSneezes = 2;
	public float emptyFruitHealthPenalty = 3f; // applied while the fruit bowl is empty

	readonly HashSet<int> slots = new HashSet<int>();   // resolved-order counts at which a sneeze begins
	float windowSeconds;   // time to wipe: one order's pace, the same seconds a clock step is worth
	bool endless;          // catering: no shift to schedule against, so sneezes repeat in real time
	float endlessTimer;

	/// <summary>The nose that is running this shift (the one Level bound). Input scripts fall back to it when their own reference is empty,
	/// so wiping works without hand-wiring each of them to the Player prefab's nose.</summary>
	public static NoseWipeScript Active { get; private set; }

	void OnDestroy() {
		if (Active == this) Active = null;
	}

	public void Bind(ParticleSystem snot, ParticleSystem sweat, GameObject snotImage, GameObject sneezeTutor, int totalOrders, float windowSeconds) {
		if (bound) return;
		this.snot = snot;
		this.sweat = sweat;
		this.snotImage = snotImage;
		this.sneezeTutor = sneezeTutor;
		this.windowSeconds = windowSeconds > 0f ? windowSeconds : 8f;
		// The snot splat animates itself whenever it is enabled.
		if (snotImage) AnimUtil.Get<SnotAnim>(snotImage);

		effectiveHealth = PlayerPrefs.GetFloat("health", 60f) - PlayerPrefs.GetInt("health effect", 0);
		if (PlayerPrefs.GetInt("fruit", 5) <= 0) effectiveHealth -= emptyFruitHealthPenalty;
		// Unpaid health insurance makes the player sick however healthy they otherwise are.
		if (BillsState.IsEffectActive("health effect")) effectiveHealth = Mathf.Min(effectiveHealth, SickHealth - 0.01f);
		Debug.Log("Effective health: " + effectiveHealth);

		endless = PlayerPrefs.HasKey("catering");
		if (endless) endlessTimer = Mathf.Max(5f, effectiveHealth);
		else ScheduleSneezes(totalOrders);

		bound = true;
		Active = this;
		OrderAdvanced(0);
	}

	// Rolls the shift's sneezes once, up front. A slot is the number of orders already resolved when the sneeze begins.
	void ScheduleSneezes(int totalOrders) {
		slots.Clear();
		if (effectiveHealth >= SickHealth) return;

		int last = totalOrders - 1;
		if (last < 1) { slots.Add(0); return; }   // a one-order day: the only order is the slot

		float sickness = Mathf.Clamp01((SickHealth - effectiveHealth) / (SickHealth - HealthFloor));
		float chance = Mathf.Lerp(chanceAtThreshold, chanceAtFloor, sickness);
		int previous = -minOrdersBetweenSneezes;
		for (int slot = 1; slot <= last; slot++) {
			if (slot - previous < minOrdersBetweenSneezes) continue;
			if (Random.value < chance) { slots.Add(slot); previous = slot; }
		}
		// Sick means at least one wipe per shift, however the rolls fell.
		if (slots.Count == 0) slots.Add(Random.Range(1, last + 1));
		Debug.Log("[Sneeze] slots " + string.Join(",", slots) + " of " + totalOrders + " orders, window " + windowSeconds + "s");
	}

	/// <summary>The clock moved: this many orders are resolved. A scheduled sneeze begins now.</summary>
	public void OrderAdvanced(int resolvedOrders) {
		if (!bound || finishing || stopped || endless || sneezeActive) return;
		if (slots.Contains(resolvedOrders)) BeginSneeze(windowSeconds);
	}

	void BeginSneeze(float window) {
		sneezeActive = true;
		sneezeTimer = window;
		if (sweat) {
			ParticleSystem.MainModule main = sweat.main;
			main.loop = true;
			sweat.Play();
		}
		nearSneezes++;
		AnimUtil.Get<NoseAnim>(gameObject).Twitch(checkSneezeTutor);
	}

	/// <summary>True while a sneeze is building or happening: the twitch, the tutorial prompt or the burst itself.</summary>
	public bool IsMidSneeze {
		get { return bound && !stopped && (sneezeActive || isSnotBurstActive || (sneezeTutor != null && sneezeTutor.activeInHierarchy)); }
	}

	/// <summary>The last order is in: no new sneeze starts, but one that is under way plays out (wipe it or sneeze) before the day ends.</summary>
	public void WindDown() {
		finishing = true;
	}

	/// <summary>The round is over (the boss is arriving): put everything about the sneeze away, whatever stage it was at.</summary>
	public void StopSneezing() {
		stopped = true;
		finishing = true;
		sneezeAllowed = false;
		sneezeActive = false;
		isSnotBurstActive = false;
		if (sweat) {
			var main = sweat.main;
			main.loop = false;
			sweat.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}
		if (snot) snot.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		if (snotImage) snotImage.SetActive(false);
		// Hiding the tutorial prompt also ends its auto-dismiss, which would otherwise un-freeze the game under the boss.
		if (sneezeTutor) sneezeTutor.SetActive(false);
		var anim = GetComponent<NoseAnim>();
		if (anim) { anim.StopTwitch(); anim.ResetPosition(); }
		var cam = Camera.main != null ? Camera.main.GetComponent<CameraShakeScript>() : null;
		if (cam) cam.ResetSneeze();
	}

	// Update is called once per frame
	void Update () {
		if (!bound) return;

		// A sneeze burst has finished playing (only catering carries on past one; otherwise the round ends as Sick).
		if (isSnotBurstActive && snot.isStopped)
		{
			isSnotBurstActive = false;
			snotImage.SetActive(false);
			Camera.main.GetComponent<CameraShakeScript>().resetCameraToOriginalPosition();
			sneezeActive = false;
			endlessTimer = timeBetweenSneezes;
			sweat.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
		}

		// Runs whenever sneezing is allowed (Level.BeginGamePlay turns it on once the shift starts).
		if (!sneezeAllowed) return;

		if (endless && !sneezeActive && !isSnotBurstActive && !finishing)
		{
			endlessTimer -= Time.deltaTime;
			if (endlessTimer <= 0f) BeginSneeze(timeBeforeSneezeWarning + 2f);
		}
		if (!sneezeActive || isSnotBurstActive) return;

		sneezeTimer -= Time.deltaTime;
		if (sneezeTimer <= 0)
		{
			Camera.main.GetComponent<CameraShakeScript>().sneeze(timeBetweenSneezes);
			snot.Play();
			snotImage.SetActive(true);
			isSnotBurstActive = true;
		}
		else if (sneezeTimer <= timeBeforeSneezeWarning)
		{
			Camera.main.GetComponent<CameraShakeScript>().giveSneezeWarning(timeBeforeSneezeWarning);
		}
	}

	public void checkSneezeTutor() {
		if (stopped || sneezeTutor == null) return;
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
		if (!bound || !sneezeAllowed) return;

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
			_scrubScore += 1f;
			_lastSign = sign;
		}

		if (_scrubScore >= ScrubSuccessScore)
		{
			Wipe();
			_scrubScore = 0f;
			_lastSign = 0;
		}
	}

	/// <summary>One press of the swipe button: a full wipe. (The hand shows it; HandPoseController plays the pose.)</summary>
	public void SwipeFinger()
	{
		if (!bound || !sneezeAllowed) return;
		Wipe();
		_scrubScore = 0f;
		_lastSign = 0;
	}

	// Only a sneeze that is building can be wiped away; once the burst has started it is too late.
	void Wipe()
	{
		if (!sneezeActive || isSnotBurstActive) return;
		AnimUtil.Get<NoseAnim>(gameObject).StopTwitch();
		var audio = GetComponent<AudioSource>();
		if (audio) audio.Play();

		sneezeActive = false;
		endlessTimer = Mathf.Max(5f, effectiveHealth);
		if (sweat) {
			ParticleSystem.MainModule main = sweat.main;
			main.loop = false;
		}
	}
}
