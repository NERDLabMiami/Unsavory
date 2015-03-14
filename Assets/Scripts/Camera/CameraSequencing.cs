using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraSequencing : MonoBehaviour {
	public GameObject preStartObject;
	public GameObject tutorObject;
	// Use this for initialization
	void Start () {
	}

	public void startCountdown() {
		if (PlayerPrefs.HasKey("tutorial")) {
			preStartObject.SetActive(true);
		}
		else {
			tutorObject.SetActive(true);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
