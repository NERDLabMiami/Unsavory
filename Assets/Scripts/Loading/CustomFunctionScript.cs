using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class CustomFunctionScript : MonoBehaviour {

	public static void resetPlayerData(float health, float money) {
		Debug.Log("Resetting Player Data");
		int timesPlayed = PlayerPrefs.GetInt("times played",0);
		bool paidSickDaysAchieved = false;
		bool usingPaidSickDays = false;
		bool twitter = false;
		bool survived = false;
		bool can_cater = false;
		if (PlayerPrefs.HasKey("can cater")) {
			can_cater = true;
		}
		if (PlayerPrefs.HasKey("paid sick days achieved")) {
			paidSickDaysAchieved = true;
		}
		if (PlayerPrefs.HasKey ("using paid sick days")) {
			usingPaidSickDays = true;
		}
		if (PlayerPrefs.HasKey("activated")) {
			twitter = true;
		}

		if (PlayerPrefs.HasKey("survived")) {
			survived = true;
		}

		PlayerPrefs.DeleteAll();
		PlayerPrefs.SetInt("times played", timesPlayed);
		PlayerPrefs.SetInt("current level", 1);
		PlayerPrefs.SetFloat ("money", money);
		PlayerPrefs.SetInt ("sneezes", 0);
		PlayerPrefs.SetInt ("warnings", 0);
		PlayerPrefs.SetInt("max warnings", 3);
		PlayerPrefs.SetInt ("paid sick days", 0);
		if(can_cater) {
			PlayerPrefs.SetInt("can cater", 1);
		}
		if (survived) {
			PlayerPrefs.SetInt("survived", 1);
		}

		if (paidSickDaysAchieved) {
			Debug.Log("Restoring Paid Sick Days");
			PlayerPrefs.SetInt ("paid sick days achieved",1);
		}
		if (twitter) {
			PlayerPrefs.SetInt("activated", 1);
		}
		if (usingPaidSickDays) {
			Debug.Log("Resetting Sick Days");
			PlayerPrefs.SetInt("using paid sick days",1);
			PlayerPrefs.SetInt("paid sick days", 0);
			PlayerPrefs.SetFloat("health", 32);

		}
		else {
			PlayerPrefs.SetFloat("health", health);
		}

	}

	public static void resetToDefaults() {
		PlayerPrefs.DeleteAll();
		PlayerPrefs.SetInt("current level", 1);
		PlayerPrefs.SetFloat ("money", 0);
		PlayerPrefs.SetInt ("sneezes", 0);
		PlayerPrefs.SetInt ("warnings", 0);
		PlayerPrefs.SetInt("max warnings", 3);
		PlayerPrefs.SetInt ("paid sick days", 0);
		PlayerPrefs.SetFloat("background volume", 1);
		PlayerPrefs.SetFloat("sfx volume", 1);

	}

	public void resetPlayer() {
		resetPlayerData(25,0);
	}
}
