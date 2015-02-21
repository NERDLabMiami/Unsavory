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
	public Text paycheckUI;
	public Text bankAccountUI;
	public Text groceryUI;
	public GameObject gameOverPanel;
	public GameObject monthCompletePanel;
	public GameObject weekCompletePanel;
	public MusicLibrary music;
	private bool weekend = false;
	private int day;
	private int finalDay;
	private float money;
	private float[] wages;
	// Use this for initialization

	private float calculateWages() {
		money = PlayerPrefs.GetFloat("money");
		wages = PlayerPrefsX.GetFloatArray("wages");
		string daysWorked = "";
		float paycheck = 0;
		if (wages.Length >= day) {
			for (int i = day; i > day - 5; i--) {
				paycheck += (int)wages[i-1];
				daysWorked += "$" + wages[i-1].ToString() + "\n";
				Debug.Log ("Pay for " + wages[i-1]);
			}
		}
		money += paycheck;

		PlayerPrefs.SetFloat("money", money);
		Debug.Log("Player has $" + money);
		return paycheck;
	}

	private int payOutstandingBills() {
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		int[] dueDates = PlayerPrefsX.GetIntArray("due");
//		string[] effectsArray = PlayerPrefsX.GetStringArray("effects");
		int unpaidBills = 0;
		for (int i = 0; i < bills["bills"].Count; i++) {
			
			if (dueDates[i] <= day) {
				//pay each bill
				if (money >= bills["bills"][i]["amount"].AsInt) {
					money = money - bills["bills"][i]["amount"].AsInt;
					dueDates[i] += finalDay;
					//remove effect, only worthwhile if playing past the month
					List<string> effects = new List<string>();
					effects.AddRange(PlayerPrefsX.GetStringArray("effects"));
					effects.RemoveAll(s => s == bills["bills"][i]["effect"]);
					PlayerPrefsX.SetStringArray("effects", effects.ToArray());
					
				}
				else {
					unpaidBills++;
				}
				
			}
		}
		PlayerPrefsX.SetIntArray("due", dueDates);
		return unpaidBills;
	}
	
	void Start () {

		if (PlayerPrefs.HasKey("catering")) {

			PlayerPrefs.DeleteKey("catering");

		}
		else {
			day = PlayerPrefs.GetInt("current level");
			Debug.Log ("I have " + money.ToString());
			finalDay = PlayerPrefs.GetInt("final day");
			Debug.Log("It's day " + day);
			if (day % 5 == 0) {
				//Pay Wages, Add Notification for Bill
				weekend = true;
			}

				if (day%finalDay == 0 && day != 0) {

					int unpaidBills = payOutstandingBills();
		
					if (unpaidBills > 0) {
						//LOST GAME, present game over panel
						gameOverPanel.SetActive(true);
					}
					else {
						//Activate Month Complete Pane
						monthCompletePanel.SetActive(true);
					}

					//TODO: unlock wage control ability

					Debug.Log("Month is over, game ends");
				}
				else if(weekend) {
					Debug.Log("It's the weekend");
					paycheckUI.text = "$" + calculateWages().ToString();
					buyGroceries();
					bankAccountUI.text = "$" + money.ToString();
					weekCompletePanel.SetActive(true);
				//TODO: Offer Overtime Catering
				//check if player has phone
				phone.SetActive(true);
				music.phoneRing();

			}
			day++;
			moneyUI.text = money.ToString();
			PlayerPrefs.SetInt("current level", day);
		}
	}
	

	public void gameOver(bool won) {
		if (won) {
			//going to credits scene
		}
		else {
			//returning to main menu as a loser
			PlayerPrefs.SetInt("fired", 1);
		}
	}

	public void continueToNextMonth() {
		int currentMonth = PlayerPrefs.GetInt("month",1);
		currentMonth++;
		PlayerPrefs.SetInt("month", currentMonth);
	}
	
	void buyGroceries() {
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		money = PlayerPrefs.GetFloat ("money");
		int costOfGroceries = bills["groceries"][0]["amount"].AsInt;
		if (money >= costOfGroceries) {
			//TODO: Show Buying Groceries
			money = money - costOfGroceries;
			PlayerPrefs.SetFloat("money", money);
			groceryUI.text = "-$" + costOfGroceries.ToString();
		}
		else {
			groceryUI.text = "N/A";
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
