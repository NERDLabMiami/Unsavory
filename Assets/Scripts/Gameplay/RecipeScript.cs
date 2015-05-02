using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RecipeScript : MonoBehaviour {
	public List<GameObject> ingredients = new List<GameObject>();
	public GameObject completed;
	private Transform upperLimit;
	private Transform orderLine;
	private bool moveLeft = true;



	void Start() {
	
		gameObject.GetComponent<Rigidbody2D>().fixedAngle = true;
		orderLine = gameObject.transform;
}

	void setAlert() {
		gameObject.GetComponent<AudioSource>().Play();
	}

	void Update() {

		if (moveLeft) {
			gameObject.GetComponent<Rigidbody2D>().velocity = -Vector3.right*10;
		}
		else {
			gameObject.GetComponent<Rigidbody2D>().velocity = -Vector3.right;

		}
		orderLine.position = new Vector3(gameObject.transform.position.x, orderLine.position.y, gameObject.transform.position.z);
		gameObject.transform.position = orderLine.position;



	}


	void OnCollisionEnter2D(Collision2D other) {

		moveLeft = false;
	}
}
