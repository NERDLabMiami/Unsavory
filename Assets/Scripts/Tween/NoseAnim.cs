using System;
using UnityEngine;

// Nose twitch while the sneeze builds, and the finger that swipes across to wipe it
// (was the Nose controller's Nose Twitch / Wipe Nose clips). Children are found by the paths the clips used.
public class NoseAnim : MonoBehaviour {
	// The nose pulses up to this multiple of the scale it has in the scene (was 0.5 -> 0.6 / 0.585).
	static readonly Vector3 TwitchFactor = new Vector3(1.2f, 1.17f, 1f);

	// Where along its start->nose path the wiping finger stops (was 13.35 -> 2.5 in local units).
	[Tooltip("Fraction of the way from the finger's scene position toward the middle where the swipe stops.")]
	public float swipeEnd = 0.187f;

	Transform _nose;
	SpriteRenderer _noseSprite, _left, _right;
	Vector3 _noseScale, _noseTwitchScale, _leftStart, _rightStart;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		_nose = transform.Find("nose");
		if (_nose != null) {
			_noseSprite = _nose.GetComponent<SpriteRenderer>();
			_noseScale = _nose.localScale;   // as authored in the scene
			_noseTwitchScale = Vector3.Scale(_noseScale, TwitchFactor);
		}
		_left = Sprite("Left Finger");
		_right = Sprite("Right Finger");
		// The fingers wait off-screen where the scene puts them; the swipe starts there.
		if (_left != null) _leftStart = _left.transform.localPosition;
		if (_right != null) _rightStart = _right.transform.localPosition;
		SetAlpha(_left, 0f);
		SetAlpha(_right, 0f);
	}

	SpriteRenderer Sprite(string n) {
		var t = transform.Find(n);
		return t != null ? t.GetComponent<SpriteRenderer>() : null;
	}

	// Pulses until the nose is wiped. onFirstPulse fires once, after the first pulse.
	public void Twitch(Action onFirstPulse) {
		if (_nose == null) return;
		Tween.Run(this, "twitch", 2f / 3f, v => {
			float k = 1f - Mathf.Abs(2f * v - 1f);
			_nose.localScale = Vector3.Lerp(_noseScale, _noseTwitchScale, k);
			if (_noseSprite != null) { var c = _noseSprite.color; c.a = Mathf.Lerp(1f, 0.7f, k); _noseSprite.color = c; }
		}, Tween.Ease.Linear).Loop(0);

		if (onFirstPulse != null)
			Tween.Run(this, "twitchEvent", 2f / 3f, null, Tween.Ease.Linear, 0f, onFirstPulse);
	}

	public void Wipe(bool left) {
		Tween.Cancel(this, "twitch");
		Tween.Cancel(this, "twitchEvent");
		if (_nose != null) _nose.localScale = _noseScale;
		SetAlpha(_noseSprite, 1f);

		var finger = left ? _left : _right;
		var other = left ? _right : _left;
		if (other != null) { Tween.Cancel(other, "wipe"); SetAlpha(other, 0f); }
		if (finger == null) return;

		var start = left ? _leftStart : _rightStart;
		var mid = new Vector3(start.x * swipeEnd, start.y, start.z);
		Tween.Run(finger, "wipe", 1f, v => {
			float k = v < 0.5f ? v * 2f : (1f - v) * 2f;
			k = k * k * (3f - 2f * k);
			finger.transform.localPosition = Vector3.Lerp(start, mid, k);
			float a = v < 0.033f ? v / 0.033f : v > 0.95f ? (1f - v) / 0.05f : 1f;
			SetAlpha(finger, a);
		}, Tween.Ease.Linear, 0f, () => SetAlpha(finger, 0f));
	}

	static void SetAlpha(SpriteRenderer s, float a) {
		if (s == null) return;
		var c = s.color; c.a = a; s.color = c;
	}
}
