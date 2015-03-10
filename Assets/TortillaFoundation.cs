using UnityEngine;
using System.Collections;

public class TortillaFoundation : MonoBehaviour {
	public GameObject trays;

	// Use this for initialization
	void Start () {
		lockTrays();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private void OnMouseDown() {
		unlockTrays();
	}

	public void lockTrays() {
		foreach(BoxCollider2D child in trays.GetComponentsInChildren<BoxCollider2D>()) {
			child.enabled = false;
		 }
		GetComponent<BoxCollider2D>().enabled = true;
	}

	public void unlockTrays() {
		foreach(BoxCollider2D child in trays.GetComponentsInChildren<BoxCollider2D>()) {
			child.enabled = true;
		}

	}
}
