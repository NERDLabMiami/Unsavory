using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;

public class StartLevel : MonoBehaviour {
	public GameObject timer;
	public float gameTimer = 0;
	public bool timerOn = false;
	public GameObject nose;
	public GameObject tutor;
	public CharacterDialog dialog;
	public MusicLibrary musicLibrary;
	private bool levelBegan = false;

	// Use this for initialization
	void Start () {
		Camera.main.GetComponent<AudioSource>().clip = musicLibrary.characterBackground;
		Camera.main.GetComponent<AudioSource>().Play();
		int currentLevel = PlayerPrefs.GetInt("current level");
		bool catering = PlayerPrefs.HasKey("catering");
		if (catering) {
			dialog.changeSpeechKey("catering");
		}
		else if (currentLevel == 1) {
			//first day of work
			Debug.Log("First Day of Work!");
			dialog.changeSpeechKey("welcome");
		}
		else {
			//return to work
			float lateness = PlayerPrefs.GetFloat("late effect", 0);
			if (lateness > 0) {
				Debug.Log("Late to work");
				dialog.changeSpeechKey("late_return");

			}
			else {
				Debug.Log("Returned to work on time");
				dialog.changeSpeechKey("return");

			}

		}

	}

	public void beginLevel() {
		float lateness = PlayerPrefs.GetFloat("late effect", 0);

		if (!levelBegan) {
			timerOn = true;
			Camera.main.GetComponent<AudioSource>().clip = musicLibrary.gameplayBackground;
			Camera.main.GetComponent<AudioSource>().Play();
			nose.GetComponent<NoseWipeScript>().sneezeAllowed = true;

		}

		float minutesLate = UnityEngine.Random.Range(0, lateness);
		TimeSpan workday = new TimeSpan(8,(int)minutesLate,0);

		//TODO: Broadcast Late Message to Update Boss Dialogue for arriving late
		timer.GetComponent<TimerScript>().setWorkday(workday);
		Debug.Log("Begin Level");

		if (PlayerPrefs.GetInt("endless") == 1) {
			timer.GetComponent<TimerScript>().endless = true;
		}
		else {
			timer.GetComponent<TimerScript>().endless = false;
		}
		levelBegan = true;
	}

	// Update is called once per frame
	void Update () {
		if (timerOn) {
			gameTimer += Time.deltaTime;
		}
	}
}
