using UnityEngine;
using System.Collections;

public class MovingMessageScript : MonoBehaviour {
	public string text;
	public float distanceToTravel = 10f;
	public float speed = .01f;
	private float traveled = 0f;

	private Vector3 originalPosition;
	private Vector3 endPosition;
	// Use this for initialization
	void Start () {
		originalPosition = transform.position;

		endPosition = new Vector3(transform.position.x - distanceToTravel, transform.position.y, transform.position.z);
		for (int i = 0; i < gameObject.GetComponentsInChildren<TextMesh>().Length; i++) {
			gameObject.GetComponentsInChildren<TextMesh>()[i].text = text;
		}

	}

	void Update() {
		if (traveled < 1f) {
			traveled += speed;
			transform.position = Vector3.Lerp (originalPosition, endPosition,traveled);
		}
		else {
			Destroy (gameObject);
		}
	}
}
