using UnityEngine;
using UnityEngine.UI;

// "Wrong order" strike indicator: the Nth marker pops in, the whole panel then shrinks away and removes itself
// (was the Bad Order Panel controller's Wrong Order - 1/2/3 clips).
public class WarningPanelAnim : MonoBehaviour {
	static readonly string[] Markers = { "One", "Two", "Three" };

	Vector3 _rest = Vector3.one;
	readonly Vector3[] _markerRest = { Vector3.one, Vector3.one, Vector3.one };

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_rest = AnimUtil.RestScale(transform);
		for (int i = 0; i < Markers.Length; i++) {
			var m = transform.Find(Markers[i] + "/Image");
			if (m != null) _markerRest[i] = AnimUtil.RestScale(m);
		}
	}

	public void Show(int warningNumber) {
		int n = Mathf.Clamp(warningNumber, 1, Markers.Length);

		for (int i = 0; i < Markers.Length; i++) {
			var t = transform.Find(Markers[i] + "/Image");
			if (t == null) continue;
			var g = t.GetComponent<Graphic>();
			if (g == null) continue;

			if (i + 1 < n) { SetAlpha(g, 1f); t.localScale = _markerRest[i]; }
			else if (i + 1 > n) SetAlpha(g, 0f);
			else {
				SetAlpha(g, 0f);
				t.localScale = Vector3.zero;
				float fade = n == 3 ? 0.25f : 1f / 3f;
				Tween.Alpha(g, 1f, fade, Tween.Ease.Linear);
				Tween.Scale(t, _markerRest[i], 1f / 3f, Tween.Ease.Linear);
			}
		}

		transform.localScale = _rest;
		Tween.Scale(transform, Vector3.zero, 1.5f - 0.833f, Tween.Ease.Linear, 0.833f, () => Destroy(gameObject));
	}

	static void SetAlpha(Graphic g, float a) {
		Tween.Cancel(g, "alpha");
		var c = g.color; c.a = a; g.color = c;
	}
}
