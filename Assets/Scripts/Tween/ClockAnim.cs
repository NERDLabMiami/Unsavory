using UnityEngine;
using UnityEngine.UI;

// HUD clock: the time fades out, changes, and fades back in when an order is finished.
public class ClockAnim : MonoBehaviour {
	const float FadeDuration = 0.35f;

	public const float ChangeDuration = FadeDuration * 2f;

	Text _text;

	public void ChangeTo(string time) {
		if (_text == null) _text = GetComponent<Text>();
		if (_text == null) return;
		if (!gameObject.activeInHierarchy || _text.text == time) { _text.text = time; return; }
		Tween.Alpha(_text, 0f, FadeDuration, Tween.Ease.Linear, 0f, () => {
			if (_text == null) return;
			_text.text = time;
			Tween.Alpha(_text, 1f, FadeDuration, Tween.Ease.Linear);
		});
	}
}
