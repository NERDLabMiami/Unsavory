using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class ShiftCompleteScript : MonoBehaviour {
	public Text fact;
	public FactScript factSheet;

	void Start() {
		if (fact) {
			fact.text = factSheet.randomFact;
		}
	}



}
