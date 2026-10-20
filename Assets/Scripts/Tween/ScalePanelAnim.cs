using System;
using UnityEngine;
using UnityEngine.UI;

// Pop-in / pop-out for panels that were driven by a pure localScale clip
// (Tutor Panel, Continue Tutorial Panel). Replaces the Animator on the same object.
public class ScalePanelAnim : MonoBehaviour {
	public float showDuration = 1f / 6f;
	public float hideDuration = 1f / 6f;

	Vector3 _rest = Vector3.one;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_rest = AnimUtil.RestScale(transform);
	}

	// Grows from nothing to the size authored in the scene. Starting from zero matches the old clips.
	public Tween Show(Action done = null) {
		transform.localScale = Vector3.zero;
		// The old Tutor Appears clip faded the label up from 0, so the scene may have it saved nearly transparent.
		foreach (var t in GetComponentsInChildren<Text>(true)) {
			var c = t.color; c.a = 1f; t.color = c;
		}
		return Tween.Scale(transform, _rest, showDuration, Tween.Ease.Linear, 0f, done);
	}

	// Shrinks away. Pass a duration to use a slower exit than the default.
	public Tween Hide(Action done = null, float? duration = null) {
		return Tween.Scale(transform, Vector3.zero, duration ?? hideDuration, Tween.Ease.Linear, 0f, done);
	}
}
