using UnityEngine;
using System.Collections;

public class HealthMessageScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		float health = PlayerPrefs.GetFloat("health");
		if (health >= 8) {
			gameObject.GetComponent<TextMesh>().text = "Feeling Like a Champ";
		}
		if (health > 6 && health < 8) {
			gameObject.GetComponent<TextMesh>().text = "There's a tickle in your throat";
		}
		if (health > 4 && health <= 6) {
			gameObject.GetComponent<TextMesh>().text = "You've got a runny nose";
		}
		if (health > 3 && health <= 4) {
			gameObject.GetComponent<TextMesh>().text = "You've got a cold";
		}
		if (health <= 3) {
			gameObject.GetComponent<TextMesh>().text = "You need to stay home";
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
