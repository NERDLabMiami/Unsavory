using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TutorScript : MonoBehaviour {
	public List<TutorialRecipe> recipes = new List<TutorialRecipe>();
	public GameObject spawningArea;
	public GameObject plate;
	public AudioClip tutorialFinished;
	public Button pauseButton;
	private int selectedTutorial = 0;
	public string tutorialEnd;

	public GameObject continueTutorialDialog;
	public GameObject allTrays;
	public TortillaFoundation tortilla;
	/// <summary>The recipe currently shown by the tutorial (PlateScript matches against it in tutorial mode).</summary>
	public GameObject CurrentRecipe { get; private set; }
	// Use this for initialization
	void Start () {

		// The boss has left; the tutorial itself needs the trays.
		if (GameplayGate.Instance != null) GameplayGate.Instance.Unlock(GameplayGate.GateLocks.BossFlow);
		tortilla.tutoring = true;
		plate.GetComponent<BoxCollider2D>().enabled = false;
		plate.GetComponent<PlateScript>().tutorialMode = true;
		Time.timeScale = 0;
		pauseButton.enabled = false;
		// Text moves on by itself or when the player acts; there is nothing to press.
		foreach (var b in GetComponentsInChildren<Button>(true))
			if (b.name == "OK") b.gameObject.SetActive(false);

		// Plate, tortilla and recipe card are on screen before any text.
		getTutorial();

		// The Tutor Appears clip is already showing the panel.
		_panelShown = true;
		IntroPlaying = true;
		_intro = StartCoroutine(ControlsIntro());
	}

	/// <summary>True during the "move the stick" prompt, when nothing may be added yet.</summary>
	public bool IntroPlaying { get; private set; }

	[Header("Text timing (real seconds)")]
	[Tooltip("How long a controls prompt stays on screen. -1 = until the player does what it asks. After the timeout the panel clears but the tutorial still waits for the input.")]
	public float promptTimeout = 10f;
	[Tooltip("How long the 'Nice job' / 'Oops' text stays up.")]
	public float resultHoldTime = 3f;

	Coroutine _intro;
	bool _panelShown;
	bool _ingredientAdded;
	bool _showingResult;

	// One command per prompt, worded for the device in use.
	string[] ControlsPrompts() {
		if (UnityEngine.InputSystem.Gamepad.current != null)
			return new[] { "Use the stick or D-pad to move.", "Press A to confirm your selection." };
		return new[] { "Use the arrow keys to move.", "Press Enter to confirm your selection." };
	}

	/// <summary>Called by a tray when an ingredient lands on the plate during the tutorial.</summary>
	public void OnTutorialIngredientAdded() {
		_ingredientAdded = true;
	}

	IEnumerator ControlsIntro() {
		var prompts = ControlsPrompts();

		// 1. Move. Nothing can be added until the stick/D-pad (or a pointer press) is used.
		yield return RunPrompt(prompts[0], () => GameInput.Instance != null &&
			(GameInput.Instance.NavigateStep != Vector2Int.zero || GameInput.Instance.NavigateHeld != Vector2Int.zero ||
			 (GameInput.Instance.ActivatePressed && GameInput.Instance.ActivateFromPointer)));

		// 2. Confirm. Ingredients can be picked from here on.
		IntroPlaying = false;
		_ingredientAdded = false;
		yield return RunPrompt(prompts[1], () => _ingredientAdded ||
			(GameInput.Instance != null && (GameInput.Instance.ConfirmPressed || GameInput.Instance.ActivatePressed)));

		_intro = null;
	}

	// Shows the prompt until the player does what it asks, or until promptTimeout (-1 = never) clears the panel.
	// The next step never starts before the input has arrived, even if the panel has already gone.
	IEnumerator RunPrompt(string text, System.Func<bool> done) {
		GetComponentInChildren<Text>().text = text;
		if (!_panelShown) { GetComponent<Animator>().SetTrigger("next"); _panelShown = true; }

		float end = Time.realtimeSinceStartup + promptTimeout;
		yield return null;   // let the press that started this frame go by

		// The input is a single-frame press, so remember it once seen, including while the panel is shrinking away.
		bool satisfied = false;
		while (!satisfied && (promptTimeout < 0f || Time.realtimeSinceStartup < end)) {
			if (done()) satisfied = true; else yield return null;
		}

		if (_panelShown) {
			_panelShown = false;
			confirmInstruction();
			float hidden = Time.realtimeSinceStartup + 0.4f;
			while (Time.realtimeSinceStartup < hidden) {
				if (!satisfied && done()) satisfied = true;
				yield return null;
			}
		}

		// Timed out without the input: the panel is gone, but the tutorial still waits for it.
		while (!satisfied) {
			if (done()) satisfied = true; else yield return null;
		}
	}

	// The panel's "confirmed" trigger shrinks it away; only send it when it is up, so no trigger is left pending.
	IEnumerator HidePanel() {
		if (!_panelShown) yield break;
		_panelShown = false;
		confirmInstruction();
		yield return new WaitForSecondsRealtime(0.4f);
	}

	// Same slot real orders use (the Tutor Panel's own position is in screen pixels, far off-screen).
	Vector3 CardSlot() {
		var layout = FindFirstObjectByType<OrderQueueLayout2D>();
		return layout != null ? layout.FrontSlotWorld : spawningArea.transform.position;
	}

	// Creates the recipe card and makes sure the plate holds just a tortilla. Used at the start and after a mistake.
	public void getTutorial() {
		if (CurrentRecipe != null) Destroy(CurrentRecipe);
		// Free choice: every tray can be picked.
		CustomFunctionScript.collisionOnObjects(allTrays, true);
		plate.GetComponent<PlateScript>().SeedTortilla();

		selectedTutorial = Random.Range (0, recipes.Count);
		GameObject newRecipe =(GameObject) Instantiate(recipes[selectedTutorial].recipe, CardSlot(), Quaternion.identity);
		newRecipe.transform.parent = spawningArea.transform;
		CurrentRecipe = newRecipe;
		Debug.Log($"[Tutorial] recipe card {newRecipe.name} world={newRecipe.transform.position} parent={spawningArea.name} " +
			$"viewport={(Camera.main ? Camera.main.WorldToViewportPoint(newRecipe.transform.position).ToString() : "no camera")} " +
			$"renderers={newRecipe.GetComponentsInChildren<SpriteRenderer>().Length} timeScale={Time.timeScale}");
	}

	// Shows the result as animated text, then moves on by itself: a finished tutorial starts the countdown,
	// a mistake starts a fresh attempt.
	public void continueTutorial(bool matched) {
		if (_showingResult) return;
		StartCoroutine(ShowResult(matched));
	}

	IEnumerator ShowResult(bool matched) {
		_showingResult = true;
		if (_intro != null) { StopCoroutine(_intro); _intro = null; }
		IntroPlaying = false;

		// Put away any prompt still on screen, then bring the panel back with the result.
		yield return HidePanel();
		// A finished tutorial has no panel text of its own; the boss's post-tutorial dialogue covers it.
		if (!matched) {
			GetComponentInChildren<Text>().text = "Oops, try again!";
			GetComponent<Animator>().SetTrigger("next");
			_panelShown = true;

			yield return new WaitForSecondsRealtime(resultHoldTime);

			yield return HidePanel();
		}
		yield return new WaitForSecondsRealtime(0.25f);

		_showingResult = false;
		if (matched) {
			var done = continueTutorialDialog.GetComponent<TutorialComplete>();
			done.finishTutorial();
			var level = FindFirstObjectByType<Level>();
			if (level != null) level.BeginPostTutorial();
			else done.startCountdown();
			gameObject.SetActive(false);
		}
		else {
			// New recipe, clean plate; the prompts are not replayed.
			getTutorial();
		}
	}

	public void tutorialComplete() {
//		GetComponent<Animator>().SetTrigger("disappear");
	}
	public void confirmInstruction() {
		GetComponent<Animator>().SetTrigger("confirmed");
	}
}
