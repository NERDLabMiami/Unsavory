using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;
//using UnionAssets.FLE;

public class Level : MonoBehaviour {
	//public GameObject timer;
	public TimerScript timer;
	//TODO: What's gametimer for
	public float gameTimer = 0;
	public bool timerOn = false;
	public GameObject nose;
	public Catering catering;
	public CharacterDialog dialog;
	public PlayerScript player;
	public PlateScript plate;
	public TacoBar slowTacos;
	public TacoBar sickTacos;
	public TacoBar successTacos;

	public GameObject shiftCompletedCanvas;
	public GameObject shiftIncompleteSick;
	public GameObject shiftIncompleteSlow;

	public Animator boss;
	public Animator gameElements;
	public Animator HUD;

	public Text wagesUI;
	public Text wagesUI_Sick;
	public Text wagesUI_Slow;

	public AudioClip gamePlayBackground;
	public GameObject homeButton;
	public GameObject quitButton;
	private bool levelBegan = false;
	private bool sick = false;
	private bool fullday = false;
	private int currentLevel;
	private int score;
	public int retries;
	private float timeWorked;
	public string appleId = "959296519";
	public string apdroidAppUrl = "market://details?id=com.dataplayed.unsavory";


	// Use this for initialization
	void Start () {
	 	currentLevel = PlayerPrefs.GetInt("current level");
		int timesPlayed = PlayerPrefs.GetInt("times played", 0);
		if (timesPlayed == 3 && !PlayerPrefs.HasKey("rated")) {
			//Ask to rate the app

			PlayerPrefs.SetInt("rated",1);
		}
		retries = PlayerPrefs.GetInt("retries", 0);

		bool catering = PlayerPrefs.HasKey("catering");
		if (catering) {
			dialog.changeSpeechKey("catering");
		}
		else if (currentLevel == 1) {
			//first day of work
			Debug.Log("First Day of Work!");
			dialog.changeSpeechKey("welcome");
		}
		else {
			//return to work
			float lateness = PlayerPrefs.GetFloat("late effect", 0);
			if (lateness > 0) {
				Debug.Log("Late to work");
				dialog.changeSpeechKey("late_return");

			}
			else {
				Debug.Log("Returned to work on time");
				dialog.changeSpeechKey("return");

			}

		}

	}

	public void setTimeWorked(float hours) {
		timeWorked = hours;
	}

	public void beginLevel() {
		float lateness = PlayerPrefs.GetFloat("late effect", 0);

		if (!levelBegan) {
			timerOn = true;
			Camera.main.GetComponent<AudioSource>().clip = gamePlayBackground;
			Camera.main.GetComponent<AudioSource>().Play();
			nose.GetComponent<NoseWipeScript>().sneezeAllowed = true;

		}

		float minutesLate = UnityEngine.Random.Range(0, lateness);
		TimeSpan workday = new TimeSpan(8,(int)minutesLate,0);

		//TODO: Broadcast Late Message to Update Boss Dialogue for arriving late
		timer.setWorkday(workday);
		Debug.Log("Begin Level");
		levelBegan = true;
	}

	public void useRetry() {
		Debug.Log("Attempting Retry Use");
		if (retries > 0) {
			retries--;
			//just using both, animating a disabled one shouldn't matter
			sickTacos.useTaco(retries);
			slowTacos.useTaco(retries);
			PlayerPrefs.SetInt("retries", retries);
		}


	}

	public void completeCatering() {
		homeButton.SetActive(true);
		dialog.changeSpeechKey("catering_complete");

	}

	public void EndCatering(bool tooSlow) {
	
		if (tooSlow) {
			catering.tooSlow();
			
			GetComponent<UnityAnalyticsIntegration>().caterModeLevelFinished(Time.time, catering.plate.getOrders(), "slow");
		}
		
		else {
			GetComponent<UnityAnalyticsIntegration>().caterModeLevelFinished(Time.time, catering.plate.getOrders(), "sick");
			
			catering.sick();
		}
	}

	private void cleanupKitchen() {
		Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
		gameElements.SetTrigger("fade orders");
		HUD.SetTrigger("Fade Out");
		homeButton.SetActive(false);

	}


	private void completedFullDay() {
		Debug.Log ("running wage update");
		shiftCompletedCanvas.SetActive(true);
		score = 1;
		//LONGEST STREAK?
		if (plate.bestMatchedAggregate >= 10) {
			score++;
		}
		//PERFECT?
		if (plate.perfectLevel) {
			score++;
		}
		successTacos.addTacos(retries,score);
		//add retries for player
	//	int retries = PlayerPrefs.GetInt("retries", 0);
		retries += score;
		//MORE THAN 5?
		if (retries > 5) {
			retries = 5;
		}
		
		PlayerPrefs.SetInt("retries", retries);
		wagesUI.text = "$" + player.addWages(8).ToString("0.00") + " added to your next paycheck";
		GetComponent<UnityAnalyticsIntegration>().careerModeLevelFinished(currentLevel, "full day", Time.time);
	}

