using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SneezeTutor : MonoBehaviour {

	// Shows the hint for a few seconds, then dismisses itself. No button to press.
	void Start () {
		Time.timeScale = 0f;

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
		var anim = GetComponent<Animator>();
		if (anim) anim.SetTrigger("disappear");   // its last frame calls EndSneezeTutorial
		else EndSneezeTutorial();
	}

	void EndSneezeTutorial() {
		Time.timeScale = 1.0f;
		Destroy (this.gameObject);
	}
}
