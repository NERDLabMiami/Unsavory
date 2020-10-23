using UnityEngine;
using SimpleJSON;

public class StartCareer : MonoBehaviour {
	public Animator mainMenu;
	public int maxWarnings = 3;
	public bool reset = false;
	public int daysUntilComplete = 20;
	// Use this for initialization
	void Start () {

		Time.timeScale = 1f;
		/*
		if (PlayerPrefs.HasKey("can cater")) {
			mainMenu.SetTrigger("catering");
		
		}
		else {
			mainMenu.SetTrigger("no catering");

		}

		*/
		if (PlayerPrefs.HasKey("fired")) {
			CustomFunctionScript.resetPlayerData(60, 0);
			Debug.Log("Resetting Game");
		}
		if (!PlayerPrefs.HasKey("started career")) {
			beginCareer();	
		}
	
		if (PlayerPrefs.HasKey("survived")) {
			//TODO: Beat game, reset levels
		}
		if (reset) {
			PlayerPrefs.DeleteAll();
		}
		if (PlayerPrefs.HasKey("reset") || reset) {
			Debug.Log("Reseting Player");
			CustomFunctionScript.resetPlayerData(60, 0);
			PlayerPrefs.DeleteKey("reset");
		}

		if (reset) {
	
			//			CustomFunctionScript.resetToDefaults();
		}
	}



	public void beginCareer() {
			PlayerPrefs.SetInt("started career", 1);
			PlayerPrefs.SetInt("current level", 1);
			PlayerPrefs.SetInt("max warnings", maxWarnings);
			PlayerPrefs.SetInt("final day", daysUntilComplete);
			PlayerPrefs.SetFloat("health", 60);
			PlayerPrefs.SetFloat ("paycheck", 0);
			PlayerPrefs.SetFloat("earned wages", 0);
			PlayerPrefs.SetFloat("bank account", 0);
			PlayerPrefs.SetFloat("hourly rate", 8.05f);
			PlayerPrefs.SetInt("retries", 3);
		int r = PlayerPrefs.GetInt("retries", -1);
		Debug.Log("RETRIES: " + r);

		//set effect levels

			PlayerPrefs.SetInt("health effect", 0);
			PlayerPrefs.SetInt("late effect", 0);
			PlayerPrefs.SetInt("overtime effect", 0);
			PlayerPrefs.SetInt("electricity effect", 0);

		//read bills JSON
			string billData =  Resources.Load<TextAsset>("bills").ToString();
			JSONNode bills = JSON.Parse(billData);

			for (int i = 0; i < bills["bills"].Count; i++) {
				PlayerPrefs.SetInt("bill" + i,0);
			}
			Debug.Log("Career Started");


	}

	public void unlockCareer() {
		/*
		Social.ReportProgress( "CgkIjOCjtq4PEAIQDA", 100, (result) => {
			Debug.Log ( result ? "Reported First Day" : "Failed to report first day");
		});
*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
