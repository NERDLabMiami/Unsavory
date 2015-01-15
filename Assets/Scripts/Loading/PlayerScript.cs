using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour {
	public float health;
	public int money;
	public bool hasBeenTutored;
	public bool hasSneezed = false;
	public bool startFromZero = true;
	public bool reset = false;

	void Start() {

//TODO: reset, this is for testing
	

		if (!PlayerPrefs.HasKey ("current level") || reset) {
			//this resets
			PlayerPrefs.SetInt("current_level",1);
			CustomFunctionScript.resetPlayerData(health, money);
		}

		float[] wages = PlayerPrefsX.GetFloatArray("wages");
		//check defaults
		if (PlayerPrefs.HasKey("health")) {
			health = PlayerPrefs.GetFloat("health");
			Debug.Log("Has Health: " + health);
		}
		else {
			Debug.Log("No Health Set, Using Default of " + health);
			PlayerPrefs.SetFloat("health", health);
		}

		if (PlayerPrefs.HasKey("sneezed")) {
			hasSneezed = true;
		}
		else {
			PlayerPrefs.SetInt("sneezed", 1);
		}

	

		if (PlayerPrefs.HasKey("money")) {
			money = PlayerPrefs.GetInt ("money");
		}
		else {
			PlayerPrefs.SetInt ("money", money);
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
