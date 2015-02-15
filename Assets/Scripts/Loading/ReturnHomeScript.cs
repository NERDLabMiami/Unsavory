using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class ReturnHomeScript : MonoBehaviour {
	public GameObject phone;
	private bool playerWasFired = false;
	public bool electricity = true;
	public Text moneyUI;


	// Use this for initialization
	void Start () {
		bool weekend = false;
		int day = PlayerPrefs.GetInt("current level");

		float money = PlayerPrefs.GetFloat("money");

		float[] wages = PlayerPrefsX.GetFloatArray("wages");
		if (PlayerPrefs.HasKey ("fired")) {
			playerWasFired = true;
			Debug.Log("Fired...");
			//TODO: Need to reveal to the player
			//TODO: Save starting health in JSON file
			CustomFunctionScript.resetPlayerData(30,0);
		}
		else  {
			Debug.Log("It's day " + day);
			if (day % 5 == 0) {
				//Pay Wages, Add Notification for Bill
				weekend = true;
				buyGroceries();
				Debug.Log("It's the weekend");
				//TODO: Game Mode -> Overtime/Catering work until too slow, paid immediately as bonus.
				string daysWorked = "";
				if (wages.Length >= day) {
					for (int i = day; i > day - 5; i--) {
						money += (int)wages[i-1];
						daysWorked += "$" + wages[i-1].ToString() + "\n";
						Debug.Log ("Pay for " + wages[i-1]);
					}
				}
			}
			//advance the day now that you're home.
			PlayerPrefs.SetInt ("current level", day+1);
			PlayerPrefs.SetFloat("money", money);
			moneyUI.text = money.ToString();
			if (day >= 20) {
				//TODO: Come up with ending, promotion?
				// OR, never ends, basically a sprint each time?
			
				Debug.Log("Month is over, game ends");
			}
		}

	}
	

	void buyGroceries() {
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		float money = PlayerPrefs.GetFloat ("money");
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
