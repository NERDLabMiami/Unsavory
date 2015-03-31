using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RecipeScript : MonoBehaviour {
	//public GameObject[] ingredients;
	public List<GameObject> ingredients = new List<GameObject>();
	private bool tutorialShouldRun = false;
	public GameObject tutorialObject;
	private Transform upperLimit;
	private Transform orderLine;
	private bool moveLeft = true;
	public string title;


	void Start() {
	
		gameObject.GetComponent<Rigidbody2D>().fixedAngle = true;
		orderLine = gameObject.transform;
//		Vector3 relativePos = GameObject.Find("OrderStopper").transform.position - gameObject.transform.position;
//		upperLimit = GameObject.Find ("Upper Limit").transform.GetComponent<Rigidbody2D>().transform;
	
//		relativePos.y = 0;
//		gameObject.GetComponent<Rigidbody2D>().AddForce(100 * relativePos);
}

	void Update() {
//		Vector3 upper = new Vector3(transform.position.x, upperLimit.position.y);

		if (moveLeft) {
			gameObject.GetComponent<Rigidbody2D>().velocity = -Vector3.right*10;
		}
		else {
			gameObject.GetComponent<Rigidbody2D>().velocity = -Vector3.right;

		}
		orderLine.position = new Vector3(gameObject.transform.position.x, orderLine.position.y, gameObject.transform.position.z);
		gameObject.transform.position = orderLine.position;
		//else {
		//	gameObject.GetComponent<Rigidbody2D>().velocity = new Vector3(-1,1,0);
			//			gameObject.GetComponent<Rigidbody2D>().velocity = Vector3.up;
			
		//}

		//if (gameObject.transform.position.y > upper.y) {
//			gameObject.GetComponent<Rigidbody2D>().velocity = new Vector3(-1,-.1f,0);

		//}
		//else {

		//}


	}
	public void setTutorialActive() {
		tutorialShouldRun = true;
	}

	public void setTutorialObject(GameObject tutor) {
		tutorialObject = tutor;
	}
	

	void OnCollisionEnter2D(Collision2D other) {

		moveLeft = false;
//		other.relativeVelocity.Set(0,0);
//		gameObject.GetComponent<Rigidbody2D>().velocity.Set(0,0);
	}
}
