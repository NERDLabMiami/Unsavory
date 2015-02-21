using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour {

	public float timer = 30;
	public bool endless = false;
	public DateTime today;
	private DateTime startOfWorkDay;
	public bool running = false;
	private bool waitingForPlayerInput = false;
	public bool catering = false;
	private Transform startingPosition;
	private float startTime;
	private float movementTime;
	private float speed = 1.0f;
	private float timeBetweenOrders = 3f;
	public GameObject player;


	void Start() {

		if (PlayerPrefs.HasKey("catering")) {
			catering = true;
		}

	}


	public void setWorkday(TimeSpan workday) {
		today = new DateTime(2015,1,1);
		today = today.Add(workday);
		startOfWorkDay = today;
		Debug.Log ("Setting Workday");

	}
	public string getClock() {
		today = today.AddSeconds(Time.deltaTime * 1080);
		return today.ToString("h:mm tt");
	}

	public int getTimeWorked() {
		TimeSpan timeWorked = today-startOfWorkDay;
		return  timeWorked.Hours;
	}

	// Update is called once per frame
	void Update () {
		if (running) {
			timer -= Time.deltaTime;
				if (timer <= 0 && !endless && !catering) {
					//Continue onto next day, full day of work
					player.GetComponent<PlayerScript>().EndOfLevel(true, false, false);
					running = false;
					Debug.Log("End of Day!");
				}
				//endless increase difficulty
				if (endless && timer <= 0) {
					timer = 30;
					timeBetweenOrders *= .95f;
					if (timeBetweenOrders <= .5f) {
						timeBetweenOrders = .5f;
					}
				}
		}
	}



}
