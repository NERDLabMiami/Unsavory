using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RecipeScript : MonoBehaviour {
	public List<GameObject> ingredients = new List<GameObject>();
	public GameObject completed;
	private Transform upperLimit;
	private Transform orderLine;
	private bool moveLeft = true;
	
	void setAlert() {
		gameObject.GetComponent<AudioSource>().Play();
	}
	
}
