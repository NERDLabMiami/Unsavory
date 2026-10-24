using UnityEngine;
using System;
using System.Collections;

// The shift is measured in orders, not seconds: every resolved order moves the clock one step from
// the arrival time toward 5 PM, and the day ends when the last order is resolved.
public class TimerScript : MonoBehaviour {

	public Level level;
	public bool running = false;

	// The shift runs from the arrival time to 5 PM on the clock, however late the player got in.
	static readonly TimeSpan ShiftEnd = new TimeSpan(17, 0, 0);
	const float EndDelay = 1f;   // lets the last clock change finish fading before the day ends

	TimeSpan shiftStart = new TimeSpan(9, 0, 0);
	int totalOrders = 8;
	int resolvedOrders;
	float wipeWindow;
	bool dayEnded;

	public void setWorkday(TimeSpan arrival) {
		shiftStart = arrival;
	}

	public void SetTotalOrders(int total) {
		totalOrders = Mathf.Max(1, total);
	}

	// Seconds a clock step is worth (one order's pace): the time a sneezing player has to wipe their nose.
	public void SetWipeWindow(float seconds) {
		wipeWindow = seconds;
	}

	public int TotalOrders { get { return totalOrders; } }
	public float WipeWindow { get { return wipeWindow; } }
	public int ResolvedOrders { get { return resolvedOrders; } }

	public string getArrivalClock() {
		return (new DateTime(2020,1,1) + shiftStart).ToString("h:mm tt");
	}

	// Each order resolved (served, timed out or thrown away) moves the clock one even step.
	public string getClock() {
		double minutes = shiftStart.TotalMinutes + (ShiftEnd.TotalMinutes - shiftStart.TotalMinutes) * Progress();
		return (new DateTime(2020,1,1).AddMinutes(minutes)).ToString("h:mm tt");
	}

	public float getTimeWorked() {
		return Progress() * 8;
	}

	float Progress() {
		return Mathf.Clamp01((float)resolvedOrders / totalOrders);
	}

	// The day ends once the last clock change has faded and any sneeze that was already building has played out:
	// the player wipes it, or it ends the round as a sick day (SneezeScript).
	IEnumerator EndWhenNoseIsClear() {
		yield return new WaitForSecondsRealtime(EndDelay);
		while (level != null && level.nose != null && level.nose.IsMidSneeze) yield return null;
		if (level != null) level.EndRound(RoundOutcome.Complete);
	}

	public void AdvanceOrder() {
		if (dayEnded) return;
		resolvedOrders++;
		if (level != null && level.nose != null) level.nose.OrderAdvanced(resolvedOrders);
		if (level != null) level.SetHudClock(getClock(), true);
		if (resolvedOrders >= totalOrders) {
			dayEnded = true;
			running = false;
			if (level != null && level.nose != null) level.nose.WindDown();
			StartCoroutine(EndWhenNoseIsClear());
		}
	}
}
