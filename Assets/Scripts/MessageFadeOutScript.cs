using UnityEngine;
using System.Collections;

public class MessageFadeOutScript : MonoBehaviour {
	public float fadeOutTime = 10;

	void Start() {
		renderer.enabled = false;
	}
	void Begin () {
		renderer.enabled = true;
		Destroy (gameObject, fadeOutTime);
	}
	
}
