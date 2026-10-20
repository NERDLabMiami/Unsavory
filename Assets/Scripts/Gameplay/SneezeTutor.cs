using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SneezeTutor : MonoBehaviour {
	Vector3 _rest = Vector3.one;

	// Shows the hint for a few seconds, then dismisses itself. No button to press.
	void Start () {
		Time.timeScale = 0f;

		// Pop in (was the Sneeze Tutor Appears clip).
		AnimUtil.DisableAnimator(gameObject);
		_rest = AnimUtil.RestScale(transform);
		transform.localScale = Vector3.zero;
		Tween.Scale(transform, _rest * 1.1f, 1f / 12f, Tween.Ease.Linear, 0f, () =>
			Tween.Scale(transform, _rest, 1f / 12f, Tween.Ease.Linear));

		foreach (var b in GetComponentsInChildren<Button>(true)) b.gameObject.SetActive(false);
		var modal = GetComponent<ModalFocus>();
		if (modal) modal.enabled = false;
		foreach (var t in GetComponentsInChildren<Text>(true))
			if (t.text.Contains("about to sneeze"))
				t.text = "You're about to sneeze and contaminate the food! Press LB (or Q) to wipe your nose.";

		StartCoroutine(AutoDismiss());
	}

	IEnumerator AutoDismiss() {
		yield return new WaitForSecondsRealtime(5f);
		// Swell slightly, then shrink away (was the Sneeze Tutor Disappears clip).
		AnimUtil.DisableAnimator(gameObject);
		Tween.Scale(transform, _rest * 1.1f, 1f / 12f, Tween.Ease.Linear, 0f, () =>
			Tween.Scale(transform, Vector3.zero, 5f / 12f, Tween.Ease.Linear, 0f, EndSneezeTutorial));
	}

	void EndSneezeTutorial() {
		Time.timeScale = 1.0f;
		Destroy (this.gameObject);
	}
}
