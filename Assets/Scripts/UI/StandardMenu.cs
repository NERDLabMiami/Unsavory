using UnityEngine;
using System.Collections;

public class StandardMenu : MonoBehaviour {
	public GameObject objectsToDisable;

	// Use this for initialization
	void Start () {
		objectsToDisable.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void reenableObjects() {
		objectsToDisable.SetActive(true);
	}
}
