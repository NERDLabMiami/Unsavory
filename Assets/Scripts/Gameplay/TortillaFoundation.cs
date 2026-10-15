using UnityEngine;
using System.Collections;

public class TortillaFoundation : MonoBehaviour, IWorldInteractable {
	public GameObject trays;
	public bool tutoring = false;

	// Use this for initialization
	void Start () {
		// The plate now starts with a tortilla, so trays are no longer locked behind it.
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Interact() { }

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
