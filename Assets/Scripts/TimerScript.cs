using UnityEngine;
using System.Collections;
using System;

public class TimerScript : MonoBehaviour {

	public float timer = 30;
	public int startHour = 8;
	public int startMinute = 0;
	private TimeSpan workday;
	private DateTime today;

	void Start() {
		workday = new TimeSpan(8,0,0);
		today = new DateTime(2014,1,1);
		today = today.Add(workday);
	}

	// Update is called once per frame
	void Update () {

		timer -= Time.deltaTime;
		today = today.AddSeconds(Time.deltaTime * 1080);


		if (timer <= 0) {
			//TIMES UP!
			timer = 0;
			bool taintedFood = false;
			GameObject[] trays = GameObject.FindGameObjectsWithTag("Tray");
			for (int i = 0; i < trays.Length; i++) {
				if (trays[i].GetComponent<AddIngredientScript>().getTainted()) {
					taintedFood = true;
				}
			}
			if (taintedFood) {
				PlayerPrefs.SetString ("Game Over Message", "You Got Paid and Spread Your Disease!");

			}
			else {
				PlayerPrefs.SetString ("Game Over Message", "Good Job! You Get Paid!");

			}

			Debug.Log("The End Time is: " + today);
			Application.LoadLevel(1);
		}
		gameObject.GetComponent<TextMesh>().text = today.ToString("h:mm tt");
	}


}
