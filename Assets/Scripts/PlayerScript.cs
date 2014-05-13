using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {
	public float health;
	public int money;
	public bool hasBeenTutored;

	void Start() {
		//check defaults
		PlayerPrefs.SetInt("highest level", 4);
		if (PlayerPrefs.HasKey("health")) {
			health = PlayerPrefs.GetFloat("health");
		}
		else {
			PlayerPrefs.SetFloat("health", health);
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
