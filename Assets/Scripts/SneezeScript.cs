using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SneezeScript : MonoBehaviour {
	public GameObject panEvent;
	public GameObject timer;
	public GameObject continueButton;
	private bool launchedLevelEnd = false;
	private bool endless = false;
	public string retryButtonText;
	public int retrySceneNumber;

	// Use this for initialization
	void Start () {
		if (PlayerPrefs.GetInt("endless") == 1) {
			endless = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!launchedLevelEnd && !endless) {
			if (Camera.main.GetComponent<CameraShakeScript>().sneezed()) {
				PlayerPrefs.SetString ("ENDOFLEVEL_TITLE", "Gross");
				PlayerPrefs.SetString ("ENDOFLEVEL_MESSAGE", "All the food is tainted...");
				launchedLevelEnd = true;
				panEvent.GetComponent<PanPositionScript>().move();
				//pause
				Time.timeScale = 0;
				//account for scaled pay
				int currentLevel = PlayerPrefs.GetInt("current level");
				List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
				float hourlyRate = timer.GetComponent<TimerScript>().hourlyRate;
				int timeWorked = timer.GetComponent<TimerScript>().getTimeWorked();
				wages.Add (hourlyRate * timeWorked);
				PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
				//advance level/day
				PlayerPrefs.SetInt ("current level", currentLevel+1);

			}
		}
		if (endless && !launchedLevelEnd) {
			if (Camera.main.GetComponent<CameraShakeScript>().sneezed()) {
				launchedLevelEnd = true;
				continueButton.GetComponent<TextMesh>().text = retryButtonText;
				continueButton.GetComponent<LoadSceneTimer>().sceneNumber = retrySceneNumber;

				PlayerPrefs.SetString ("ENDOFLEVEL_TITLE", "Gross");
				PlayerPrefs.SetString ("ENDOFLEVEL_MESSAGE", "All the food is tainted...");
				Debug.Log("Calling End of Level");
				timer.GetComponent<TimerScript>().EndOfLevel();
			}
		}
	}
}
