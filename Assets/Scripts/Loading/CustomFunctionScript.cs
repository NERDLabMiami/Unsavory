using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class CustomFunctionScript : MonoBehaviour {

	public static void resetPlayerData(float health, float money) {
		Debug.Log("Resetting Player Data");
		PlayerPrefs.DeleteAll();
		PlayerPrefs.SetInt("current level", 1);
		PlayerPrefs.SetFloat("health", health);
		PlayerPrefs.SetFloat ("money", money);
		PlayerPrefs.SetInt ("sneezes", 0);
		PlayerPrefs.SetInt ("warnings", 0);
		PlayerPrefs.SetInt("max warnings", 3);
	}
	public void resetPlayer() {
		resetPlayerData(30,0);
	}
}
