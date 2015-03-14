using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class PlayerScript : MonoBehaviour {
	public Text moneyUI;
	public Text pillsUI;
	private float money;
	private int pills;
	private float hourlyRate;
//	public bool hasSneezed = false;
	public bool reset = false;
	public GameObject levelCompleteCanvas;
	public Animator gameScreen;
	public Animator gameGUI;
	public GameObject homeButton;
	public GameObject retryButton;
	public GameObject quitButton;
	public GameObject boss;
	public MusicLibrary music;
	public Text shiftCompleteText;
	public Text wagesUI;
	private float daysWage;
	private bool endless;
	private bool fullday;
	private bool sick;

	public void resetData() {
		if (!PlayerPrefs.HasKey ("current level") || reset) {
			//this resets
			CustomFunctionScript.resetPlayerData(30, money);
		}

	}

	public float addWages(float timeWorked) {
		float earnedWages = PlayerPrefs.GetFloat("earned wages",0);
		Debug.Log("Earned Wages Before: " + earnedWages);

		daysWage = hourlyRate * timeWorked;
		Debug.Log("Today's Wage: " + daysWage);
		earnedWages += daysWage;
		PlayerPrefs.SetFloat("earned wages", earnedWages);
		Debug.Log("Earned Wages Now " + earnedWages);
		return daysWage;
	}

	public void cateringEvent() {
		PlayerPrefs.SetInt("catering",1);
	}

	public void completeCatering() {
		homeButton.SetActive(true);
		retryButton.SetActive(false);
		boss.GetComponent<CharacterDialog>().changeSpeechKey("catering_complete");
		//cue boss
		boss.GetComponent<Animator>().SetBool("talkAgain", true);
		boss.GetComponent<Animator>().SetBool("finishedTalking", false);

	}

	public void setBoss() {
		int currentLevel = PlayerPrefs.GetInt("current level");

		if (!endless) {
			Debug.Log("Setting Retry Button Inactive");
			retryButton.SetActive(false);
			homeButton.SetActive(true);

			if (fullday) {

				Debug.Log ("Now Day " + currentLevel);

				if (currentLevel%5 == 0) {
					Debug.Log("Weekend");
					boss.GetComponent<CharacterDialog>().changeSpeechKey("weekend");
					
				}
				else {
					boss.GetComponent<CharacterDialog>().changeSpeechKey("complete");
					
				}
				
				//cue boss
				boss.GetComponent<Animator>().SetTrigger("reappear");

				
			}
			else {
				//too late

				if (sick) {

					//First sneeze?
					if (PlayerPrefs.GetInt("sneezes",1) <= 2) {
						boss.GetComponent<CharacterDialog>().changeSpeechKey("sneeze_tip");

					}
					else {
						boss.GetComponent<CharacterDialog>().changeSpeechKey("sneezed");

					}
				}
				else {
					//Slow
					boss.GetComponent<CharacterDialog>().changeSpeechKey("slow");					
				}

				int warnings = PlayerPrefs.GetInt ("warnings");
				warnings+=1;
				int maxWarnings = PlayerPrefs.GetInt("max warnings");
				PlayerPrefs.SetInt("warnings", warnings);
				if (warnings > maxWarnings) {
					//TODO: Fire Player
					Debug.Log("Player should be fired");
					PlayerPrefs.SetInt("fired",1);
					if (sick) {
						boss.GetComponent<CharacterDialog>().changeSpeechKey("fired_sick");

					}
					else {
						boss.GetComponent<CharacterDialog>().changeSpeechKey("fired_slow");

					}

					quitButton.SetActive(true);
					homeButton.SetActive(false);
				}
			}
		}
		else {
			if (sick) {
				boss.GetComponent<CharacterDialog>().changeSpeechKey("fired_sick");
				
			}
			else {
				boss.GetComponent<CharacterDialog>().changeSpeechKey("fired_slow");
				
			}

		}

	}

	public void EndOfLevel(bool _fullday, bool _endless, bool _sick) {
		fullday = _fullday;
		endless = _endless;
		sick = _sick;
		Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
		gameScreen.SetTrigger("fade orders");

//		int currentLevel = PlayerPrefs.GetInt("current level");
		PlayerPrefs.SetInt("tutorial", 1);
		Debug.Log("End of Level Ran");
		gameGUI.SetTrigger("Fade Out");
		homeButton.SetActive(false);
		retryButton.SetActive(false);


		 if(fullday) {
			Time.timeScale = 0;
			Debug.Log ("running wage update");
			levelCompleteCanvas.SetActive(true);
			wagesUI.text = "$" + addWages(8).ToString("0.00") + " added to your next paycheck";
			shiftCompleteText.text = "Shift Complete";
//			music.levelCompleted();
			Camera.main.GetComponent<AudioSource>().Stop ();
			levelCompleteCanvas.GetComponentInChildren<ShiftCompleteScript>().success();

		}

		else if(sick) {
			//ended because of sickness
			wagesUI.text = "You were caught being sick on the job. You have to leave work early. $" + daysWage.ToString("0.00") + " has been added to your next paycheck for your work today.";
			shiftCompleteText.text = "Sick on the Job";
			Camera.main.GetComponent<AudioSource>().Stop ();
			levelCompleteCanvas.GetComponent<ShiftCompleteScript>().failed();

		}
		
		else {
			Time.timeScale = 0;
			wagesUI.text = "You were too slow with preparing orders and have been sent home. $" + daysWage.ToString("0.00") + " has been added to your next paycheck for your work today.";
			shiftCompleteText.text = "Too Slow";
			Debug.Log("SLOW UPDATE SPEECH");
//			music.levelFailed();
			Camera.main.GetComponent<AudioSource>().Stop ();
			levelCompleteCanvas.SetActive(true);
//			ShiftCompleteScript shift = levelCompleteCanvas.GetComponent<ShiftCompleteScript>();
//			shift.failed ();
			levelCompleteCanvas.GetComponent<ShiftCompleteScript>().failed();
		}
		//NOT SURE IF THIS APPLIES ANYMORE
//		gameScreen.SetBool("ended", true);
	
	}

	void Start() {
		//checks if values need to be reset
		resetData();
		hourlyRate = PlayerPrefs.GetFloat("hourly rate", 8.05f);

		if (PlayerPrefs.HasKey("pills")) {
			pills = PlayerPrefs.GetInt("pills");
		}
		else {
			PlayerPrefs.SetInt("pills", 0);
			pills = 0;
		}

	
		if (PlayerPrefs.HasKey("money")) {
			money = PlayerPrefs.GetFloat ("money");
		}
		else {
			PlayerPrefs.SetFloat("money", money);
		}

		if (moneyUI) {
			moneyUI.text = "$" + money.ToString("0");
			Debug.Log("Set Money");
			Debug.Log("Money: " + money);
		}

		if (pillsUI) {
			pillsUI.text = pills.ToString();
		}

	}



}
