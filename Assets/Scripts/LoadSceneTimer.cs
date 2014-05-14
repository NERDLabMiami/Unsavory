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
		guiTexture.pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);
		texture = new Texture2D(Screen.width, Screen.height, TextureFormat.ARGB32, false);
		guiTexture.texture = texture;
		beginTransition();
		Debug.Log("Load Scene Timer?");
		Time.timeScale = 1f;
	}

	void timesUp() {
		startTime = Time.time;
		switchingScene = false;
		fading = true;
		guiTexture.enabled = true;
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
		guiTexture.color = Color.Lerp (guiTexture.color, Color.black, fadeAmount);
		if (guiTexture.color.a >= 0.95f) {
			Debug.Log("Faded Completely");
			Application.LoadLevel(sceneNumber);
		}
	}


	void Awake() {
	}

	public void beginTransition() {
		switchingScene = true;
	}


}
