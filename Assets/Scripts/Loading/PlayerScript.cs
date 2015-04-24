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
	public bool reset = false;
//	public float daysWage;

	public void resetData() {
		if (!PlayerPrefs.HasKey ("current level") || reset) {
			//this resets
			CustomFunctionScript.resetPlayerData(60, money);
		}

	}

	public float potentialWages(float timeWorked) {
		Debug.Log("TIME WORKED: " + timeWorked);
		return hourlyRate * timeWorked;
	}

	public float addWages(float timeWorked) {
		float earnedWages = PlayerPrefs.GetFloat("earned wages",0);
		Debug.Log("Earned Wages Before: " + earnedWages);

		float daysWage = potentialWages(timeWorked);
		earnedWages += daysWage;
		PlayerPrefs.SetFloat("earned wages", earnedWages);
		Debug.Log("Earned Wages Now " + earnedWages);
		return daysWage;
	}

	public void cateringEvent() {
		PlayerPrefs.SetInt("catering",1);
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
