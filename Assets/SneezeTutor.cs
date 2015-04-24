using UnityEngine;
using System.Collections;

public class SneezeTutor : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0f;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void EndSneezeTutorial() {
		Time.timeScale = 1.0f;
		Destroy (this.gameObject);
	}
}
