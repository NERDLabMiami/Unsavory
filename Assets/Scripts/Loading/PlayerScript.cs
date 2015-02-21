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
	public float hourlyRate;
//	public bool hasBeenTutored;
//	public int warningsBeforeFired = 3;
	public bool hasSneezed = false;
	public bool startFromZero = true;
	public bool reset = false;
	public Canvas levelCompleteCanvas;
	//public Animator boss;
	public Animator gameScreen;
	public GameObject homeButton;
	public GameObject retryButton;
	public GameObject quitButton;
	public GameObject boss;
	public MusicLibrary music;
	public Text hoursWorkedUI;
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

	public float addWages(int timeWorked) {
		List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
		daysWage = hourlyRate * timeWorked;
		wages.Add (daysWage);
		PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
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
			retryButton.SetActive(false);
			homeButton.SetActive(true);

			if (fullday) {
				//Normal Day at Work
				//TODO: Start Animation that shows amount worked / wage potential

				Debug.Log ("Now Day " + currentLevel);

				if (currentLevel%5 == 0) {
					Debug.Log("Weekend");
					boss.GetComponent<CharacterDialog>().changeSpeechKey("weekend");
					
				}
				else {
					boss.GetComponent<CharacterDialog>().changeSpeechKey("complete");
					
				}
				
				//cue boss
				boss.GetComponent<Animator>().SetBool("talkAgain", true);
				boss.GetComponent<Animator>().SetBool("finishedTalking", false);

				
			}
			else {
				//too late

				if (sick) {
					boss.GetComponent<CharacterDialog>().changeSpeechKey("sneezed");
				}
				else {
					boss.GetComponent<CharacterDialog>().changeSpeechKey("slow");					
				}

				int warnings = PlayerPrefs.GetInt ("warnings");
				warnings+=1;
				int maxWarnings = PlayerPrefs.GetInt("max warnings");
				PlayerPrefs.SetInt("warnings", warnings);
				if (warnings >= maxWarnings) {
					//TODO: Fire Player
					Debug.Log("Player should be fired");
					hoursWorkedUI.text = "Fired";
					PlayerPrefs.SetInt("fired",1);
					boss.GetComponent<CharacterDialog>().changeSpeechKey("fired");
					quitButton.SetActive(true);
					homeButton.SetActive(false);
				}
			}
		}

	}

	public void EndOfLevel(bool _fullday, bool _endless, bool _sick) {
		fullday = _fullday;
		endless = _endless;
		sick = _sick;
		Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
		Time.timeScale = 0;
//		int currentLevel = PlayerPrefs.GetInt("current level");
		PlayerPrefs.SetInt("tutorial", 1);
		Debug.Log("End of Level Ran");
		levelCompleteCanvas.enabled = true;

		if (endless) {
			retryButton.SetActive(true);
			quitButton.SetActive(true);

				if (sick) {
					//ended because of sickness
					hoursWorkedUI.text = "Sick on the Job";

				}
				else {
					//ended because of too many orders
					hoursWorkedUI.text = "Too Slow";

				}

		}


		else if(fullday) {
			Debug.Log ("running wage update");
			hoursWorkedUI.text = "Full Day of Work";
			wagesUI.text = "$" + addWages(8).ToString() + " added to your next paycheck";
			music.levelCompleted();
			Camera.main.audio.Stop ();

		}

		else if(sick) {
			//ended because of sickness
			wagesUI.text = "$" + daysWage.ToString() + " added to your next paycheck.";
			hoursWorkedUI.text = "Sick on the Job";
			music.levelFailed();
			Camera.main.audio.Stop ();

		}
		
		else {
			wagesUI.text = "$" + daysWage.ToString() + " added to your next paycheck.";

			hoursWorkedUI.text = "Too Slow";
			Debug.Log("SLOW UPDATE SPEECH");
			music.levelFailed();
			Camera.main.audio.Stop ();

		}

		gameScreen.SetBool("ended", true);
	
	}

	void Start() {
		//checks if values need to be reset
		resetData();
//		float[] wages = PlayerPrefsX.GetFloatArray("wages");

		if (PlayerPrefs.HasKey("pills")) {
			pills = PlayerPrefs.GetInt("pills");
		}
		else {
			PlayerPrefs.SetInt("pills", 0);
		}
		if (PlayerPrefs.HasKey("sneezed")) {
			hasSneezed = true;
		}
		else {
			PlayerPrefs.SetInt("sneezed", 1);
		}

	
		if (PlayerPrefs.HasKey("money")) {
			money = PlayerPrefs.GetFloat ("money");
			Debug.Log("Have Money : " + money);
		}
		else {
			Debug.Log("No Money");
			PlayerPrefs.SetFloat("money", money);
		}

		if (moneyUI) {
			moneyUI.text = money.ToString();
			Debug.Log("Set Money");
		}

		if (pillsUI) {
			pillsUI.text = pills.ToString();
		}

	}

	void Awake() {
		//race... not working, need a better system for this
		if (PlayerPrefs.GetInt("highest level") > 2) {
			GameObject[] unlockables = GameObject.FindGameObjectsWithTag("Unlockable");
			for (int i = 0; i < unlockables.Length; i++) {
				Debug.Log("Unlocking..." + i);
				unlockables[i].GetComponent<ButtonScript>().setVisibility(true);
			}
		}

	}



}
