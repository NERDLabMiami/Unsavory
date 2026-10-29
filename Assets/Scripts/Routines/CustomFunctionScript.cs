using UnityEngine;
using System.Linq;

public class CustomFunctionScript : MonoBehaviour {

	public static void resetPlayerData(float health, float money) {
		int timesPlayed = PlayerPrefs.GetInt("times played",0);
		bool paidSickDaysAchieved = false;
		bool usingPaidSickDays = false;
		bool twitter = false;
		bool survived = false;
		bool can_cater = false;
		int evaluationGroup = Random.Range(0,5);

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
		PlayerPrefs.SetInt("evaluation group", evaluationGroup);
		PlayerPrefs.SetInt("times played", timesPlayed);
		PlayerPrefs.SetInt("current level", 0);

		
		PlayerPrefs.SetFloat ("money", money);
		PlayerPrefs.SetInt ("sneezes", 0);
		PlayerPrefs.SetInt ("warnings", 0);
		PlayerPrefs.SetInt("max warnings", 2);
		PlayerPrefs.SetInt ("paid sick days", 0);
		if(can_cater) {
			PlayerPrefs.SetInt("can cater", 1);
		}
		if (survived) {
			PlayerPrefs.SetInt("survived", 1);
		}

		if (paidSickDaysAchieved) {
			PlayerPrefs.SetInt ("paid sick days achieved",1);
		}
		if (twitter) {
			PlayerPrefs.SetInt("activated", 1);
		}
		if (usingPaidSickDays) {
			PlayerPrefs.SetInt("using paid sick days",1);
			PlayerPrefs.SetInt("paid sick days", 0);
			PlayerPrefs.SetFloat("health", 60);

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
		PlayerPrefs.SetInt("max warnings", 2);
		PlayerPrefs.SetInt ("paid sick days", 0);
		PlayerPrefs.SetFloat("health", 20);
		PlayerPrefs.SetFloat("background volume", 1);
		PlayerPrefs.SetFloat("sfx volume", 1);

	}

	public static void collisionOnObjects(GameObject obj, bool collision) {
			foreach(BoxCollider2D child in obj.GetComponentsInChildren<BoxCollider2D>()) {
				child.enabled = collision;
			}
	}

	public void resetPlayer() {
		resetPlayerData(25,0);
	}
}
