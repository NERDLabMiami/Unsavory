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

	public void resetData() {
		if (!PlayerPrefs.HasKey ("current level") || reset) {
			//this resets
			CustomFunctionScript.resetPlayerData(15, money);
		}

	}

	public void addWages(int timeWorked) {
		List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
		wages.Add (hourlyRate * timeWorked);
		PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
	}
	public void EndOfLevel(bool fullday, bool endless) {
		Debug.Log("End of Level Ran");

		//		endOfLevelCanvas.enabled = true;
		Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
		Time.timeScale = 0;
		int currentLevel = PlayerPrefs.GetInt("current level");
		PlayerPrefs.SetInt("tutorial", 1);
		if (endless) {
			
		}

		if(fullday) {
			List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
			wages.Add (hourlyRate * 8);
			PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
			PlayerPrefs.SetInt ("current level", currentLevel+1);
			Debug.Log ("Now Day " + currentLevel);
			//cue boss
			boss.SetBool("talkAgain", true);
		}
		else {
			//too slow
			boss.SetBool("talkAgain", true);

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
