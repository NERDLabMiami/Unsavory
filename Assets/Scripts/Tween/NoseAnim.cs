using System;
using UnityEngine;

// Nose twitch while the sneeze builds (was the Nose controller's Nose Twitch clip). The wipe itself is the hand pose
// (HandPoseController). Children are found by the paths the clips used.
public class NoseAnim : MonoBehaviour {
	// The nose pulses up to this multiple of the scale it has in the scene (was 0.5 -> 0.6 / 0.585).
	static readonly Vector3 TwitchFactor = new Vector3(1.2f, 1.17f, 1f);

	Transform _nose;
	SpriteRenderer _noseSprite;
	Vector3 _noseScale, _noseTwitchScale, _noseRestPos;
	float _noseAlpha = 1f;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		// The Player prefab's script sits on the Player root with a 'Nose' child; the old Nose prefab had a 'nose' child.
		_nose = transform.Find("nose") ?? transform.Find("Nose");
		if (_nose == null && GetComponent<SpriteRenderer>() != null) _nose = transform;   // the sprite itself
		if (_nose == null) Debug.LogWarning("[NoseAnim] no nose sprite found under " + name + ": no twitch or wipe dip.", this);
		if (_nose != null) {
			_noseRestPos = _nose.localPosition;
			_noseSprite = _nose.GetComponent<SpriteRenderer>();
			if (_noseSprite != null) _noseAlpha = _noseSprite.color.a;   // as authored
			_noseScale = _nose.localScale;   // as authored in the scene
			_noseTwitchScale = Vector3.Scale(_noseScale, TwitchFactor);
		}
	}

	// Pulses until the nose is wiped. onFirstPulse fires once, after the first pulse.
	public void Twitch(Action onFirstPulse) {
		// Without a 'nose' child there is nothing to pulse, but the tutorial prompt still follows the first pulse.
		if (_nose != null)
			Tween.Run(this, "twitch", 2f / 3f, v => {
				float k = 1f - Mathf.Abs(2f * v - 1f);
				_nose.localScale = Vector3.Lerp(_noseScale, _noseTwitchScale, k);
				if (_noseSprite != null) { var c = _noseSprite.color; c.a = Mathf.Lerp(_noseAlpha, _noseAlpha * 0.7f, k); _noseSprite.color = c; }
			}, Tween.Ease.Linear).Loop(0);

		if (onFirstPulse != null)
			Tween.Run(this, "twitchEvent", 2f / 3f, null, Tween.Ease.Linear, 0f, onFirstPulse);
	}

	// Ends the twitch and puts the nose back at its rest scale and colour (a wipe, or the day ending mid-sneeze).
	public void StopTwitch() {
		Tween.Cancel(this, "twitch");
		Tween.Cancel(this, "twitchEvent");
		if (_nose != null) _nose.localScale = _noseScale;
		SetAlpha(_noseSprite, _noseAlpha);
	}

	// The nose comes down to the fingertip as the hand rises, stays for the swipe, then returns to its rest position.
	public void Drop() {
		if (_nose == null) return;
		var hands = FindFirstObjectByType<HandPoseController>();
		if (hands == null || !hands.TryGetWipeContact(out var tip, out float rise, out float swipe, out float back)) return;
		float y = _nose.parent ? _nose.parent.InverseTransformPoint(tip).y : tip.y;
		var down = new Vector3(_noseRestPos.x, Mathf.Min(_noseRestPos.y, y), _noseRestPos.z);   // only ever dips south
		Tween.MoveLocal(_nose, down, rise, Tween.Ease.OutQuad, 0f,
			() => Tween.MoveLocal(_nose, _noseRestPos, back, Tween.Ease.OutQuad, swipe));
	}

	// Cancels a drop in progress and puts the nose back at rest (the day ending mid-wipe).
	public void ResetPosition() {
		if (_nose == null) return;
		Tween.Cancel(_nose, "pos");
		_nose.localPosition = _noseRestPos;
	}

	static void SetAlpha(SpriteRenderer s, float a) {
		if (s == null) return;
		var c = s.color; c.a = a; s.color = c;
	}
}
