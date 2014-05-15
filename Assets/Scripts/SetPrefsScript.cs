using UnityEngine;
using System.Collections;

public class SetPrefsScript : MonoBehaviour {
	public bool endless = false;

	void OnMouseDown() {
		if (endless) {
			PlayerPrefs.SetInt("endless", 1);
		}
		else {
			PlayerPrefs.SetInt("endless", 0);

		}
	}
}