	private void incompleteSick() {
		GetComponent<UnityAnalyticsIntegration>().careerModeLevelFinished(currentLevel, "sick", Time.time);
	//TODO: Add Wages when end shift button clicked instead of before this runs.	
//		shiftCompleteText.text = "Sick on the Job";
//		realWorldFact.SetActive(true);
		Debug.Log("INCOMPLETE SICK");
		wagesUI_Sick.text = "You were caught being sick on the job. You can leave work early with "  + player.potentialWages(timeWorked).ToString("$0.00")  + " for your work today added to your next paycheck or try again.";
		int sneezes = PlayerPrefs.GetInt("sneezes", 1);
		if (sneezes > 1) {
			PlayerPrefs.SetInt("letter", 2);

		}
		else {
			PlayerPrefs.SetInt("letter", 1);

		}
		shiftIncompleteSick.SetActive(true);
	}

	private void incompleteWarningsExceeded() {
		GetComponent<UnityAnalyticsIntegration>().careerModeLevelFinished(currentLevel, "mismatches", Time.time);
		Debug.Log("INCOMPLETE SLOW");
		wagesUI_Slow.text = "Looks like you couldn't keep up with the orders! You can leave work and add "  + player.potentialWages(timeWorked).ToString("$0.00")  + " to your next paycheck for your work today or try again.";

		shiftIncompleteSlow.SetActive(true);
		//		shiftCompleteText.text = "Oh No!";
//		realWorldFact.SetActive(false);
//		Camera.main.GetComponent<AudioSource>().Stop ();
//		levelCompleteCanvas.SetActive(true);
//		levelCompleteCanvas.GetComponent<ShiftCompleteScript>().failed();
	}
	public void acceptWages() {
		player.addWages(timeWorked);
	}

	public void EndOfLevel(bool _fullday, bool _sick) {
		//stop showing the tutorial
		PlayerPrefs.SetInt("tutorial", 1);
		Time.timeScale = 0;
		fullday = _fullday;
		sick = _sick;
		cleanupKitchen();
		Camera.main.GetComponent<AudioSource>().Stop ();

		if(fullday) {
			completedFullDay();
		}
		
		else if(sick) {
			incompleteSick();
		}
		
		else {
			incompleteWarningsExceeded();
		}
		
	}

	public void initializeBossEndOfFullday() {
		Debug.Log ("Now Day " + currentLevel);
		homeButton.SetActive(true);		
		//FOR FINISHING!

		if (currentLevel%5 == 0) {
			Debug.Log("Weekend");
			dialog.changeSpeechKey("weekend");
			
		}
		else {
			dialog.changeSpeechKey("complete" + score.ToString());
			
		}
		
		//cue boss
		boss.SetTrigger("reappear");

	}

	public void initializeBossSick() {
		homeButton.SetActive(true);		

		Debug.Log("Giving Sick Talk");
		PlayerPrefs.SetInt("letter",0);
		PlayerPrefs.SetInt("activated",1);
		//First sneeze?
		if (PlayerPrefs.GetInt("sneezes",1) <= 2) {
			dialog.changeSpeechKey("sneeze_tip");
			Debug.Log("Giving Sneeze Tip");
		}
		else {
			dialog.changeSpeechKey("sneezed");
			Debug.Log("GIving Sneeze Speech");
			
		}

		if (checkWarnings(giveWarning())) {
			//FIRE
			dialog.changeSpeechKey("fired_sick");
			//ALLOW SICK DAYS
			PlayerPrefs.SetInt("paid sick days achieved",1);

		}

	}


	public void initializeBossSlow() {
		homeButton.SetActive(true);		

		dialog.changeSpeechKey("slow");
		if (checkWarnings(giveWarning())) {
			//FIRE
			dialog.changeSpeechKey("fired_slow");

		}
		Debug.Log("GIving Slow Talk");

	}

	private int giveWarning() {
		int warnings = PlayerPrefs.GetInt ("warnings");
		warnings+=1;
		PlayerPrefs.SetInt("warnings", warnings);
		return warnings;
	}

	private bool checkWarnings(int warnings) {
		int maxWarnings = PlayerPrefs.GetInt("max warnings");

		if (warnings > maxWarnings) {
			//TODO: Fire Player
			Debug.Log("Player should be fired");
			PlayerPrefs.SetInt("fired",1);
			PlayerPrefs.SetInt("can cater", 1);
			Debug.Log("Too Many Warnings");
			GetComponent<UnityAnalyticsIntegration>().fired ();
			homeButton.SetActive(false);
			//TODO: QUIT BUTTON SET ACTIVE
			quitButton.SetActive(true);
			return true;
		}
		else {
			return false;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (timerOn) {
			gameTimer += Time.deltaTime;
		}
	}
}
