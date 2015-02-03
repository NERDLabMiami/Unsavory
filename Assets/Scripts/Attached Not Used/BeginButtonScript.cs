using UnityEngine;
using System.Collections;

public class BeginButtonScript : MonoBehaviour {
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
	private Texture2D texture;
	private bool fading = false;
	private float fadingTime = 10;
	
	public void beginTransition() {
		startingPosition =  Camera.main.transform;
		moving = true;
		movementStartTime = Time.realtimeSinceStartup;
		movementTime = Vector3.Distance(startingPosition.position, newCameraPosition.position);
		Time.timeScale = 1f;
		switchingScene = true;
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
			timeUntilNextScene -= Time.deltaTime;
			if (timeUntilNextScene <= 0) {
				timesUp();
			}
		}
		
		if (fading) {
			fadeToBlack();
		}
		
		
	}
	
	void timesUp() {
		startTime = Time.time;
		switchingScene = false;
		fading = true;
		guiTexture.enabled = true;
	}


	void fadeToBlack() {
		float fadeTime = (Time.time - startTime) * fadeSpeed;
		float fadeAmount = fadeTime / fadingTime;
		guiTexture.color = Color.Lerp (guiTexture.color, Color.black, fadeAmount);
		if (guiTexture.color.a >= 0.95f) {
			Application.LoadLevel(sceneNumber);
		}
	}
	// Use this for initialization
	void Awake () {
		if (PlayerPrefs.GetInt("current level") > 1) {
			Debug.Log("CURRENT LEVEL: " + PlayerPrefs.GetInt("current level"));
			GetComponent<TextMesh>().text = "continue";
		}
	}

}
