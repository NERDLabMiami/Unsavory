using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// A timed instruction window, built from the "Sneeze Tutor" object in the scene (inactive there, used as a template;
// the sneeze hint itself is untouched). Show() pops a copy in with new text; Dismiss() pops it out and removes it.
// It never touches Time.timeScale: whoever runs the tutorial decides whether the game is paused.
// A window must never outlive its use, so every one is tracked and CloseAll() removes whatever is left.
public class TutorModal : MonoBehaviour {
	static readonly List<TutorModal> Live = new List<TutorModal>();

	Vector3 _rest = Vector3.one;
	bool _closing;

	/// <summary>True once the window has finished popping out (or if there never was one).</summary>
	public bool IsGone { get; private set; }

	public static TutorModal Show(string text, string title = "Tip") {
		SneezeTutor template = null;
		foreach (var s in Object.FindObjectsByType<SneezeTutor>(FindObjectsInactive.Include, FindObjectsSortMode.None))
			if (s.gameObject.scene.IsValid()) { template = s; break; }

		if (template == null) {
			Debug.LogError("[TutorModal] no 'Sneeze Tutor' object in the scene to copy; the tutorial continues without its window.");
			var none = new GameObject("TutorModal (missing template)").AddComponent<TutorModal>();
			none.IsGone = true;
			none._closing = true;
			Destroy(none.gameObject);
			return none;
		}

		var go = Instantiate(template.gameObject, template.transform.parent);
		go.name = "Tutor Modal";
		go.SetActive(false);

		// Strip what the sneeze hint does for itself: its own timer, clip events, focus grab and buttons.
		DestroyImmediate(go.GetComponent<SneezeTutor>());
		var events = go.GetComponent<AnimationFunctions>();
		if (events != null) DestroyImmediate(events);
		var modal = go.GetComponent<ModalFocus>();
		if (modal != null) modal.enabled = false;
		AnimUtil.DisableAnimator(go);
		foreach (var b in go.GetComponentsInChildren<Button>(true)) b.gameObject.SetActive(false);

		foreach (var t in go.GetComponentsInChildren<Text>(true)) {
			t.text = t.gameObject.name == "Title" ? title : text;
			var c = t.color; c.a = 1f; t.color = c;
		}
		// A hint, not a dialog: it must never swallow a click or a tap meant for the game.
		foreach (var g in go.GetComponentsInChildren<Graphic>(true)) g.raycastTarget = false;

		var self = go.AddComponent<TutorModal>();
		self._rest = AnimUtil.RestScale(template.transform);
		Live.Add(self);
		go.transform.SetAsLastSibling();
		go.SetActive(true);
		BossPanelAnim.PopIn(go.transform, self._rest, 0.35f);
		Debug.Log($"[TutorModal] show '{text}' (open: {Live.Count})");
		return self;
	}

	/// <summary>Pops the window out and removes it. Yield on the result to wait for it to be gone.</summary>
	public Tween Dismiss() {
		if (_closing) return null;
		_closing = true;
		// If the pop-out is ever cancelled its completion never runs; this makes sure the window goes anyway.
		Tween.Delay(0.6f, () => { if (this != null) Destroy(gameObject); });
		return BossPanelAnim.PopOut(transform, 0.25f, 0f, () => {
			if (this != null) Destroy(gameObject);
		});
	}

	/// <summary>Removes every tutorial window at once, e.g. when the tutorial ends or its object is switched off.</summary>
	public static void CloseAll() {
		for (int i = Live.Count - 1; i >= 0; i--)
			if (Live[i] != null) Destroy(Live[i].gameObject);
		Live.Clear();
	}

	void OnDestroy() {
		IsGone = true;
		Live.Remove(this);
		Debug.Log($"[TutorModal] closed (open: {Live.Count})");
	}
}
