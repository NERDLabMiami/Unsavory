using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraSequencing : MonoBehaviour {
	public GameObject preStartObject;

	// Use this for initialization
	void Start () {
	
	}

	public void enableGamePlay() {
		preStartObject.SetActive(true);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
