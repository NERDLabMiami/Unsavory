using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RecipeScript : MonoBehaviour {
	//public GameObject[] ingredients;
	public List<GameObject> ingredients = new List<GameObject>();
	private bool tutorialShouldRun = false;
	public GameObject tutorialObject;
	public string title;


	void Start() {
		Vector3 relativePos = GameObject.Find("OrderStopper").transform.position - gameObject.transform.position;
		relativePos.y = 0;
		gameObject.rigidbody2D.AddForce(50 * relativePos);
	}

	public void setTutorialActive() {
		tutorialShouldRun = true;
	}

	public void setTutorialObject(GameObject tutor) {
		tutorialObject = tutor;
	}
	

	void OnCollisionEnter2D(Collision2D other) {
		other.relativeVelocity.Set(0,0);
		gameObject.rigidbody2D.velocity.Set(0,0);
	}
}
