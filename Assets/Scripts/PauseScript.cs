using UnityEngine;
using System.Collections;

public class PauseScript : MonoBehaviour {
	public bool isPaused = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseDown() {
		if (!isPaused) {
			Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
			Debug.Log("Pausing");
			Time.timeScale = 0.0f;
		}
		else {
			Debug.Log("Resuming");
			Camera.main.GetComponent<CameraShakeScript>().resumeSneezing();
			Time.timeScale = 1.0f;
		}
	}
}
