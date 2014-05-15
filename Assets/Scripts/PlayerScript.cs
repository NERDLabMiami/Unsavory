using UnityEngine;
using System.Collections;

public class PlayerScript : MonoBehaviour {
	public float health;
	public int money;
	public bool hasBeenTutored;

	void Start() {
		//TODO: reset, this is for testing
		PlayerPrefs.SetInt("highest level", 4);

		//check defaults

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

		if (!PlayerPrefs.HasKey ("current level")) {
			PlayerPrefs.SetInt("current level", 0);
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
