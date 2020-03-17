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
	public FruitBowl fruit;
	public GameObject gameOverPanel;
	public GameObject monthCompletePanel;
	public GameObject weekCompletePanel;
	public GameObject billButton;
	public GameObject drugButton;
	public GameObject bedButton;
	public GameObject doorButton;
	public GameObject bedroomPanel;
	public GameObject doorPanel;

	public GameObject letterButton;
	public SwipeScene room;
	public GameObject firstTimeHomePanel;
	public MusicLibrary music;
	private bool weekend = false;
	private int day;
	private int finalDay;
	private float money;
//	private float wages;
	private string billData;
	private bool visitedBedroom = false;
	private bool visitedExit = false;
	// Use this for initialization

	private float calculateWages() {
		money = PlayerPrefs.GetFloat("money");
		float paycheck = PlayerPrefs.GetFloat("earned wages", 0);
//		Debug.Log("WAGES TO BE PAID: " + wages);
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
				Debug.Log("ATTEMPTING BILL " + i);
				if (PlayerPrefs.GetInt("bill" + i,0) == 0) {
					Debug.Log("HAVEN'T PAID " + i);

					//pay it
					float remainingMoney =money - bills["bills"][i]["amount"].AsInt;
					if (remainingMoney >= 0) {
						money = remainingMoney;
						PlayerPrefs.SetInt("bill" + i,1); 
						Debug.Log("PAID " + i);

					}
					else {
						Debug.Log("CAN'T PAY " + i);

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
		//TODO: DEBUG

		//END DEBUG
		if (!PlayerPrefs.HasKey("welcomed home")) {
			firstTimeHomePanel.SetActive(true);
			//TODO: Attention to Drugs and Bills
			billButton.GetComponent<Animator>().SetTrigger("attention");
			drugButton.GetComponent<Animator>().SetTrigger("attention");
			PlayerPrefs.SetInt("welcomed home", 1);
		}

		if (PlayerPrefs.HasKey("seen bedroom")) {
			visitedBedroom = true;
		}

		if (PlayerPrefs.HasKey("seen door")) {
			visitedExit = true;
		}

		//Apply Effects Based on Unpaid Bills
		billData =  Resources.Load<TextAsset>("bills").ToString();

		JSONNode bills = JSON.Parse(billData);
		for (int i = 0; i < bills["bills"].Count; i++) {
			//if it's past the due date
			if (bills["bills"][i]["due"].AsInt < day) {
				//check if it's paid
				if (PlayerPrefs.GetInt("bill" + i,0) == 1) {
					//it's unpaid
					gameObject.GetComponent<UnityAnalyticsIntegration>().overdueBill(i);
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
			finalDay = PlayerPrefs.GetInt("final day", 20);
			Debug.Log("It's day " + day);
			fruit.setFruit(day%5);
			if (day % 5 == 0) {
				//Pay Wages, Add Notification for Bill
				weekend = true;
				//Decrease health significantly
				float health = PlayerPrefs.GetFloat("health");

				if (health >= 50) {
					PlayerPrefs.SetInt("letter", 0);
					PlayerPrefs.SetFloat("health", 20);
				}
				else if (health >= 30) {
					PlayerPrefs.SetFloat("health", 15);
				}
				else if (health >= 15) {
					PlayerPrefs.SetFloat("health", 10);
				}


			}

				if (day%finalDay == 0 && day != 0) {
				//add last paycheck
					paycheckUI.text = calculateWages().ToString("$0");
					float groceryCost = buyGroceries();

					int unpaidBills = payOutstandingBills();
		
					if (unpaidBills > 0) {
						//LOST GAME, present game over panel
						gameObject.GetComponent<UnityAnalyticsIntegration>().survivedWithUnpaidBills();
						gameOverPanel.SetActive(true);
						
					}
					else {
						//Activate Month Complete Pane
						gameObject.GetComponent<UnityAnalyticsIntegration>().survived();

						monthCompletePanel.SetActive(true);
						//GKAchievementReporter.ReportAchievement( Achievements.SURVIVED, 100, true);

				}

					//TODO: unlock wage control ability

					Debug.Log("Month is over, game ends");
				}
				else if(weekend) {
					if (PlayerPrefs.HasKey("using paid sick days")) {
						int paidSickDays = PlayerPrefs.GetInt("paid sick days",0);
						paidSickDays++;
						PlayerPrefs.SetInt("paid sick days", paidSickDays);
					}
		
					Debug.Log("It's the weekend");
					PlayerPrefs.SetInt("can cater",1);
					paycheckUI.text = calculateWages().ToString("$0");
					float groceryCost = buyGroceries();
					if (groceryCost > 0) {
						Debug.Log("Deducting " + groceryCost + " for groceries");
						money = money - groceryCost;
						PlayerPrefs.SetFloat("money", money);
						groceryUI.text = groceryCost.ToString("-$0");
					}
				else {
					//health deducted in buygroceries
						groceryUI.text = "N/A";
				}
				gameObject.GetComponent<UnityAnalyticsIntegration>().weekend();
				bankAccountUI.text = money.ToString("$0");
					weekCompletePanel.SetActive(true);

				//TODO: Offer Overtime Catering
				//check if player has phone
//				phone.SetActive(true);
//				music.phoneRing();
				billButton.GetComponent<Animator>().SetTrigger("attention");

			}
			day++;
			moneyUI.text = money.ToString("$0");
			PlayerPrefs.SetInt("current level", day);
			if (PlayerPrefs.HasKey("letter")) {
				//TODO: Turn letter button on
				letterButton.SetActive(true);
				PlayerPrefs.SetInt("activated",1);
			}

		}
	}
	

	public void gameOver(bool won) {
		if (won) {
			//going to credits scene
			PlayerPrefs.SetInt("survived", 1);
			PlayerPrefs.SetInt("reset",1);
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
		if (!visitedExit) {
			if (room.currentPlace == 2) {
				Debug.Log("visited exit");
				PlayerPrefs.SetInt("seen door",1);
				visitedExit = true;
				doorButton.GetComponent<Animator>().SetTrigger("attention");
				doorPanel.SetActive(true);

			}
		}

		if (!visitedBedroom) {
			if (	room.currentPlace == 0) {
				Debug.Log("visited bedroom");
				PlayerPrefs.SetInt("seen bedroom",1);
				bedButton.GetComponent<Animator>().SetTrigger("attention");
				visitedBedroom = true;
				bedroomPanel.SetActive(true);
			}
		}
	}
}
