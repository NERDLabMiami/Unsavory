using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;
public class StartCareer : MonoBehaviour {
	public Text startButton;
	public int maxWarnings = 3;
	public int daysUntilComplete = 30;
	// Use this for initialization
	void Start () {
		if (PlayerPrefs.HasKey("fired")) {
			CustomFunctionScript.resetPlayerData(30, 0);
			Debug.Log("Resetting Game");
		}
		if (!PlayerPrefs.HasKey("started career")) {
			beginCareer();	
		}

//		displayDueDates();
	
	}

	public void displayDueDates() {
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		int[] dueDates = PlayerPrefsX.GetIntArray("due");
		for (int i = 0; i < bills["bills"].Count; i++) {
			Debug.Log(bills["bills"][i]["title"] + ": " + bills["bills"][i]["amount"] + " due in " + dueDates[i] + " days");
		}
	}


	public void beginCareer() {
			PlayerPrefs.SetInt("started career", 1);
			PlayerPrefs.SetInt("current level", 1);
			PlayerPrefs.SetInt("max warnings", maxWarnings);
			PlayerPrefs.SetInt("final day", daysUntilComplete);
			//read bills JSON
			string billData =  Resources.Load<TextAsset>("bills").ToString();
			JSONNode bills = JSON.Parse(billData);
			List<int> dueDates = new List<int>();

			for (int i = 0; i < bills["bills"].Count; i++) {
				int startDueDate = bills["bills"][i]["delay"].AsInt;
				int finalDueDate = daysUntilComplete;
				int dueDate = Random.Range(startDueDate, finalDueDate);
				dueDates.Add(dueDate);
				Debug.Log("Added due date of " + dueDate);
			}
			PlayerPrefsX.SetIntArray("due", dueDates.ToArray());
			Debug.Log("Career Started");

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
