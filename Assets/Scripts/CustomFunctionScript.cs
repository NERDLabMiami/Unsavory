using UnityEngine;
using System.Collections;

public class CustomFunctionScript : MonoBehaviour {

	public static void resetPlayerData(float health, int money) {
		Debug.Log("Resetting Player Data");
		PlayerPrefs.DeleteAll();
		PlayerPrefs.SetInt("current level", 1);
		PlayerPrefs.SetFloat("health", health);
		PlayerPrefs.SetInt ("money", money);
		PlayerPrefs.SetInt ("sneezes", 0);

	}
}
