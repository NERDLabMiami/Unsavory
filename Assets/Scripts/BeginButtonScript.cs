using UnityEngine;
using System.Collections;

public class BeginButtonScript : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		if (PlayerPrefs.GetInt("current level") > 1) {
			Debug.Log("CURRENT LEVEL: " + PlayerPrefs.GetInt("current level"));
			GetComponent<TextMesh>().text = "day " + PlayerPrefs.GetInt("current level");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
