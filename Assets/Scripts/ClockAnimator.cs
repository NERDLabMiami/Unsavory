using UnityEngine;
using System;

public class ClockAnimator : MonoBehaviour {
	
	private const float
		hoursToDegrees = 360f / 48f,
		minutesToDegrees = 360f / 3.2f;
		public float startTime = 0f;
	public Transform hours, minutes;

	void Start() {
	
	}

	void Update () {
		startTime += Time.deltaTime;
			//rotate on Z
			hours.localRotation = Quaternion.Euler(0f,0f,startTime * -hoursToDegrees);
			minutes.localRotation =
				Quaternion.Euler(0f,0f,startTime * -minutesToDegrees);
	}
}