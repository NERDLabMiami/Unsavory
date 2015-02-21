using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SneezeScript : MonoBehaviour {
	public GameObject timer;
	public GameObject player;
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
				PlayerPrefs.SetInt("sneezed", 1);
				//add to the sneeze count
				int sneezes = PlayerPrefs.GetInt ("sneezes") + 1;
				PlayerPrefs.SetInt ("sneezes", sneezes);
				/*
				if (sneezes > player.GetComponent<PlayerScript>().warningsBeforeFired) {
					//maxed out, real game over
					PlayerPrefs.SetInt("fired", 1);
				}
*/
				launchedLevelEnd = true;
				//pause
				Time.timeScale = 0;
				player.GetComponent<PlayerScript>().addWages( timer.GetComponent<TimerScript>().getTimeWorked());
				player.GetComponent<PlayerScript>().EndOfLevel(false, false, true);

			}
		}

		if (endless && !launchedLevelEnd) {
			if (Camera.main.GetComponent<CameraShakeScript>().sneezed()) {
				//END OF LEVEL POINT, NO STORY MODE
				launchedLevelEnd = true;
				PlayerPrefs.SetString ("ENDOFLEVEL_TITLE", "Gross");
				PlayerPrefs.SetString ("ENDOFLEVEL_MESSAGE", "All the food is tainted...");
				Debug.Log("Calling End of Level - not end of day");
				player.GetComponent<PlayerScript>().EndOfLevel(false, true, true);
			}
		}
	}




}
