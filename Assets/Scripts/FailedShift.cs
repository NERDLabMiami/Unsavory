using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FailedShift : MonoBehaviour {

	public Button retryButton;
	private int retries = 0;

	// Use this for initialization
	void Start () {
		retries = TacoLedger.Trust;
		//TODO: Set Existing Taco Hearts to Full


		if (retries > 0) {
			retryButton.interactable = true;
		}
		else {
			retryButton.interactable = false;
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
