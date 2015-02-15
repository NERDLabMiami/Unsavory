using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;
public class StartCareer : MonoBehaviour {
	public Text startButton;
	// Use this for initialization
	void Start () {
		beginCareer();	
		displayDueDates();
	
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
		if (!PlayerPrefs.HasKey("started career")) {
			//TODO: Remove Started Career Key on Game Over
			PlayerPrefs.SetInt("started career", 1);
			PlayerPrefs.SetInt("current level", 1);
			//read bills JSON
			string billData =  Resources.Load<TextAsset>("bills").ToString();
			JSONNode bills = JSON.Parse(billData);
			List<int> dueDates = new List<int>();

			for (int i = 0; i < bills["bills"].Count; i++) {
				int startDueDate = bills["bills"][i]["delay"].AsInt;
				int finalDueDate = 20;
				int dueDate = Random.Range(startDueDate, finalDueDate);
				dueDates.Add(dueDate);
				Debug.Log("Added due date of " + dueDate);
			}
			PlayerPrefsX.SetIntArray("due", dueDates.ToArray());
			Debug.Log("Career Started");
		}
		else {
			startButton.text = "Get Back to Work";
			Debug.Log("Career Already Started");
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
