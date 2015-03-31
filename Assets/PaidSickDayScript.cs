using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class PaidSickDayScript : MonoBehaviour {
	public Text description;
	// Use this for initialization
	void Start () {
		if (PlayerPrefs.HasKey("using paid sick days")) {
			int paidSickDaysAvailable = PlayerPrefs.GetInt("paid sick days", 0);
			description.text = "You have " + paidSickDaysAvailable + " paid sick days available. Take one?";
		}
		else {

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
