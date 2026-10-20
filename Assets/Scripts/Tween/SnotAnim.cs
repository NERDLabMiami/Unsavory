using UnityEngine;
using UnityEngine.UI;

// Snot splat over the screen after a sneeze: swells in with a shake that settles, fading up
// (was the Snot All Over controller's Snot clip, its default state, so it plays whenever the object is enabled).
public class SnotAnim : MonoBehaviour {
	Vector3 _rest = Vector3.one;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_rest = AnimUtil.RestScale(transform);
	}

	void OnEnable() {
		var graphic = GetComponent<Graphic>();
		var sprite = GetComponent<SpriteRenderer>();
		Tween.Run(this, "snot", 1f, v => {
			float s = v < 0.833f ? Mathf.Lerp(0f, 1.1f, v / 0.833f) : Mathf.Lerp(1.1f, 1f, (v - 0.833f) / 0.167f);
			transform.localScale = _rest * s;
			transform.localRotation = Quaternion.Euler(0f, 0f, 20f * (1f - v) * Mathf.Cos(v * 25f));
			if (graphic != null) { var c = graphic.color; c.a = v; graphic.color = c; }
			if (sprite != null) { var c = sprite.color; c.a = v; sprite.color = c; }
		}, Tween.Ease.Linear);
	}
}
