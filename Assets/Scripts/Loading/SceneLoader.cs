using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour {
	private int sceneNumber;
	public float fadeSpeed = 5f;          // Speed that the screen fades to and from black.
	public CanvasGroup canvases;
	private bool sceneStarting = true;      // Whether or not the scene is still fading in.
	private bool sceneEnding = false;


	void Awake ()
	{
		// Set the texture so that it is the the size of the screen and covers it.
		guiTexture.pixelInset = new Rect(0f, 0f, Screen.width, Screen.height);
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		// If the scene is starting...
		if(sceneStarting) {
			// ... call the StartScene function.
			StartScene();
		}
		if (sceneEnding) {
			FadeToBlack();
		}
	}

	void FadeToClear ()
	{
		// Lerp the colour of the texture between itself and transparent.
		guiTexture.color = Color.Lerp(guiTexture.color, Color.clear, fadeSpeed * Time.deltaTime);
	}
	
	
	void FadeToBlack ()
	{
		// Lerp the colour of the texture between itself and black.
		guiTexture.color = Color.Lerp(guiTexture.color, Color.black, fadeSpeed * Time.deltaTime);
		canvases.alpha *= .9f;
			// If the screen is almost black...
		if(guiTexture.color.a >= 0.95f) {
			// ready to load next scene.
			Application.LoadLevel(sceneNumber);
		}

	}
	
	
	void StartScene ()
	{
		// Fade the texture to clear.
		FadeToClear();
		
		// If the texture is almost clear...
		if(guiTexture.color.a <= 0.05f)
		{
			// ... set the colour to clear and disable the GUITexture.
			guiTexture.color = Color.clear;
			guiTexture.enabled = false;
			
			// The scene is no longer starting.
			sceneStarting = false;
		}
	}

	public void RestartGame() {
		string resume = PlayerPrefs.GetString("resume game");
		if (resume.Equals("home")) {
			sceneNumber = 2;

		}

		else if (resume.Equals("game")) {
			sceneNumber = 1;

		}
		else {
			sceneNumber = 1;
		}
	}

	
	public void EndScene ()
	{
		// Make sure the texture is enabled.
		guiTexture.enabled = true;
		sceneEnding = true;
		Time.timeScale = 1.0f;
		Debug.Log("Ending Scene");
	}

	public void QuitInGame() {
		PlayerPrefs.SetString("resume game", "game");
	}

	public void QuitAtHome() {
		PlayerPrefs.SetString("resume game", "home");
	}

	public void setSceneNumber(int sceneNum) {
		sceneNumber = sceneNum;
	}

	public void setEndless(bool endless) {
		if (endless) {
			PlayerPrefs.SetInt("endless", 1);
		}
		else {
			PlayerPrefs.SetInt("endless", 0);
			
		}
	}
}
