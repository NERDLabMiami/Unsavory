using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Boss dialogue animations (was the Boss Canvas Animator: Boss Reappears / Boss Disappears / Advance Dialog).
// Child objects are found by the same paths the clips used. Everything pops (scale with overshoot + a little jiggle) in and out.
public class BossPanelAnim : MonoBehaviour {
	public float popDuration = 0.4f;
	public float popOutDuration = 0.22f;

	RectTransform _container;
	Graphic _image, _panel, _text;
	Vector3 _imageRest, _panelRest;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_container = transform.Find("Big Container") as RectTransform;
		_image = Find("Big Container/Container/Image");
		_panel = Find("Big Container/Container/Panel");
		_text = Find("Big Container/Container/Panel/Text");
		// Remember the scene's sizes; panels saved collapsed come back at 1.
		if (_panel != null) _panelRest = AnimUtil.RestScale(_panel.transform);
		if (_image != null) _imageRest = AnimUtil.RestScale(_image.transform);
	}

	Graphic Find(string path) {
		var t = transform.Find(path);
		return t != null ? t.GetComponent<Graphic>() : null;
	}

	// ---- Pop helpers (also used for the player's reply button) ----

	/// <summary>Grows t from nothing to rest with overshoot, then wobbles it a little.</summary>
	public static Tween PopIn(Transform t, Vector3 rest, float duration = 0.4f, float delay = 0f, Action done = null) {
		Tween.Cancel(t, "jiggle");
		t.localRotation = Quaternion.identity;
		t.localScale = Vector3.zero;
		Jiggle(t, delay + duration * 0.5f);
		return Tween.Scale(t, rest, duration, Tween.Ease.OutBack, delay, done);
	}

	/// <summary>Shrinks t to nothing.</summary>
	public static Tween PopOut(Transform t, float duration = 0.22f, float delay = 0f, Action done = null) {
		Tween.Cancel(t, "jiggle");
		t.localRotation = Quaternion.identity;
		return Tween.Scale(t, Vector3.zero, duration, Tween.Ease.InBack, delay, done);
	}

	// Damped side-to-side wobble that ends flat. Strength and length come from PopTuning.
	static void Jiggle(Transform t, float delay) {
		var tuning = PopTuning.Current;
		if (tuning.jiggleDegrees <= 0f || tuning.jiggleCycles <= 0f) return;
		float degrees = tuning.jiggleDegrees, cycles = tuning.jiggleCycles;
		Tween.Run(t, "jiggle", tuning.jiggleSeconds, v => {
			if (t == null) return;
			float angle = Mathf.Sin(v * Mathf.PI * 2f * cycles) * degrees * (1f - v);
			t.localRotation = Quaternion.Euler(0f, 0f, angle);
		}, Tween.Ease.Linear, delay, () => { if (t != null) t.localRotation = Quaternion.identity; });
	}

	// ---- Boss panel ----

	bool _shown;
	Tween _showTween;

	public Tween Show() {
		// A second call (a double press on Continue) must not restart the pop-in.
		if (_shown) return _showTween;
		_shown = true;
		SetBlocking(true);
		SetAlpha(_panel, 1f);
		SetAlpha(_text, 1f);
		SetAlpha(_image, 1f);
		if (_container != null) _container.anchoredPosition = new Vector2(0f, _container.anchoredPosition.y);

		Tween panel = _panel != null ? PopIn(_panel.transform, _panelRest, popDuration) : null;
		Tween portrait = _image != null ? PopIn(_image.transform, _imageRest, popDuration, 0.08f) : null;
		_showTween = portrait ?? panel;
		return _showTween;
	}

	// Everything pops out. The returned tween completes when the boss is gone (null if nothing to pop).
	public Tween Hide() {
		_shown = false;
		Tween last = null;
		if (_panel != null) last = PopOut(_panel.transform, popOutDuration);
		if (_image != null) last = PopOut(_image.transform, popOutDuration, 0.06f);
		// A hidden boss is still on a canvas above the other panels; once he is gone he must not swallow their clicks.
		if (last != null) last.OnComplete(() => SetBlocking(false));
		else SetBlocking(false);
		return last;
	}

	// New line of text: the speech panel pops out, the text changes, and it pops back in with a jiggle.
	public void Swap(Action setText, Action landed = null) {
		if (_panel == null) {
			if (setText != null) setText();
			if (landed != null) landed();
			return;
		}
		var t = _panel.transform;
		PopOut(t, 0.15f, 0f, () => {
			if (setText != null) setText();
			PopIn(t, _panelRest, popDuration * 0.8f, 0f, landed);
		});
	}

	// Invisible graphics still catch pointer clicks. While the boss is hidden, stop his non-button graphics from
	// blocking buttons on panels underneath (e.g. Continue on the shift-complete screen). Buttons keep working.
	readonly List<Graphic> _unblocked = new List<Graphic>();

	void SetBlocking(bool on) {
		if (on) {
			foreach (var g in _unblocked) if (g != null) g.raycastTarget = true;
			_unblocked.Clear();
			return;
		}
		foreach (var g in GetComponentsInChildren<Graphic>(true)) {
			if (!g.raycastTarget || g.GetComponentInParent<Selectable>() != null) continue;
			g.raycastTarget = false;
			_unblocked.Add(g);
		}
	}

	static void SetAlpha(Graphic g, float a) {
		if (g == null) return;
		Tween.Cancel(g, "alpha");
		var c = g.color; c.a = a; g.color = c;
	}
}
