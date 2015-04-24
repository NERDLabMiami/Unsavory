using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class BuyDrugsScript : MonoBehaviour {
	public int costOfDrugs = 5;
	public Text moneyUI;
	public Text pillsUI;
	public Text canvasTitle;
	private int pills;

	// Use this for initialization
	public void Start() {
		pills = PlayerPrefs.GetInt("pills",0);
		pillsUI.text = pills.ToString();
		canvasTitle.text = "You have " + pills.ToString() + " pills";
		setHealthMessage();
	}

	public void setHealthMessage() {
		float health = PlayerPrefs.GetFloat("health",0);
		if (health > 29) {
			canvasTitle.text = "Feeling Good!";
		}
		else if (health > 25) {
			canvasTitle.text = "Feeling Okay";

		}
		else if (health > 20) {
			canvasTitle.text = "Feeling Less Than Stellar";

		}
		else if (health > 15) {
			canvasTitle.text = "Feeling Nauseous";
		}

		else if (health > 10) {
			canvasTitle.text = "Feeling Awful";

		}
		else if (health > 5) {
			canvasTitle.text = "Feeling Horrible";

		}
		else {
			canvasTitle.text = "Definitely Sick";

		}
	}
	public void buy() {
		Debug.Log("Buying Drugs");

		float money = PlayerPrefs.GetFloat("money");
		if (money - costOfDrugs >= 0) {
			Debug.Log("Have Enough Money");
			money -= costOfDrugs;
			pills++;
			PlayerPrefs.SetInt("pills", pills);
			PlayerPrefs.SetFloat("money",money);
			moneyUI.text = "$" + money.ToString("0");
			pillsUI.text = pills.ToString();
			canvasTitle.text = "Cold Medicine Purchased";
		}
		else {
			canvasTitle.text = "You don't have enough money";
			Debug.Log("Money = " + money.ToString());
		}
		//TODO: figure out health change here vs. flag in the game round
	}

	public void take() {
		if (pills > 0) {
			Debug.Log("Taking Pill");
			pills--;
			PlayerPrefs.SetInt("pills", pills);
			pillsUI.text = pills.ToString();
			float health = PlayerPrefs.GetFloat("health");
			canvasTitle.text = "Feeling Slightly Better";

			if (health < 30) {
				Debug.Log("Unhealthy, taking pill");
				health += 1.0f;
				PlayerPrefs.SetFloat("health", health);
			}

			else {
				Debug.Log("Wasted pill, already healthy");
			}
		}
		else {
			canvasTitle.text = "You don't have any pills to take";

			Debug.Log("Not enough pills to take one");
		}

	}
}
