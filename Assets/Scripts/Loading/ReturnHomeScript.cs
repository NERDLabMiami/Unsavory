using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class ReturnHomeScript : MonoBehaviour {
	public GameObject phone;
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
	private float wages;
	private string billData;
	// Use this for initialization

	private float calculateWages() {
		money = PlayerPrefs.GetFloat("money");
		float paycheck = PlayerPrefs.GetFloat("earned wages", 0);
		Debug.Log("WAGES TO BE PAID: " + wages);
		money += paycheck;
		//set earned wages back to nothing
		PlayerPrefs.SetFloat("earned wages", 0);
		PlayerPrefs.SetFloat("money", money);
		Debug.Log("Player has $" + money);
		return paycheck;
	}

	private int payOutstandingBills() {
		JSONNode bills = JSON.Parse(billData);
		int unpaidBills = 0;

		for (int i = 0; i < bills["bills"].Count; i++) {
			//if it's past the due date
			if (bills["bills"][i]["due"].AsInt >= day) {
				//check if it's unpaid
				if (PlayerPrefs.GetInt("bill" + i,0) == 0) {
					//pay it
					float remainingMoney =money - bills["bills"][i]["amount"].AsInt;
					if (remainingMoney >= 0) {
						money = remainingMoney;
						PlayerPrefs.SetInt("bill" + i,1); 
					}
					else {
						unpaidBills++;
					}
				}
			}
		}
		return unpaidBills;
	}
	
	void Start () {
		money = PlayerPrefs.GetFloat("money");
		day = PlayerPrefs.GetInt("current level", 0);
		Time.timeScale = 1f;
		//Apply Effects Based on Unpaid Bills
		billData =  Resources.Load<TextAsset>("bills").ToString();

		JSONNode bills = JSON.Parse(billData);
		for (int i = 0; i < bills["bills"].Count; i++) {
			//if it's past the due date
			if (bills["bills"][i]["due"].AsInt < day) {
				//check if it's paid
				if (PlayerPrefs.GetInt("bill" + i,0) == 1) {
					//it's unpaid
					string effect = bills["bills"][i]["effect"];
					int currentEffect = PlayerPrefs.GetInt(effect, 0);
					currentEffect++;
					PlayerPrefs.SetInt(effect, currentEffect);
					Debug.Log(effect + " is now " + currentEffect);
				}
				else {
					Debug.Log("This bill is already paid. No effect added");
				}
			}

		}

		if (PlayerPrefs.HasKey("catering")) {

			PlayerPrefs.DeleteKey("catering");

		}
		else {
			Debug.Log ("I have " + money.ToString());
			finalDay = PlayerPrefs.GetInt("final day", 30);
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
					paycheckUI.text = "$" + calculateWages().ToString("0.00");
					float groceryCost = buyGroceries();
					if (groceryCost > 0) {
						Debug.Log("Deducting " + groceryCost + " for groceries");
						money = money - groceryCost;
						PlayerPrefs.SetFloat("money", money);
						groceryUI.text = "-$" + groceryCost.ToString("0.00");
					}
				else {
					//health deducted in buygroceries
						groceryUI.text = "N/A";
				}
					bankAccountUI.text = "$" + money.ToString("0.00");
					weekCompletePanel.SetActive(true);
				//TODO: Offer Overtime Catering
				//check if player has phone
				phone.SetActive(true);
				music.phoneRing();

			}
			day++;
			moneyUI.text = "$" + money.ToString("0.00");
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
		Application.LoadLevel(0);
	}

	public void continueToNextMonth() {
		int currentMonth = PlayerPrefs.GetInt("month",1);
		currentMonth++;
		PlayerPrefs.SetInt("month", currentMonth);
	}
	
	float buyGroceries() {
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		money = PlayerPrefs.GetFloat ("money");
		float costOfGroceries = bills["groceries"][0]["amount"].AsFloat;
		if (money >= costOfGroceries) {
			return costOfGroceries; 
		}
		else {
			float health = PlayerPrefs.GetFloat("health");
			health--;
			PlayerPrefs.SetFloat("health", health);
			return 0f;
		}
	}


	// Update is called once per frame
	void Update () {
	
	}
}
