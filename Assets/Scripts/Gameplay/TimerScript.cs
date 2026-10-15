using UnityEngine;
using System;

public class TimerScript : MonoBehaviour {

	public float timer = 30;
	public Level level;
	public bool running = false;

	private Transform startingPosition;
	private DateTime today;
	private float fullday;
	private float startTime;
	private float movementTime;
	private float timeBetweenOrders = 3f;
	
	void Start() {
		fullday = timer;
	}
	
	public void setWorkday(TimeSpan workday) {
		today = new DateTime(2020,1,1);
		today = today.Add(workday);
	}
	public string getClock() {
		today = today.AddSeconds(Time.deltaTime * 540);
		return today.ToString("h:mm tt");
	}

	public float getTimeWorked() {

		float timeWorked = fullday - timer;
		float percentageOfFullDay = timeWorked / fullday;
		return percentageOfFullDay * 8;
	}

	// Update is called once per frame
	void Update () {
		if (running) {
			timer -= Time.deltaTime;
				if (timer <= 0) {
					//Continue onto next day, full day of work
					level.EndOfLevel(true, false);
					running = false;
				}
		}
	}
	
}
