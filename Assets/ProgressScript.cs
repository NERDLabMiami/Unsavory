﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProgressScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int daysLeft = 21 - PlayerPrefs.GetInt("current level", 0);
		if (daysLeft < 20) {
			float money = PlayerPrefs.GetFloat("money");
			GetComponent<Text>().text = money.ToString ("$0.00") + " with " + daysLeft.ToString("0 days left");
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
