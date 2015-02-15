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
	public MusicLibrary musicLibrary;

	// Use this for initialization
	void Start () {
		Camera.main.GetComponent<AudioSource>().clip = musicLibrary.characterBackground;
		Camera.main.GetComponent<AudioSource>().Play();
	}

	public void beginLevel() {
		timerOn = true;
		Camera.main.GetComponent<AudioSource>().clip = musicLibrary.gameplayBackground;
		Camera.main.GetComponent<AudioSource>().Play();
		nose.GetComponent<NoseWipeScript>().sneezeAllowed = true;
		List<string> effects = new List<string>();
		effects.AddRange(PlayerPrefsX.GetStringArray("effects"));
		
		Debug.Log(effects.Count + " effects are being applied.");
		int healthEffect = 0;
		int lateness = 0;
		for (int i = 0; i < effects.Count; i++) {
			if (effects[i].Equals("health")) {
				healthEffect++;
			}
			else if (effects[i].Equals("bus")) {
				//TODO: Figure out proper amount to add for each lateness count. Too late = fired?
				lateness++;
			}
			else {
				Debug.Log("disregarded " + effects[i]);
			}
		}
		//sneezetimer script is looking at this as well on startup. need to redo to remove the race condition
		float health = PlayerPrefs.GetFloat("health");
		health = health - healthEffect;
		PlayerPrefs.SetFloat("health", health);
		
		Debug.Log("LATENESS MULTIPLIER: " + lateness + " HEALTH MULTIPLIER: " + healthEffect);
		int minutesLate = UnityEngine.Random.Range(0, lateness);
		TimeSpan workday = new TimeSpan(8,minutesLate,0);

		//TODO: Broadcast Late Message to Update Boss Dialogue for arriving late
		timer.GetComponent<TimerScript>().setWorkday(workday);
		Debug.Log("Begin Level");
		//		startingPosition = Camera.main.transform;
		
		if (PlayerPrefs.GetInt("endless") == 1) {
			timer.GetComponent<TimerScript>().endless = true;
		}
		else {
			timer.GetComponent<TimerScript>().endless = false;
		}

		if (PlayerPrefs.HasKey("tutorial")) {
//			spawnMoreOrders = true;
			tutor.GetComponent<TutorScript>().orderHopper.GetComponent<CurrentOrderScript>().spawnMoreOrders = true;
			Debug.Log("Turning off tutorial");
		}
		else {
			tutor.SetActive(true);
			Debug.Log("Running Tutorial");
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (timerOn) {
			gameTimer += Time.deltaTime;
		}
	}
}
