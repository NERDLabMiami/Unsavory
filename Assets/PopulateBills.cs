
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class PopulateBills : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int day = PlayerPrefs.GetInt("current level");
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		int[] dueDates = PlayerPrefsX.GetIntArray("due");
		List<string> effects = new List<string>();
		int index = 0;
		foreach(BillScript child in GetComponentsInChildren<BillScript>()) {
			bool overdue = false;

			if (dueDates[index] <= day) {
				overdue = true;
				effects.Add(bills["bills"][index]["effect"]);
			}

			child.amount = bills["bills"][index]["amount"].AsInt;
			child.id = index;
			child.title = bills["bills"][index]["title"];
			child.effect = bills["bills"][index]["effect"];
			child.overdue = overdue;
			child.set();
			index++;	
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
