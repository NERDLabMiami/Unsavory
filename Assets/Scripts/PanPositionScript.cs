using UnityEngine;
using System.Collections;

public class PanPositionScript : MonoBehaviour {
	public Transform newCameraPosition;
	public GameObject nextPanEvent;
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

			Camera.main.transform.position = Vector3.Lerp (startingPosition.position, newCameraPosition.position, fracJourney);

			if (Vector3.Distance (newCameraPosition.transform.position, Camera.main.transform.position) <= .1) {
				moving = false;
				//queue next event
				if (nextPanEvent != null) {
					nextPanEvent.GetComponent<PanPositionScript>().move ();
				}
			}
		}	
	}

	public void move() {
		moveOnLoad = false;
		startingPosition =  Camera.main.transform;
		startTime = Time.realtimeSinceStartup;
		movementTime = Vector3.Distance(startingPosition.position, newCameraPosition.position);
		moving = true;
	}

}
