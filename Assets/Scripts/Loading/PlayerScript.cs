using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Linq;

public class PlayerScript : MonoBehaviour {
	public Text moneyUI;
	public Text pillsUI;
	private int money;
	private int pills;
	public float hourlyRate;
	public bool hasBeenTutored;
	public bool hasSneezed = false;
	public bool startFromZero = true;
	public bool reset = false;
	public Canvas endOfLevelCanvas;
	public Animator boss;
	public GameObject homeButton;
	public GameObject retryButton;
	public GameObject quitButton;

	public void resetData() {
		if (!PlayerPrefs.HasKey ("current level") || reset) {
			//this resets
			CustomFunctionScript.resetPlayerData(10, money);
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
				List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
				wages.Add (hourlyRate * 8);
				PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
				PlayerPrefs.SetInt ("current level", currentLevel+1);
				Debug.Log ("Now Day " + currentLevel);
				//cue boss
				boss.SetBool("talkAgain", true);
				boss.SetBool("finishedTalking", false);
				homeButton.SetActive(true);

			}
			else {
				boss.SetBool("finishedTalking", false);
				boss.SetBool("talkAgain", true);
//				retryButton.SetActive(true);
				homeButton.SetActive(true);
				quitButton.SetActive(true);

				if (sick) {
					//sneezed
					//TODO: Change Dialogue for Sickness
				}
				else {
					//TODO: Change dialogue for too slow
				}
			}
		}


		//		endOfLevelCanvas.enabled = true;
		if (endless) {
		}

		if(fullday && !endless) {

		}
		else if (!fullday && !endless) {
			//too slow, warning, fired after x amount of too slow?
		
		}
		
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
			money = PlayerPrefs.GetInt ("money");
			Debug.Log("Have Money : " + money);
		}
		else {
			Debug.Log("No Money");
			PlayerPrefs.SetInt ("money", money);
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
