using UnityEngine;
using System;

public class TimerScript : MonoBehaviour {

	public float timer = 30;
	public Level level;
	public bool running = false;

	private Transform startingPosition;
	private float fullday;
	private float startTime;
	private float movementTime;
	private float timeBetweenOrders = 3f;
	
	void Start() {
		fullday = timer;
	}
	
	// The shift runs from the arrival time to 5 PM on the clock, however late the player got in.
	static readonly TimeSpan ShiftEnd = new TimeSpan(17, 0, 0);

	public void setWorkday(TimeSpan arrival) {
		shiftStart = arrival;
	}

	TimeSpan shiftStart = new TimeSpan(9, 0, 0);

	public string getArrivalClock() {
		return (new DateTime(2020,1,1) + shiftStart).ToString("h:mm tt");
	}

	// 5:00 PM lands exactly when the shift timer runs out.
	public string getClock() {
		float progress = fullday > 0f ? Mathf.Clamp01((fullday - timer) / fullday) : 0f;
		double minutes = shiftStart.TotalMinutes + (ShiftEnd.TotalMinutes - shiftStart.TotalMinutes) * progress;
		return (new DateTime(2020,1,1).AddMinutes(minutes)).ToString("h:mm tt");
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
					level.EndRound(RoundOutcome.Complete);
					running = false;
				}
		}
	}
	
}
