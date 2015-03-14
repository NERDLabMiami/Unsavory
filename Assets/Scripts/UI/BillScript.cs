using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine.UI;

public class BillScript : MonoBehaviour {
	public int id;
	public int amount;
	public string title;
	public string effect;
	public bool overdue = false;
	public Text titleUI;
	public Text amountUI;
	public Text dueUI;
	public Button payButton;
	public Text bankAccount;
	private float money;
	public int dueDate = 0;
	private int effectAmount = 0;
	private bool paid = false;
	private bool newlyDisabled = false;

	// Use this for initialization
	void Start () {
		money = PlayerPrefs.GetFloat("money");
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		dueDate = bills["bills"][id]["due"].AsInt;
		title = bills["bills"][id]["title"];
		effect = bills["bills"][id]["effect"];
		amount = bills["bills"][id]["amount"].AsInt;
		if (PlayerPrefs.GetInt("bill" + id,0) == 1) {
			paid = true;
		}

		set ();
	}

	public void set() {
		titleUI.text = title;
		amountUI.text = "$" + amount.ToString("0.00");

		int currentDay = PlayerPrefs.GetInt("current level");
		Debug.Log("I Have " + money);
		if (money > amount) {
			Debug.Log("Enabling bill because I have enough money");
			payButton.enabled = true;
		}
		else {
			Debug.Log("Disabling");

			payButton.enabled = false;
			payButton.gameObject.GetComponent<Animator>().SetTrigger("disable");
		}

		if (paid) {
			payButton.GetComponentInChildren<Text>().text = "Paid";
			payButton.gameObject.GetComponent<Animator>().SetTrigger("disable");
			payButton.interactable = false;
			//payButton.gameObject.SetActive(false);
			dueUI.text = "";

		}

		else if (dueDate > currentDay) {
			//due in a bit
			dueUI.text = "Due in " + ( dueDate - currentDay).ToString() + " days";
		}
		else if (dueDate == currentDay) {
			//due today
			dueUI.text = "Due Today";
		}
		else {
			//overdue
			dueUI.text = "OVERDUE!";
			effectAmount = currentDay - dueDate;
		}

	}
	// Update is called once per frame
	void Update () {
		if (money > amount) {
			payButton.enabled = true;
		}
		else {
			if (!newlyDisabled) {
				payButton.gameObject.GetComponent<Animator>().SetTrigger("disable");
				newlyDisabled = true;
			}
			payButton.interactable = false;
			payButton.enabled = false;

		}

	}

	public void payBill() {
		money = PlayerPrefs.GetFloat("money");

		if (money - amount < 0) {
		//TODO: Visual update for not being able to pay the bill
			Debug.Log("Have : " + money + " need " + amount);
		}
		else {
			//Pay Amount Required
			Debug.Log ("Paying bill");
			money = money - amount;
			PlayerPrefs.SetFloat("money", money);
			GameObject player = GameObject.Find("/Player");
			player.GetComponent<PlayerScript>().moneyUI.text = money.ToString();
			//TODO: When going another month, reset current day, add month
			PlayerPrefs.SetInt("bill" + id,1);
			//TODO: Animate to remove
			//payButton.gameObject.SetActive(false);

			//remove effect
			int currentEffect = PlayerPrefs.GetInt(effect, 0);
			PlayerPrefs.SetInt(effect, effectAmount - currentEffect);
			dueUI.text = "";
			payButton.GetComponentInChildren<Text>().text = "Paid";
			payButton.gameObject.GetComponent<Animator>().SetTrigger("disable");

			//TODO: Animation for Paying Bill / Bill Paid Swap Out Panel
			paid = true;
			bankAccount.text = money.ToString("Account Balance: $0.00");
		}
	}

}
