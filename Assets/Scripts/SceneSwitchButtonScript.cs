using UnityEngine;
using System.Collections;

public class SceneSwitchButtonScript : MonoBehaviour {
	public Transform newCameraPosition;
	private Transform startingPosition;
	private float movementTime;
	private float startTime;
	private bool moving = false;
	private float speed = 1.0f;


	void Update() {
		if (moving) {
			float distanceCovered = (Time.realtimeSinceStartup - startTime) * speed;
			float fracJourney = distanceCovered / movementTime;
			Camera.main.transform.position = Vector3.Lerp (startingPosition.position, newCameraPosition.position, fracJourney);

			if (Vector3.Distance (newCameraPosition.transform.position, Camera.main.transform.position) <= .1) {
				moving = false;
				Debug.Log("Stopped Moving");
			}
		}	
	}

	void OnMouseDown() {
		startingPosition =  Camera.main.transform;
		moving = true;
		startTime = Time.time;
		movementTime = Vector3.Distance(startingPosition.position, newCameraPosition.position);

	}

}
