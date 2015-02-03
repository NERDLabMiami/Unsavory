using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class ReturnHomeScript : MonoBehaviour {
	public GameObject phone;
	public Canvas check;
	private bool playerWasFired = false;
	public bool electricity = true;

	// Use this for initialization
	void Start () {
		bool weekend = false;
		int day = PlayerPrefs.GetInt("current level");
		float[] wages = PlayerPrefsX.GetFloatArray("wages");
		if (PlayerPrefs.HasKey ("fired")) {
			playerWasFired = true;
			Debug.Log("Fired...");
			//TODO: Need to reveal to the player
			CustomFunctionScript.resetPlayerData(100,0);
		}
		else  {
			Debug.Log("It's day " + day);
			if (day % 5 == 0) {
				//Pay Wages, Add Notification for Bill
				weekend = true;
				buyGroceries();
				Debug.Log("It's the weekend");
				string daysWorked = "";
				if (wages.Length >= day) {
					for (int i = day; i > day - 5; i--) {
						daysWorked += "$" + wages[i-1].ToString() + "\n";
						Debug.Log ("Pay for " + wages[i-1]);
					}
					GetComponent<TextMesh>().text = daysWorked;
				}
			}
			//advance the day now that you're home.
			PlayerPrefs.SetInt ("current level", day+1);
		}

	}
	

	void buyGroceries() {
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		int money = PlayerPrefs.GetInt ("money");
		int costOfGroceries = bills["groceries"][0]["amount"].AsInt;
		if (money >= costOfGroceries) {
			//TODO: Show Buying Groceries
			money = money - costOfGroceries;
		}
		else {
			addEffect(bills["groceries"][0]["effect"]);
		}
	}

	void addEffect(string effect) {
		string[] effectsArray = PlayerPrefsX.GetStringArray("effects");
		List<string> effects = new List<string>();
		effects.AddRange(effectsArray);
		effects.Add(effect);
		PlayerPrefsX.SetStringArray("effects", effects.ToArray());

	}

	// Update is called once per frame
	void Update () {
	
	}
}
