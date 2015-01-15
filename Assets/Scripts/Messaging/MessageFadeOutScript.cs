using UnityEngine;
using System.Collections;

public class MessageFadeOutScript : MonoBehaviour {
	public float fadeOutTime = 10;

	void Start() {
		GetComponent<Renderer>().enabled = false;
	}
	void Begin () {
		GetComponent<Renderer>().enabled = true;
		Destroy (gameObject, fadeOutTime);
	}
	
}
