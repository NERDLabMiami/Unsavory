using UnityEngine;
using System.Collections;

public class StrikeScript : MonoBehaviour {
	public int strikesLeft = 3;

	// Update is called once per frame
	void Update () {
		gameObject.guiText.text = strikesLeft.ToString();
		if (strikesLeft <= 0) {
			Application.LoadLevel(1);
		}
	}
}
