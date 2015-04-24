using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine.UI;

public class TimerScript : MonoBehaviour {

	public float timer = 30;
	public DateTime today;
	public Level level;
	public bool running = false;
	public bool catering = false;

	private Transform startingPosition;
	private float fullday;
	private float startTime;
	private float movementTime;
	private float timeBetweenOrders = 3f;


	void Start() {
		fullday = timer;
		if (PlayerPrefs.HasKey("catering")) {
			catering = true;
		}

	}


	public void setWorkday(TimeSpan workday) {
		today = new DateTime(2015,1,1);
		today = today.Add(workday);
		//TODO: don't think I need this
//		startOfWorkDay = today;
		Debug.Log ("Setting Workday");

	}
	public string getClock() {
//		today = today.AddSeconds(Time.deltaTime * 1080);
		//voodoo math
		today = today.AddSeconds(Time.deltaTime * 540);
		return today.ToString("h:mm tt");
	}

	public float getTimeWorked() {

//		float fullday = 60;
		float timeWorked = fullday - timer;
		float percentageOfFullDay = timeWorked / fullday;
		return percentageOfFullDay * 8;

//		return  timeWorked.Hours;
	}

	// Update is called once per frame
	void Update () {
		if (running) {
			timer -= Time.deltaTime;
				if (timer <= 0 && !catering) {
					//Continue onto next day, full day of work
					level.EndOfLevel(true, false);
					running = false;
					Debug.Log("End of Day!");
				}
				//endless increase difficulty
				if (catering && timer <= 0) {
					timer = 30;
					timeBetweenOrders *= .85f;
					Debug.Log("Increasing Difficulty");
					if (timeBetweenOrders <= .5f) {
						timeBetweenOrders = .5f;
					}
				}
		}
	}



}
