using UnityEngine;
using System.Collections;

public class LoadSceneTimer : MonoBehaviour {
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


	void OnMouseDown() {
		GetComponent<GUITexture>().pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);
		texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
		GetComponent<GUITexture>().texture = texture;
		beginTransition();
		Time.timeScale = 1f;
	}
	
	void timesUp() {
		startTime = Time.time;
		switchingScene = false;
		fading = true;
		GetComponent<GUITexture>().enabled = true;
	}

	void Update () {

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
	
	void fadeToBlack() {
		float fadeTime = (Time.time - startTime) * fadeSpeed;
		float fadeAmount = fadeTime / fadingTime;
		GetComponent<GUITexture>().color = Color.Lerp (GetComponent<GUITexture>().color, Color.black, fadeAmount);
		if (GetComponent<GUITexture>().color.a >= 0.95f) {
			Application.LoadLevel(sceneNumber);
		}
	}


	void Awake() {
	}

	public void beginTransition() {
		switchingScene = true;
	}


}
