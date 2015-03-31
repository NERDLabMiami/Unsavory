using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
//using UnionAssets.FLE;

public class StartLevel : MonoBehaviour {
	public GameObject timer;
	//TODO: What's gametimer for
	public float gameTimer = 0;
	public bool timerOn = false;
	public GameObject nose;
	public GameObject tutor;
	public CharacterDialog dialog;
	public AudioClip gamePlayBackground;
	private bool levelBegan = false;
	public string appleId = "959296519";
	public string apdroidAppUrl = "market://details?id=com.dataplayed.unsavory";


	// Use this for initialization
	void Start () {
		int currentLevel = PlayerPrefs.GetInt("current level");
		int timesPlayed = PlayerPrefs.GetInt("times played", 0);
		if (timesPlayed == 3 && !PlayerPrefs.HasKey("rated")) {
			//Ask to rate the app
/*			MobileNativeRateUs ratePopUp =  new MobileNativeRateUs("Like this game?", "Rate it and let us know what you thnk!");
			ratePopUp.SetAppleId(appleId);
			ratePopUp.SetAndroidAppUrl(apdroidAppUrl);
			ratePopUp.OnComplete += OnRatePopUpClose;
			
			ratePopUp.Start();

*/
			PlayerPrefs.SetInt("rated",1);
		}
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
			Camera.main.GetComponent<AudioSource>().clip = gamePlayBackground;
			Camera.main.GetComponent<AudioSource>().Play();
			nose.GetComponent<NoseWipeScript>().sneezeAllowed = true;

		}

		float minutesLate = UnityEngine.Random.Range(0, lateness);
		TimeSpan workday = new TimeSpan(8,(int)minutesLate,0);

		//TODO: Broadcast Late Message to Update Boss Dialogue for arriving late
		timer.GetComponent<TimerScript>().setWorkday(workday);
		Debug.Log("Begin Level");
		levelBegan = true;
	}

	// Update is called once per frame
	void Update () {
		if (timerOn) {
			gameTimer += Time.deltaTime;
		}
	}
}
