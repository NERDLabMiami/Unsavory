using UnityEngine;
using UnityEngine.UI;

// "N IN A ROW!" popup: pops in, settles slightly smaller, fades out and removes itself
// (was the Combo UI controller's Combo Text clip).
public class ComboTextAnim : MonoBehaviour {
	const float Total = 0.45f;

	Vector3 _rest = Vector3.one;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_rest = AnimUtil.RestScale(transform);
	}

	// Call after the text has been parented.
	public void Play() {
		var text = GetComponent<Graphic>();
		transform.localScale = Vector3.zero;
		SetAlpha(text, 0f);

		Tween.Run(this, "pop", Total, v => {
			float t = v * Total;
			float s = t < 0.317f ? Mathf.Lerp(0f, 1f, t / 0.317f)
				: t < 0.4f ? Mathf.Lerp(1f, 0.8f, (t - 0.317f) / 0.083f) : 0.8f;
			transform.localScale = _rest * s;
			float a = t < 0.083f ? t / 0.083f : Mathf.Lerp(1f, 0f, (t - 0.083f) / (Total - 0.083f));
			SetAlpha(text, a);
		}, Tween.Ease.Linear, 0f, () => Destroy(gameObject));
	}

	static void SetAlpha(Graphic g, float a) {
		if (g == null) return;
		var c = g.color; c.a = a; g.color = c;
	}
}
