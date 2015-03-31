using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour {
	public bool isPaused = false;
	public Button pauseButton;
	public GameObject pausedCanvas;
	public GameObject gameElements;
	public Text clock;
	public MusicLibrary music;
	//TODO: Check if gameplaystarted varaible can be removed
	public bool gamePlayStarted = false;
	public Sprite pauseImage;
	public Sprite playImage;

	public void pause() {
		if (!isPaused) {
			if (gameElements != null) {
				foreach(BoxCollider2D child in gameElements.GetComponentsInChildren<BoxCollider2D>()) {
//					child.enabled = false;
				}
			}

			Camera.main.GetComponent<AudioSource>().Stop();
			Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
			GetComponentInParent<TimerScript>().running = false;
			Debug.Log("Pausing");
			Time.timeScale = 0.0f;
			pausedCanvas.SetActive(true);
			isPaused = true;
			pauseButton.image.sprite = playImage;
		}
		else {
			if (gameElements != null) {
				foreach(BoxCollider2D child in gameElements.GetComponentsInChildren<BoxCollider2D>()) {
//					child.enabled = true;
				}
			}

			pauseButton.image.sprite = pauseImage;
			Debug.Log("Resuming");
			music.unpause();
			Camera.main.GetComponent<AudioSource>().Play();
			Camera.main.GetComponent<CameraShakeScript>().resumeSneezing();
			GetComponentInParent<TimerScript>().running = true;
			Time.timeScale = 1.0f;
			isPaused = false;
			pausedCanvas.SetActive(false);
		}


	}

	void Update() {
	if (!isPaused && gamePlayStarted) {
			clock.text = GetComponent<TimerScript>().getClock();
		}
	}

}
