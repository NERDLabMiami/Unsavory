using UnityEngine;
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
	public GameObject letterButton;
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
	public SwipeScene room; // assign
	public HomeToast toast;

	private void OnEnable()
	{
		if (room != null) room.OnPlaceChanged += HandlePlaceChanged;
	}

	private void OnDisable()
	{
		if (room != null) room.OnPlaceChanged -= HandlePlaceChanged;
	}

	private void HandlePlaceChanged(int place)
	{
		// Example mapping: 0 bedroom, 1 living room, 2 door (your old mapping)
		if (place == 0 && !PlayerPrefs.HasKey("seen bedroom"))
		{
			PlayerPrefs.SetInt("seen bedroom", 1);
			PlayerPrefs.Save();
			if (toast) toast.Show("Bedroom: Rest here to stay home and recover.");
		}
		else if (place == 2 && !PlayerPrefs.HasKey("seen door"))
		{
			PlayerPrefs.SetInt("seen door", 1);
			PlayerPrefs.Save();
			if (toast) toast.Show("Door: Go to work to earn wages (but it costs health).");
		}
		else if (place == 1 && !PlayerPrefs.HasKey("seen living"))
		{
			PlayerPrefs.SetInt("seen living", 1);
			PlayerPrefs.Save();
			if (toast) toast.Show("Living room: Use your phone to pay bills.");
		}
	}

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
		if (!PlayerPrefs.HasKey("welcomed home"))
		{
			if (toast) toast.Show("Welcome home. Pay bills, stock up on medicine, then head to work.");
			PlayerPrefs.SetInt("welcomed home", 1);

			// Optional: keep your “attention” animations without blocking
			// billButton.GetComponent<Animator>().SetTrigger("attention");
			// drugButton.GetComponent<Animator>().SetTrigger("attention");
		}

		//Apply Effects Based on Unpaid Bills
		billData =  Resources.Load<TextAsset>("bills").ToString();

		JSONNode bills = JSON.Parse(billData);
		for (int i = 0; i < bills["bills"].Count; i++)
		{
			if (bills["bills"][i]["due"].AsInt < day)
			{
				if (PlayerPrefs.GetInt("bill" + i, 0) == 0)
				{
					gameObject.GetComponent<UnityAnalyticsIntegration>()?.overdueBill(i);
					string eff = bills["bills"][i]["effect"];
					int currentEffect = PlayerPrefs.GetInt(eff, 0);
					currentEffect++;
					PlayerPrefs.SetInt(eff, currentEffect);
				}
			}
		}

		Debug.Log ("I have " + money.ToString());
		finalDay = PlayerPrefs.GetInt("final day", 20);
		Debug.Log("It's day " + day);
		fruit.setFruit(day%5);
// --- replacement for your block inside Start() ---

		bool isMonthEnd = (day % finalDay == 0) && (day != 0);
		bool isWeekend  = (day % 5 == 0);

		if (isWeekend)
		{
			ClampHealthForWeekend();
		}

		if (isMonthEnd)
		{
			// month end includes paycheck + groceries + bills check
			PaycheckAndGroceries();

			int unpaidBills = payOutstandingBills();
			if (unpaidBills > 0)
			{
				gameObject.GetComponent<UnityAnalyticsIntegration>()?.survivedWithUnpaidBills();
				if (gameOverPanel) gameOverPanel.SetActive(true);
			}
			else
			{
				gameObject.GetComponent<UnityAnalyticsIntegration>()?.survived();
				if (monthCompletePanel) monthCompletePanel.SetActive(true);
			}

			Debug.Log("Month is over, game ends");
		}
		else if (isWeekend)
		{
			ApplyPaidSickDayUsageIfNeeded();

			Debug.Log("It's the weekend");
			PlayerPrefs.SetInt("can cater", 1);

			PaycheckAndGroceries();

			gameObject.GetComponent<UnityAnalyticsIntegration>()?.weekend();
			if (bankAccountUI) bankAccountUI.text = money.ToString("$0");
			if (weekCompletePanel) weekCompletePanel.SetActive(true);
		}

// Always advance day at end of Start() flow
		UpdateEndOfDayUIAndAdvanceDay();
		PlayerPrefs.Save();
		
	}
	
	private void ClampHealthForWeekend()
	{
		float health = PlayerPrefs.GetFloat("health", 0f);

		// original behavior:
		// >=50 => set letter=0 and health=20
		// >=30 => set health=15
		// >=15 => set health=10
		if (health >= 50f)
		{
			PlayerPrefs.SetInt("letter", 0);
			PlayerPrefs.SetFloat("health", 20f);
		}
		else if (health >= 30f)
		{
			PlayerPrefs.SetFloat("health", 15f);
		}
		else if (health >= 15f)
		{
			PlayerPrefs.SetFloat("health", 10f);
		}
	}

	private void ApplyPaidSickDayUsageIfNeeded()
	{
		if (PlayerPrefs.HasKey("using paid sick days"))
		{
			int paidSickDays = PlayerPrefs.GetInt("paid sick days", 0);
			paidSickDays++;
			PlayerPrefs.SetInt("paid sick days", paidSickDays);
		}
	}

	private float PaycheckAndGroceries()
	{
		// paycheck
		float paycheck = calculateWages();
		if (paycheckUI) paycheckUI.text = paycheck.ToString("$0");

		// groceries (buyGroceries already handles health penalty when broke)
		float groceryCost = buyGroceries();
		if (groceryCost > 0f)
		{
			money -= groceryCost;
			PlayerPrefs.SetFloat("money", money);
			if (groceryUI) groceryUI.text = groceryCost.ToString("-$0");
		}
		else
		{
			if (groceryUI) groceryUI.text = "N/A";
		}

		return groceryCost;
	}

	private void UpdateEndOfDayUIAndAdvanceDay()
	{
		day++;
		if (moneyUI) moneyUI.text = money.ToString("$0");
		PlayerPrefs.SetInt("current level", day);

		if (PlayerPrefs.HasKey("letter"))
		{
			toast.Show("You got a letter in the mail.");
		}
	}

	public void gameOver(bool won)
	{
		CareerFlowManager.Instance.GameOver(won);
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
}
