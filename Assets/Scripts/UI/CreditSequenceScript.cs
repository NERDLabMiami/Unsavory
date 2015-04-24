using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CreditSequenceScript : MonoBehaviour {
	public GameObject credit;
	public GameObject creditTemplate;
	private int currentCredit = 0;
	public string[] credits;

	// Use this for initialization
	void Start () {
		updateCredit();
	}

	private void updateCredit() {
		credit.GetComponent<Text>().text = credits[currentCredit];
		currentCredit++;
		if (currentCredit >= credits.Length) {
			currentCredit = 0;
		}
	}
	// Update is called once per frame
	void Update () {
		if (credit == null) {
			credit = (GameObject) Instantiate (creditTemplate);
			updateCredit();
			credit.transform.SetParent(gameObject.transform,false);
		}
	}
}
