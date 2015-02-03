using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour {
	public bool isPaused = false;
	public Text pauseText;
	public Canvas pauseCanvas;
	public Animator panelAnimator;
	public Text clock;
	public bool gamePlayStarted = false;

	public void pause() {
		if (!isPaused) {
			Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
			GetComponentInParent<TimerScript>().running = false;
			Debug.Log("Pausing");
			Time.timeScale = 0.0f;
			isPaused = true;
			pauseText.text = "Resume";
			panelAnimator.enabled = true;
			pauseCanvas.enabled = true;
			panelAnimator.Play("Pause Panel Pop");

		}
		else {
			Debug.Log("Resuming");
			Camera.main.GetComponent<CameraShakeScript>().resumeSneezing();
			GetComponentInParent<TimerScript>().running = true;
			Time.timeScale = 1.0f;
			pauseText.text = "Pause";
			isPaused = false;
			panelAnimator.Play("Pause Panel Out");
		}
//		pauseCanvas.enabled = isPaused;


	}

	void Update() {
	if (!isPaused && gamePlayStarted) {
//			gameObject.GetComponent<TextMesh>().text = GetComponent<TimerScript>().getClock();
			//gameObject.GetComponent<TextMesh>().text = timer.GetComponent<TimerScript>().getClock();
			clock.text = GetComponent<TimerScript>().getClock();
//			GetComponent<TextMesh>().text = GetComponent<TimerScript>().getClock();
		}
	}

}
