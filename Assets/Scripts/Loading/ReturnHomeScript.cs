using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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
	public string newMonthCaption = "New month, another adventure.";
	private bool rentPending = false;
	private bool endingStarted = false;

	private void OnEnable()
	{
		if (room != null) room.OnPlaceChanged += HandlePlaceChanged;
		BillsSignals.BillPaid += HandleBillPaid;
	}

	private void OnDisable()
	{
		if (room != null) room.OnPlaceChanged -= HandlePlaceChanged;
		BillsSignals.BillPaid -= HandleBillPaid;
	}

	// Paying rent is the last action of a month: fade out, caption, then the run ends.
	private void HandleBillPaid(int id)
	{
		if (rentPending && BillsState.GetEffect(id) == "gameover")
			StartNewMonthSequence();
	}

	private void StartNewMonthSequence()
	{
		if (endingStarted) return;
		endingStarted = true;
		rentPending = false;
		StartCoroutine(NewMonthRoutine());
	}

	private IEnumerator NewMonthRoutine()
	{
		gameObject.GetComponent<UnityAnalyticsIntegration>()?.survived();

		var canvasGO = new GameObject("NewMonthFade", typeof(Canvas));
		var canvas = canvasGO.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 32000;

		var bgGO = new GameObject("Black", typeof(CanvasRenderer), typeof(Image));
		bgGO.transform.SetParent(canvasGO.transform, false);
		var bg = bgGO.GetComponent<Image>();
		bg.color = new Color(0f, 0f, 0f, 0f);
		bg.raycastTarget = true; // swallow input during the ending
		Stretch(bgGO.GetComponent<RectTransform>());

		var textGO = new GameObject("Caption", typeof(CanvasRenderer), typeof(TextMeshProUGUI));
		textGO.transform.SetParent(canvasGO.transform, false);
		var caption = textGO.GetComponent<TextMeshProUGUI>();
		caption.text = newMonthCaption;
		caption.alignment = TextAlignmentOptions.Center;
		caption.fontSize = 48;
		caption.color = new Color(1f, 1f, 1f, 0f);
		caption.raycastTarget = false;
		Stretch(textGO.GetComponent<RectTransform>());

		yield return FadeAlpha(bg, caption, 1.5f, 0f, 1f, false);
		yield return FadeAlpha(bg, caption, 1.0f, 0f, 1f, true);
		yield return new WaitForSecondsRealtime(2.5f);

		continueToNextMonth();
		gameOver(true);
	}

	private static void Stretch(RectTransform rt)
	{
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = rt.offsetMax = Vector2.zero;
	}

	private static IEnumerator FadeAlpha(Image bg, TMP_Text text, float duration, float from, float to, bool captionPass)
	{
		for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
		{
			float a = Mathf.Lerp(from, to, t / duration);
			if (captionPass) text.color = new Color(1f, 1f, 1f, a);
			else bg.color = new Color(0f, 0f, 0f, a);
			yield return null;
		}
		if (captionPass) text.color = new Color(1f, 1f, 1f, to);
		else bg.color = new Color(0f, 0f, 0f, to);
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

	// Bills are only ever paid by the player (BillScript.payBill). At month end, an unpaid bill whose
	// effect is "gameover" (rent) ends the run.
	private int countUnpaidGameOverBills(bool pastDueOnly) {
		JSONNode bills = JSON.Parse(billData);
		int unpaid = 0;
		for (int i = 0; i < bills["bills"].Count; i++) {
			if (bills["bills"][i]["effect"].Value != "gameover" || PlayerPrefs.GetInt("bill" + i, 0) != 0) continue;
			if (!pastDueOnly || bills["bills"][i]["due"].AsInt < day)
				unpaid++;
		}
		return unpaid;
	}
	
	void Start () {

		money = PlayerPrefs.GetFloat("money");
		day = PlayerPrefs.GetInt("current level", 0);
		Time.timeScale = 1f;
		// Arriving home for the very first time (New Game) is not the end of a workday: day 1 is still to come.
		bool firstVisit = !PlayerPrefs.HasKey("welcomed home");
		if (firstVisit)
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
			if (bills["bills"][i]["type"].Value != "buy" && bills["bills"][i]["due"].AsInt < day)
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
		// Fruit only replenishes when the player buys groceries on the phone; it dwindles by one a day.
		int fruitCount = Mathf.Max(0, PlayerPrefs.GetInt("fruit", 5) - (firstVisit ? 0 : 1));
		PlayerPrefs.SetInt("fruit", fruitCount);
		if (fruit) fruit.SetCount(fruitCount);
// --- replacement for your block inside Start() ---

		bool isMonthEnd = (day % finalDay == 0) && (day != 0);
		bool isWeekend  = (day % 5 == 0);

		if (isWeekend)
		{
			ClampHealthForWeekend();
		}

		if (countUnpaidGameOverBills(true) > 0)
		{
			// Rent was due on the last day of the month and never paid.
			gameObject.GetComponent<UnityAnalyticsIntegration>()?.survivedWithUnpaidBills();
			if (gameOverPanel) gameOverPanel.SetActive(true);
		}
		else if (isMonthEnd)
		{
			PaycheckAndGroceries();

			// The player ends the month by paying rent on the phone; that triggers the fade-out.
			if (countUnpaidGameOverBills(false) == 0)
			{
				StartNewMonthSequence();
			}
			else
			{
				rentPending = true;
				if (toast) toast.Show("Rent is due today. Pay it to start a new month.");
			}

			Debug.Log("Month is over, waiting on rent");
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
		UpdateEndOfDayUIAndAdvanceDay(firstVisit);
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

		if (groceryUI) groceryUI.text = "N/A";
		float groceryCost = 0f;
		return groceryCost;
	}

	private void UpdateEndOfDayUIAndAdvanceDay(bool firstVisit)
	{
		if (!firstVisit) day++;
		if (moneyUI) moneyUI.text = money.ToString("$0");
		PlayerPrefs.SetInt("current level", day);

		if (PlayerPrefs.HasKey("letter"))
		{
			if (toast) toast.Show("You got a letter in the mail.");
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
}
