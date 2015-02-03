using UnityEngine;
using System.Collections;

public class SceneSwitchButtonScript : MonoBehaviour {
	public Transform newCameraPosition;
	private Transform startingPosition;
	private float movementTime;
	private float movementStartTime;
	private bool moving = false;
	private float speed = 1.0f;
	public float timeUntilNextScene = 10;
	public float fadeSpeed = 1.5f;
	private bool sceneStarting = true;
	public int sceneNumber;
	private float currentTime;
	private float startTime;
	private bool switchingScene = false;
	private bool fading = false;
	private float fadingTime = 10;
	
	public void setEndPosition(Transform endPosition) {
		newCameraPosition = endPosition;
	}

	public void beginTransition(int scene) {
		Debug.Log("Switching Scene");
		Time.timeScale = 1.0f;
		startingPosition =  Camera.main.transform;
		moving = true;
		sceneNumber = scene;
		movementStartTime = Time.realtimeSinceStartup;
		movementTime = Vector3.Distance(startingPosition.position, newCameraPosition.position);
		switchingScene = true;
	}

	public void setEndless(bool endless) {
		if (endless) {
			PlayerPrefs.SetInt("endless", 1);
		}
		else {
			PlayerPrefs.SetInt("endless", 0);
			
		}
	}

	void Update() {
		if (moving) {
			float distanceCovered = (Time.realtimeSinceStartup - movementStartTime) * speed;
			float fracJourney = distanceCovered / movementStartTime;
			Camera.main.transform.position = Vector3.Lerp (startingPosition.position, newCameraPosition.position, fracJourney);
			if (Vector3.Distance (newCameraPosition.transform.position, Camera.main.transform.position) <= .1) {
				moving = false;
			}
		}

		if (switchingScene) {
			Debug.Log("Time Left: " + timeUntilNextScene);

			timeUntilNextScene -= Time.deltaTime;
			if (timeUntilNextScene <= 0) {
				Application.LoadLevel(sceneNumber);
//				timesUp();
			}
		}
	

	}

}
