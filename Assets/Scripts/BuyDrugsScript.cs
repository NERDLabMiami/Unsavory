using UnityEngine;
using System.Collections;

public class BuyDrugsScript : MonoBehaviour {
	public float costOfDrugs = 5.99f;

	// Use this for initialization
	void OnMouseDown() {

		float money = PlayerPrefs.GetFloat("money");
		if (money - costOfDrugs >= 0) {
			money -= costOfDrugs;
			PlayerPrefs.SetFloat("money",money);
		}
		//TODO: figure out health change here vs. flag in the game round
	}
}
