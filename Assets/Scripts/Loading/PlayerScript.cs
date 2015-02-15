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
	public bool hasBeenTutored;
	public int warningsBeforeFired = 3;
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
	public Text hoursWorkedUI;
	public Text wagesUI;

	public void resetData() {
		if (!PlayerPrefs.HasKey ("current level") || reset) {
			//this resets
			CustomFunctionScript.resetPlayerData(30, money);
		}

	}

	public void addWages(int timeWorked) {
		List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
		wages.Add (hourlyRate * timeWorked);
		PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
	}
	public void EndOfLevel(bool fullday, bool endless, bool sick) {
		Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
		Time.timeScale = 0;
		int currentLevel = PlayerPrefs.GetInt("current level");
		PlayerPrefs.SetInt("tutorial", 1);
		Debug.Log("End of Level Ran");
		levelCompleteCanvas.enabled = true;
		if (endless) {
			retryButton.SetActive(true);
			quitButton.SetActive(true);

			if (fullday) {
				//technically this is impossible
			}
			else {

				if (sick) {
					//ended because of sickness
				}
				else {
					//ended because of too many orders
				}

			}
		}

		if (!endless) {
			if (fullday) {
				//Normal Day at Work
				//TODO: Start Animation that shows amount worked / wage potential
				List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
				wages.Add (hourlyRate * 8);
				hoursWorkedUI.text = "8 Hours Worked";
				wagesUI.text = "$" + (hourlyRate * 8).ToString() + " added to your next paycheck";

				PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
				PlayerPrefs.SetInt ("current level", currentLevel+1);
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
				homeButton.SetActive(true);
//				Text responseText = homeButton.GetComponentInChildren<Text>();
//				responseText.text = jsonDialog[response_key][selectedIndex][dialogIndex];


			}
			else {
				//TODO: Fire after x many screw ups
				//TODO: Start Animation that shows amount worked / wage potential
				hoursWorkedUI.text = "8 Hours Worked";
				wagesUI.text = "$" + (hourlyRate * 8).ToString() + " added to your next paycheck";

				//RETRY: Start Level Again, No Penalties <- Setting Unlockable?"Poor Performance Allowed"
				//				retryButton.SetActive(true);
				//GO HOME: Give Up, Come Back
				//+1 Strike
				homeButton.SetActive(true);
				//
//				quitButton.SetActive(true);

				if (sick) {
					//sneezed
					//TODO: Change Dialogue for Sickness
					boss.GetComponent<CharacterDialog>().changeSpeechKey("sneezed");

				}
				else {
					//TODO: Change dialogue for too slow
					boss.GetComponent<CharacterDialog>().changeSpeechKey("slow");

				}
			}
		}

		if (endless) {
		}

		if(fullday && !endless) {

		}
		else if (!fullday && !endless) {
			//too slow, warning, fired after x amount of too slow?
		
		}
		gameScreen.SetBool("ended", true);

	}

	void Start() {
		//checks if values need to be reset
		resetData();
		float[] wages = PlayerPrefsX.GetFloatArray("wages");

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
