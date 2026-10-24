using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PauseScript : MonoBehaviour {
	public bool isPaused = false;
	public Button pauseButton;
	public GameObject pausedCanvas;
	public GameObject gameElements;
	public Text clock;
	Level level;
	public MusicLibrary music;
	//TODO: Check if gameplaystarted varaible can be removed
	public bool gamePlayStarted = false;
	public Sprite pauseImage;
	public Sprite playImage;

	void Start() {
		level = FindFirstObjectByType<Level>();
		// The pause panel pops in each time it is enabled.
		PanelPopAnim.Prepare(pausedCanvas, "", 0f, 0.5f, true);
	}

	public void pause() {
		if (!isPaused) {
			if (clock != null) clock.text = GetComponentInParent<TimerScript>().getClock();
			Camera.main.GetComponent<AudioSource>().Pause();
			Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
			GetComponentInParent<TimerScript>().running = false;
			Debug.Log("Pausing");
			Time.timeScale = 0.0f;
			pausedCanvas.SetActive(true);
			isPaused = true;
			pauseButton.image.sprite = playImage;
		}
		else {
			pauseButton.image.sprite = pauseImage;
			Debug.Log("Resuming");
			music.unpause();
			Camera.main.GetComponent<AudioSource>().Play();
			Camera.main.GetComponent<CameraShakeScript>().resumeSneezing();
			GetComponentInParent<TimerScript>().running = true;
			Time.timeScale = 1.0f;
			isPaused = false;
			pausedCanvas.SetActive(false);
			// "Pause Panel Pop" locks the gate; the matching "out" clip never plays because the panel is just deactivated.
			if (GameplayGate.Instance != null) GameplayGate.Instance.Unlock("AnimationFunctions");
		}


	}

	void Update() {
		var input = GameInput.Instance;
		if (input == null) return;

		// Start / Esc / P toggles pause; while paused, Cancel (B / Esc) also resumes.
		// One pause() call per frame so Esc (bound to both) can't resume and re-pause.
		// Pausing is only allowed when the HUD pause button is (not during tutorial/countdown) and no modal is up.
		bool canPause = gamePlayStarted && pauseButton != null && pauseButton.enabled && !ModalFocus.HasFocus;
		if (isPaused ? (input.PausePressed || input.CancelPressed) : (input.PausePressed && canPause))
			pause();
	}

}
