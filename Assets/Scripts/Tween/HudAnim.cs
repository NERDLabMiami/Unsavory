using UnityEngine;
using UnityEngine.UI;

// Top HUD (clock, bank, pills, pause button): slides down and fades in when enabled, and back out on Hide
// (was the HUD controller's HUD Appears / HUD Disappears clips).
public class HudAnim : MonoBehaviour {
	static readonly string[] Parts = { "Pause Button", "Bank Image", "Pills Image", "Money in Bank", "Pills on Hand" };
	const float Duration = 1f;
	const float HiddenY = 350f;

	RectTransform _rect;
	Vector2 _rest;   // where the scene places the HUD

	public static HudAnim Prepare(GameObject go) {
		return go == null ? null : AnimUtil.Get<HudAnim>(go);
	}

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_rect = transform as RectTransform;
		if (_rect != null) _rest = _rect.anchoredPosition;
	}

	void OnEnable() { Slide(0f, 1f); }

	// Slides away and switches the HUD off.
	public void Hide() {
		Slide(HiddenY, 0f, () => gameObject.SetActive(false));
	}

	void Slide(float toY, float toAlpha, System.Action done = null) {
		if (_rect != null) {
			if (toAlpha > 0f) _rect.anchoredPosition = _rest + new Vector2(0f, HiddenY);
			Tween.Anchored(_rect, _rest + new Vector2(0f, toY), Duration, Tween.Ease.Linear, 0f, done);
		}
		foreach (var part in Parts) {
			var t = transform.Find(part);
			var g = t != null ? t.GetComponent<Graphic>() : null;
			if (g == null) continue;
			if (toAlpha > 0f) { var c = g.color; c.a = 0f; g.color = c; }
			Tween.Alpha(g, toAlpha, Duration, Tween.Ease.Linear);
		}
	}
}
