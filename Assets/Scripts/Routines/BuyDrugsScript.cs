using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class BuyDrugsScript : MonoBehaviour {
	public int costOfDrugs = 5;
	public Text moneyUI;
	public Text pillsUI;
	private int pills;

	// Use this for initialization
	public void Start() {
		pills = PlayerPrefs.GetInt("pills",0);
		pillsUI.text = pills.ToString();
	}

	public void buy() {
		Debug.Log("Buying Drugs");

		int money = PlayerPrefs.GetInt("money");
		if (money - costOfDrugs >= 0) {
			Debug.Log("Have Enough Money");
			money -= costOfDrugs;
			pills++;
			PlayerPrefs.SetInt("pills", pills);
			PlayerPrefs.SetInt("money",money);
			moneyUI.text = money.ToString();
			pillsUI.text = pills.ToString();
		}
		else {
			Debug.Log("Money = " + money.ToString());
		}
		//TODO: figure out health change here vs. flag in the game round
	}
}
