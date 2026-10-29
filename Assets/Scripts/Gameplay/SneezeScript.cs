using UnityEngine;

public class SneezeScript : MonoBehaviour {
	public TimerScript timer;
	public Level level;
	private bool launchedLevelEnd = false;
	private bool endless = false;


	// Use this for initialization
	void Start () {
		if (PlayerPrefs.HasKey("catering")) {
			endless = true;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!launchedLevelEnd && !endless) {
			if (Camera.main.GetComponent<CameraShakeScript>().sneezed()) {
				PlayerPrefs.SetInt("sneezed", 1);
				//ALLOW SICK DAYS
				PlayerPrefs.SetInt("paid sick days achieved",1);

				//add to the sneeze count
				int sneezes = PlayerPrefs.GetInt ("sneezes") + 1;
				PlayerPrefs.SetInt ("sneezes", sneezes);
				level.setTimeWorked(timer.getTimeWorked());
				launchedLevelEnd = true;
				timer.GetComponent<TimerScript>().running = false;
				level.setTimeWorked(timer.getTimeWorked());
				level.BeginSneezeAftermath();

			}
		}

		if (endless && !launchedLevelEnd) {
			if (Camera.main.GetComponent<CameraShakeScript>().sneezed()) {
				//END OF LEVEL POINT, NO STORY MODE
				launchedLevelEnd = true;
			}
		}
	}




}
