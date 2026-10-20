using UnityEngine;

// Order card feedback: time-running-out wobble, the pop-out when served, and the shake and drop when thrown away
// (was the Recipe controller's Order Time Running Out / Unmatched Order clips). Entrances are not animated here:
// new cards slide in from the right through OrderQueueLayout2D.
public class RecipeAnim : MonoBehaviour {
	SpriteRenderer _sprite;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		var t = transform.Find("Sprite");
		_sprite = t != null ? t.GetComponent<SpriteRenderer>() : null;
	}

	// Loops until the card is served, discarded or destroyed.
	public void Alert() {
		Tween.Run(this, "alert", 1f / 3f, v => {
			transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(v * Mathf.PI * 4f));
			if (_sprite != null) {
				var c = _sprite.color; c.a = Mathf.Lerp(1f, 0.5f, 1f - Mathf.Abs(2f * v - 1f)); _sprite.color = c;
			}
		}, Tween.Ease.Linear).Loop(0);
	}

	void StopAlert() {
		Tween.Cancel(this, "alert");
		transform.localRotation = Quaternion.identity;
		if (_sprite != null) { var c = _sprite.color; c.a = 1f; _sprite.color = c; }
	}

	// Served: the card pops up, then shrinks away completely.
	public void PopOut() {
		StopAlert();
		Vector3 rest = transform.localScale;
		Tween.Scale(transform, rest * 1.2f, 0.1f, Tween.Ease.OutBack, 0f,
			() => { if (this != null) Tween.Scale(transform, Vector3.zero, 0.2f, Tween.Ease.InQuad); });
		FadeAll(0f, 0.2f, 0.1f);
	}

	// Thrown away (wrong ingredient or out of time): a quick shake, then it drops and fades with everything on it.
	public void Unmatched() {
		StopAlert();
		Tween.Run(this, "shake", 0.1f, v => transform.localRotation = Quaternion.Euler(0f, 0f, 3f * Mathf.Sin(v * Mathf.PI * 4f) * (1f - v)),
			Tween.Ease.Linear, 0f, () => { if (this != null) transform.localRotation = Quaternion.identity; });
		Tween.Move(transform, transform.position + new Vector3(0f, -0.6f, 0f), 0.3f, Tween.Ease.InQuad, 0.1f);
		FadeAll(0f, 0.3f, 0.1f);
	}

	// Fades every sprite on the card, so the ingredient icons go with the card instead of being left behind.
	void FadeAll(float to, float time, float delay) {
		foreach (var s in GetComponentsInChildren<SpriteRenderer>(true)) {
			Tween.Cancel(s, "alpha");
			Tween.Alpha(s, to, time, Tween.Ease.Linear, delay);
		}
	}
}
