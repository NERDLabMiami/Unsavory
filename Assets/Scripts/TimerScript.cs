﻿using UnityEngine;
using System.Collections;

public class TimerScript : MonoBehaviour {

	public float timer = 30;
	// Update is called once per frame
	void Update () {
		timer -= Time.deltaTime;
		if (timer <= 0) {
			//TIMES UP!
			timer = 0;
			Application.LoadLevel(1);

		}

		gameObject.guiText.text = ((int)timer).ToString();
	}

}
