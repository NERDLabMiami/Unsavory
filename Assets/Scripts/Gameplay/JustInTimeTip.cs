using System;
using System.Collections;
using UnityEngine;

// A one-time hint shown just as the player needs it. Like the tutorial's prompts it waits a moment first, and
// stays out of the way if the player already does the thing; otherwise it shows a TutorModal that goes away when
// they do it or after a few seconds. It never pauses the game. Each tip is shown once, remembered in PlayerPrefs.
public class JustInTimeTip : MonoBehaviour {
	public const string RocketKey = "tip_rocket";
	public const string SneezeKey = "tip_sneeze";

	const float DelaySeconds = 1f;
	const float ShowSeconds = 5f;

	static JustInTimeTip _runner;
	TutorModal _modal;

	/// <summary>Starts the tip unless it was already seen. `done` is true once the player has done what the tip says
	/// (or it no longer applies); it is polled every frame.</summary>
	public static void Run(string prefsKey, string text, Func<bool> done) {
		if (PlayerPrefs.GetInt(prefsKey, 0) == 1) return;
		PlayerPrefs.SetInt(prefsKey, 1);
		if (_runner == null) _runner = new GameObject("Just In Time Tips").AddComponent<JustInTimeTip>();
		_runner.StartCoroutine(_runner.Routine(text, done));
	}

	/// <summary>The tip text for the device in use: the gamepad names or the keyboard key.</summary>
	public static string Device(string gamepad, string keyboard) {
		return UnityEngine.InputSystem.Gamepad.current != null ? gamepad : keyboard;
	}

	IEnumerator Routine(string text, Func<bool> done) {
		float showAt = Time.realtimeSinceStartup + DelaySeconds;
		while (!done() && Time.realtimeSinceStartup < showAt) yield return null;
		if (done()) yield break;

		// A tip already up (e.g. both fire together) is replaced rather than stacked.
		if (_modal != null) _modal.Dismiss();
		var modal = _modal = TutorModal.Show(text);
		float hideAt = Time.realtimeSinceStartup + ShowSeconds;
		while (!done() && Time.realtimeSinceStartup < hideAt) yield return null;

		modal.Dismiss();
		if (_modal == modal) _modal = null;
	}

	void OnDisable() {
		TutorModal.CloseAll();
		_modal = null;
	}

	void OnDestroy() {
		if (_runner == this) _runner = null;
	}
}
