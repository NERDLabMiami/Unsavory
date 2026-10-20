using UnityEngine;

// The pre-game countdown word: pops and squashes each time the word changes, then the text clears at "Get to work."
// (was the Panel WTF controller's Wobble / Disappear clips). The old clips also locked the gameplay gate while
// the countdown ran; that is kept for objects that carry an AnimationFunctions.
public class CountdownWordAnim : MonoBehaviour {
	Vector3 _rest = Vector3.one, _textRest = Vector3.one;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_rest = AnimUtil.RestScale(transform);
		var text = transform.Find("Text");
		if (text != null) _textRest = AnimUtil.RestScale(text);
	}

	public void Wobble() {
		var text = transform.Find("Text");
		if (text != null) text.localScale = _textRest;

		var af = GetComponent<AnimationFunctions>();
		if (af != null) af.disableCollisionOnObjects();

		Tween.Run(this, "wobble", 1f, v => {
			float t = v;   // seconds, since the clip is 1s long
			float x, y;
			if (t < 0.15f) { x = y = Mathf.Lerp(0.5f, 1f, t / 0.15f); }
			else if (t < 0.2f) { float k = (t - 0.15f) / 0.05f; x = Mathf.Lerp(1f, 0.988f, k); y = Mathf.Lerp(1f, 0.7f, k); }
			else if (t < 0.417f) { float k = (t - 0.2f) / 0.217f; x = Mathf.Lerp(0.988f, 0.5f, k); y = Mathf.Lerp(0.7f, 0.5f, k); }
			else { float k = (t - 0.417f) / 0.583f; x = y = Mathf.Lerp(0.5f, 0f, k); }
			transform.localScale = Vector3.Scale(_rest, new Vector3(x, y, Mathf.Min(x, y)));
			transform.localRotation = Quaternion.Euler(0f, 0f, 0.4f * Mathf.Sin(t * 40f) * (1f - t));
		}, Tween.Ease.Linear);
	}

	// Last word: text goes away, then the gate opens.
	public void Finished() {
		Tween.Cancel(this, "wobble");
		transform.localScale = _rest;
		transform.localRotation = Quaternion.identity;
		var text = transform.Find("Text");
		if (text != null) text.localScale = Vector3.zero;

		var af = GetComponent<AnimationFunctions>();
		if (af != null) Tween.Delay(0.5f, af.enableCollisionOnObjects);
	}
}
