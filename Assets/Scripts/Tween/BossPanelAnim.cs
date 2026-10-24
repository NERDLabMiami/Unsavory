using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Boss dialogue animations (was the Boss Canvas Animator: Boss Reappears / Boss Disappears / Advance Dialog).
// Child objects are found by the same paths the clips used. Everything pops (scale with overshoot + a little jiggle) in and out.
public class BossPanelAnim : MonoBehaviour {
	public float popDuration = 0.4f;
	public float popOutDuration = 0.22f;
	[Tooltip("Boss portrait sliding in from the right edge.")]
	public float slideDuration = 0.45f;
	[Tooltip("Speech bubble scale/fade in, after the portrait has landed.")]
	public float bubbleDuration = 0.25f;

	const float BubbleStartScale = 0.85f;

	RectTransform _container, _imageRt;
	Graphic _image, _panel, _text;
	CanvasGroup _panelGroup;
	Vector3 _imageRest, _panelRest;
	Vector2 _imageRestPos;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_container = transform.Find("Big Container") as RectTransform;
		_image = Find("Big Container/Container/Image");
		_panel = Find("Big Container/Container/Panel");
		_text = Find("Big Container/Container/Panel/Text");
		// Remember the scene's sizes; panels saved collapsed come back at 1.
		if (_panel != null) {
			_panelRest = AnimUtil.RestScale(_panel.transform);
			_panelGroup = AnimUtil.Get<CanvasGroup>(_panel.gameObject);
		}
		if (_image != null) {
			_imageRest = AnimUtil.RestScale(_image.transform);
			_imageRt = _image.rectTransform;
			_imageRestPos = _imageRt.anchoredPosition;
		}
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

	// ---- Expression ----

	Sprite _neutral;

	/// <summary>Swaps the portrait sprite (a null sprite restores the one the scene started with). The pop is left alone.</summary>
	public void SetFace(Sprite sprite) {
		var img = _image as Image;
		if (img == null) return;
		if (_neutral == null) _neutral = img.sprite;
		img.sprite = sprite != null ? sprite : _neutral;
	}

	// ---- Boss panel ----

	bool _shown;
	Tween _showTween;

	// The portrait slides in from the right, then the speech bubble scales/fades in. The returned tween completes
	// once the bubble has landed (callers hang the player's answers off it).
	public Tween Show() {
		// A second call (a double press on Continue) must not restart the animation.
		if (_shown) return _showTween;
		_shown = true;
		SetBlocking(true);
		SetAlpha(_panel, 1f);
		SetAlpha(_text, 1f);
		SetAlpha(_image, 1f);
		if (_container != null) _container.anchoredPosition = new Vector2(0f, _container.anchoredPosition.y);

		float slide = 0f;
		if (_imageRt != null) {
			Tween.Cancel(_imageRt.transform, "jiggle");
			_imageRt.localRotation = Quaternion.identity;
			_imageRt.localScale = _imageRest;
			_imageRt.anchoredPosition = _imageRestPos + OffscreenRight();
			slide = slideDuration;
			Tween.Anchored(_imageRt, _imageRestPos, slide, Tween.Ease.OutCubic);
		}
		if (_panel != null) BubbleIn(slide, bubbleDuration);
		_showTween = Tween.Delay(slide + (_panel != null ? bubbleDuration : 0f));
		return _showTween;
	}

	// The bubble fades and the portrait slides back out to the right. The returned tween completes when the boss is gone (null if nothing to animate).
	public Tween Hide() {
		_shown = false;
		Tween last = null;
		if (_panel != null) last = BubbleOut(popOutDuration);
		if (_imageRt != null) last = Tween.Anchored(_imageRt, _imageRestPos + OffscreenRight(), popOutDuration + 0.1f, Tween.Ease.InQuad, 0.05f);
		// A hidden boss is still on a canvas above the other panels; once he is gone he must not swallow their clicks.
		if (last != null) last.OnComplete(() => SetBlocking(false));
		else SetBlocking(false);
		return last;
	}

	// New line of text: the speech bubble fades out, the text changes, and it fades back in.
	public void Swap(Action setText, Action landed = null) {
		if (_panel == null) {
			if (setText != null) setText();
			if (landed != null) landed();
			return;
		}
		BubbleOut(0.15f, () => {
			if (setText != null) setText();
			BubbleIn(0f, bubbleDuration * 0.8f, landed);
		});
	}

	// Far enough right that the portrait is clear of the screen edge whatever its pivot.
	Vector2 OffscreenRight() {
		return new Vector2(_imageRt.rect.width + 150f, 0f);
	}

	// Simple scale + fade, no overshoot.
	Tween BubbleIn(float delay, float duration, Action done = null) {
		var t = _panel.transform;
		Tween.Cancel(t, "jiggle");
		t.localRotation = Quaternion.identity;
		t.localScale = _panelRest * BubbleStartScale;
		Tween.Cancel(_panelGroup, "alpha");
		_panelGroup.alpha = 0f;
		Tween.Alpha(_panelGroup, 1f, duration, Tween.Ease.OutQuad, delay);
		return Tween.Scale(t, _panelRest, duration, Tween.Ease.OutQuad, delay, done);
	}

	Tween BubbleOut(float duration, Action done = null) {
		var t = _panel.transform;
		Tween.Cancel(t, "jiggle");
		Tween.Alpha(_panelGroup, 0f, duration, Tween.Ease.InQuad);
		return Tween.Scale(t, _panelRest * BubbleStartScale, duration, Tween.Ease.InQuad, 0f, done);
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
