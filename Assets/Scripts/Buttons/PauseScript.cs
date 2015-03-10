using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour {
	public bool isPaused = false;
	public Button pauseButton;
	public Canvas pauseCanvas;
	public Animator panelAnimator;
	public Text clock;
	public MusicLibrary music;
	public bool gamePlayStarted = false;
	public Sprite pauseImage;
	public Sprite playImage;

	public void pause() {
		if (!isPaused) {
			music.pause();
			Camera.main.audio.Stop();
			Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
			GetComponentInParent<TimerScript>().running = false;
			Debug.Log("Pausing");
			Time.timeScale = 0.0f;
			isPaused = true;
			pauseButton.image.sprite = playImage;
//			pauseText.text = "Resume";

			panelAnimator.enabled = true;
			pauseCanvas.enabled = true;
			panelAnimator.Play("Pause Panel Pop");

		}
		else {
			pauseButton.image.sprite = pauseImage;
			Debug.Log("Resuming");
			music.unpause();
			Camera.main.audio.Play();
			Camera.main.GetComponent<CameraShakeScript>().resumeSneezing();
			GetComponentInParent<TimerScript>().running = true;
			Time.timeScale = 1.0f;
//			pauseText.text = "Pause";
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
