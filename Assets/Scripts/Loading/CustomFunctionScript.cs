using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class CustomFunctionScript : MonoBehaviour {

	public static void resetPlayerData(float health, int money) {
		Debug.Log("Resetting Player Data");
		PlayerPrefs.DeleteAll();
		PlayerPrefs.SetInt("current level", 1);
//		PlayerPrefs.DeleteKey("wages");
//		PlayerPrefs.DeleteKey("sneezed");
		PlayerPrefs.SetFloat("health", health);
		PlayerPrefs.SetInt ("money", money);
		PlayerPrefs.SetInt ("sneezes", 0);
		List<string> effects = new List<string>();
		List<int> dueDates = new List<int>();
		PlayerPrefsX.SetStringArray("effects",effects.ToArray());

//		PlayerPrefs.DeleteKey("tutorial");
	}
}
