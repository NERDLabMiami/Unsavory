using UnityEngine;
using SimpleJSON;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class ReturnHomeScript : MonoBehaviour {
	public FruitBowl fruit;
	private int day;
	private int finalDay;
	private float money;
	private string billData;
	public SwipeScene room; // assign
	public HomeToast toast;
	public string newMonthCaption = "New month, another adventure.";
	private bool rentPending = false;
	private bool endingStarted = false;

	private void OnEnable()
	{
		BillsSignals.BillPaid += HandleBillPaid;
	}

	private void OnDisable()
	{
		BillsSignals.BillPaid -= HandleBillPaid;
	}

	// Paying rent is the last action of a month: fade out, caption, then the layoff mail arrives.
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

		// Laid off. The player stays home with the email; the door now leads to the credits.
		EndRun("layoff");
		yield return FadeAlpha(bg, caption, 1.0f, 1f, 0f, true);
		yield return FadeAlpha(bg, caption, 1.5f, 1f, 0f, false);
		Destroy(canvasGO);
		ShowRunEndToast();
	}

	// How a run ends: record it, deliver the ending mail (see mail.txt "ending"), and tell the player to read it.
	private void EndRun(string how)
	{
		CareerFlowManager.Instance.EndRun(how);
	}

	private void ShowRunEndToast()
	{
		if (toast) toast.ShowKey("home_enter", new Dictionary<string, float> { { "run_end", RunEndCode() } });
	}

	private static float RunEndCode()
	{
		switch (MailArchive.RunEnd())
		{
			case "layoff": return 1;
			case "eviction": return 2;
			default: return 0;
		}
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

	private float calculateWages() {
		money = PlayerPrefs.GetFloat("money");
		float paycheck = PlayerPrefs.GetFloat("earned wages", 0);
//		Debug.Log("WAGES TO BE PAID: " + wages);
		money += paycheck;
		//set earned wages back to nothing
		PlayerPrefs.SetFloat("earned wages", 0);
		PlayerState.RecordPaycheck(paycheck);
		PlayerState.SetMoney(money);
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

		// The scene opens black: a toast reads on the black, then the scene fades in as it leaves.
		ScreenFade.Black(toast != null ? toast.RootCanvas : null);

		money = PlayerPrefs.GetFloat("money");
		day = PlayerPrefs.GetInt("current level", 0);
		Time.timeScale = 1f;

		// The run is over: nothing here advances any more, the player reads their email and leaves by the door.
		if (CareerFlowManager.Instance.RunEnded)
		{
			ShowRunEndToast();
			if (toast == null || !toast.IsShowing) ScreenFade.FadeIn(HomeToast.RevealSeconds);
			return;
		}
		// Arriving home for the very first time (New Game) is not the end of a workday: day 1 is still to come.
		bool firstVisit = !PlayerPrefs.HasKey("welcomed home");
		if (firstVisit)
		{
			if (toast) toast.Show("Welcome home. Pay bills, stock up on medicine, then head to work.");
			PlayerPrefs.SetInt("welcomed home", 1);

		}

		//Apply Effects Based on Unpaid Bills
		billData =  Resources.Load<TextAsset>("bills").ToString();

		JSONNode bills = JSON.Parse(billData);
		for (int i = 0; i < bills["bills"].Count; i++)
		{
			if (bills["bills"][i]["type"].Value != "buy" && BillsState.IsAvailable(i) && bills["bills"][i]["due"].AsInt < day)
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
		bool isMonthEnd = (day % finalDay == 0) && (day != 0);
		bool isWeekend  = (day % 5 == 0);

		if (!firstVisit) DecayHealth();
		if (isWeekend && PlayerPrefs.GetFloat("health", 0f) >= 30f) PlayerPrefs.SetInt("letter", 0);

		if (countUnpaidGameOverBills(true) > 0)
		{
			// Rent was due on the last day of the month and never paid.
			gameObject.GetComponent<UnityAnalyticsIntegration>()?.survivedWithUnpaidBills();
			EndRun("eviction");
		}
		else if (isMonthEnd)
		{
			calculateWages();

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

			calculateWages();

			gameObject.GetComponent<UnityAnalyticsIntegration>()?.weekend();
		}

// Always advance day at end of Start() flow
		UpdateEndOfDayUIAndAdvanceDay(firstVisit);
		PlayerPrefs.Save();

		// No entry toast to carry the reveal.
		if (toast == null || !toast.IsShowing) ScreenFade.FadeIn(HomeToast.RevealSeconds);

	}
	
	// Health is the seconds between sneezes. It wears down a little every workday, quickly at first and then more slowly
	// (60 on day 1, about 31 on day 5, about 12 on day 11), and stops at the floor, which unpaid health insurance then eats into.
	const float HealthDecayFactor = 0.85f;
	const float HealthFloor = 10f;

	private void DecayHealth()
	{
		float health = PlayerPrefs.GetFloat("health", 0f);
		if (health > HealthFloor)
			PlayerPrefs.SetFloat("health", Mathf.Max(HealthFloor, health * HealthDecayFactor));
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

	private void UpdateEndOfDayUIAndAdvanceDay(bool firstVisit)
	{
		if (!firstVisit) day++;
		PlayerPrefs.SetInt("current level", day);
		MailArchive.DeliverDue();

		// One toast for arriving home, chosen from toasts.txt by day, missed bills, rent, letter, first visit.
		if (toast) toast.ShowKey("home_enter", new Dictionary<string, float>
		{
			{ "first_visit", firstVisit ? 1 : 0 },
			{ "rent_pending", rentPending ? 1 : 0 },
			{ "letter", PlayerPrefs.HasKey("letter") ? 1 : 0 },
			{ "run_end", RunEndCode() },
		});
	}
}
