using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TutorScript : MonoBehaviour {
	// The tutorial order is always this card (bean and cheese): Resources/Recipes/3-B.
	const string TutorialRecipePath = "Recipes/3-B";
	public GameObject spawningArea;
	public GameObject plate;
	public AudioClip tutorialFinished;
	public Button pauseButton;
	public string tutorialEnd;

	public GameObject continueTutorialDialog;
	public GameObject allTrays;
	public TortillaFoundation tortilla;
	/// <summary>The recipe currently shown by the tutorial (PlateScript matches against it in tutorial mode).</summary>
	public GameObject CurrentRecipe { get; private set; }
	GameObject _recipePrefab;
	// Use this for initialization
	// The old Tutor Panel object is only a host for this script now (the hints are TutorModal windows). Its scene
	// Animator would play "Tutor Appears" when the object turns on, and its OK button would take gamepad focus.
	void RetireLegacyPanel() {
		AnimUtil.DisableAnimator(gameObject);
		var focus = GetComponent<ModalFocus>();
		if (focus != null) focus.enabled = false;
		transform.localScale = Vector3.zero;
		foreach (var g in GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;
		foreach (var b in GetComponentsInChildren<Button>(true)) b.gameObject.SetActive(false);
	}

	void OnDisable() {
		TutorModal.CloseAll();
		_modal = null;
		if (_selector != null) _selector.SelectionChanged -= OnSelectionChanged;
	}

	void Start () {
		RetireLegacyPanel();

		// The boss has left; the tutorial itself needs the trays.
		if (GameplayGate.Instance != null) GameplayGate.Instance.Unlock(GameplayGate.GateLocks.BossFlow);
		tortilla.tutoring = true;
		// The redesigned Plate may have no collider; the tutorial must still start.
		if (plate.TryGetComponent<BoxCollider2D>(out var plateCollider)) plateCollider.enabled = false;
		plate.GetComponent<PlateScript>().tutorialMode = true;
		Time.timeScale = 0;
		if (pauseButton != null) pauseButton.enabled = false;
		// Plate, tortilla and recipe card are on screen before any text.
		getTutorial();

		_selector = FindFirstObjectByType<IngredientGridSelector>();
		if (_selector != null) _selector.SelectionChanged += OnSelectionChanged;
		_intro = StartCoroutine(ControlsIntro());
	}

	// A hint stays up this long (real seconds) unless the player does what it says first.
	const float PromptSeconds = 5f;
	// How long the "Oops" panel stays up.
	const float ResultHoldSeconds = 3f;

	IngredientGridSelector _selector;
	bool _selectionChanged;

	void OnSelectionChanged() { _selectionChanged = true; }

	// The instruction window currently on screen (a timed modal copied from the Sneeze Tutor object), if any.
	TutorModal _modal;

	Coroutine _intro;
	bool _ingredientAdded;
	bool _showingResult;

	// One command per hint, worded for the device in use.
	string[] ControlsPrompts() {
		if (UnityEngine.InputSystem.Gamepad.current != null)
			return new[] { "Use the D-pad or stick to select an ingredient.", "Press the A button to add the ingredient to the order." };
		return new[] { "Use the arrow keys to select an ingredient.", "Press Enter to add the ingredient to the order." };
	}

	/// <summary>Called by a tray when an ingredient lands on the plate during the tutorial.</summary>
	public void OnTutorialIngredientAdded() {
		_ingredientAdded = true;
	}

	// Two hints, one after the other. Nothing here blocks the player: the tutorial itself ends when the order is made.
	IEnumerator ControlsIntro() {
		var prompts = ControlsPrompts();

		_selectionChanged = false;
		_ingredientAdded = false;
		yield return RunPrompt(prompts[0], () => _selectionChanged || _ingredientAdded);

		if (!_ingredientAdded)
			yield return RunPrompt(prompts[1], () => _ingredientAdded);

		_intro = null;
	}

	// Shows a hint until the player does what it says or PromptSeconds pass, then pops it away.
	IEnumerator RunPrompt(string text, System.Func<bool> done) {
		_modal = TutorModal.Show(text);

		float end = Time.realtimeSinceStartup + PromptSeconds;
		while (!done() && Time.realtimeSinceStartup < end) yield return null;

		var hiding = DismissModal();
		while (hiding != null && !hiding.IsDone) yield return null;
	}

	// Pops the window away; null when none is up.
	Tween DismissModal() {
		if (_modal == null) return null;
		var hiding = _modal.Dismiss();
		_modal = null;
		return hiding;
	}

	// Pops the window away and waits until it has gone; does nothing when none is up.
	IEnumerator HidePanel() {
		var hiding = DismissModal();
		if (hiding != null) yield return hiding;
	}

	// Same slot real orders use (the Tutor Panel's own position is in screen pixels, far off-screen).
	Vector3 CardSlot() {
		var layout = FindFirstObjectByType<OrderQueueLayout2D>();
		return layout != null ? layout.FrontSlotWorld : spawningArea.transform.position;
	}

	// Creates the recipe card and makes sure the plate holds just a tortilla. Used at the start and after a mistake.
	public void getTutorial() {
		if (CurrentRecipe != null) Destroy(CurrentRecipe);
		if (_recipePrefab == null) _recipePrefab = Resources.Load<GameObject>(TutorialRecipePath);
		if (_recipePrefab == null) { Debug.LogError("[TutorScript] missing tutorial recipe at Resources/" + TutorialRecipePath); return; }
		// Free choice: every tray can be picked.
		CustomFunctionScript.collisionOnObjects(allTrays, true);
		plate.GetComponent<PlateScript>().EnsureTray();

		// The card slides in from beyond the right edge, the same way real orders arrive.
		var layout = FindFirstObjectByType<OrderQueueLayout2D>();
		Vector3 slot = CardSlot();
		Vector3 start = layout != null ? layout.OffscreenRightWorld : slot;
		GameObject newRecipe =(GameObject) Instantiate(_recipePrefab, start, Quaternion.identity);
		newRecipe.transform.parent = spawningArea.transform;
		Tween.Move(newRecipe.transform, slot, 0.45f, Tween.Ease.OutCubic);
		CurrentRecipe = newRecipe;
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

		// After a mistake the next card slides in with the new tray at once, rather than after the "Oops" text.
		if (!matched) Tween.Delay(0.35f, getTutorial);

		// Put away any prompt still on screen, then bring the panel back with the result.
		yield return HidePanel();
		// A finished tutorial has no window of its own; the boss's post-tutorial dialogue covers it.
		if (!matched) {
			_modal = TutorModal.Show("Oops, try again!");

			yield return new WaitForSecondsRealtime(ResultHoldSeconds);

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
	}

	public void tutorialComplete() {

	}
	public void confirmInstruction() {
		DismissModal();
	}
}
