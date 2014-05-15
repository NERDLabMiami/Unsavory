using UnityEngine;
using System.Collections;

public class PanPositionScript : MonoBehaviour {
	public Transform newCameraPosition;
	public bool moveOnLoad = false;
	private Transform startingPosition;
	private float movementTime;
	private float startTime;
	private bool moving = false;
	private float speed = 1.0f;



	void Update() {
		if (moveOnLoad) {
			move ();
		}
		if (moving) {
			float distanceCovered = (Time.realtimeSinceStartup - startTime) * speed;
			float fracJourney = distanceCovered / movementTime;

			Camera.main.transform.position = Vector2.Lerp (startingPosition.position, newCameraPosition.position, fracJourney);

			if (Vector2.Distance (newCameraPosition.transform.position, Camera.main.transform.position) <= .1) {
				moving = false;
				Debug.Log("Stopped Moving");
			}
		}	
	}

	void move() {
		moveOnLoad = false;
		startingPosition =  Camera.main.transform;
		startTime = Time.realtimeSinceStartup;
		movementTime = Vector2.Distance(startingPosition.position, newCameraPosition.position);
		moving = true;
	}

}
