using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;
public class StartCareer : MonoBehaviour {
	public Text startButton;
	public GameObject cateringButton;
	public int maxWarnings = 3;
	public bool reset = false;
	public int daysUntilComplete = 30;
	// Use this for initialization
	void Start () {
		Time.timeScale = 1f;
		if (PlayerPrefs.HasKey("can cater")) {
			cateringButton.SetActive(true);
		}

		if (PlayerPrefs.HasKey("fired")) {
			CustomFunctionScript.resetPlayerData(20, 0);
			Debug.Log("Resetting Game");
		}
		if (!PlayerPrefs.HasKey("started career")) {
			beginCareer();	
		}
	
		if (PlayerPrefs.HasKey("survived")) {
			//TODO: Beat game, reset levels
		}
		if (PlayerPrefs.HasKey("reset")) {
			CustomFunctionScript.resetPlayerData(20, 0);
			PlayerPrefs.DeleteKey("reset");
		}

		if (reset) {
			CustomFunctionScript.resetToDefaults();
		}
	}

	public void beginCareer() {
			PlayerPrefs.SetInt("started career", 1);
			PlayerPrefs.SetInt("current level", 1);
			PlayerPrefs.SetInt("max warnings", maxWarnings);
			PlayerPrefs.SetInt("final day", daysUntilComplete);
			PlayerPrefs.SetFloat("health", 20);
			PlayerPrefs.SetFloat ("paycheck", 0);
			PlayerPrefs.SetFloat("earned wages", 0);
			PlayerPrefs.SetFloat("bank account", 0);
			PlayerPrefs.SetFloat("hourly rate", 8.05f);

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
	
	// Update is called once per frame
	void Update () {
	
	}
}
