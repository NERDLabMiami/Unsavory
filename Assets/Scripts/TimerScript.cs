using UnityEngine;
using System.Collections;
using System;

public class TimerScript : MonoBehaviour {

	public float timer = 30;
	public int startHour = 8;
	public int startMinute = 0;
	public Transform endOfDayCameraPosition;
	public int mainMenuSceneNumber;
	public int homeSceneNumber;
	public GameObject endOfLevelTitle;
	public string endOfLevelTitleString;
	public string endOfLevelMessageString;
	public GameObject endOfLevelMessage;
	public bool endless = false;
	private TimeSpan workday;
	private DateTime today;
	private DateTime startOfWorkDay;
	private bool endOfDay = false;
	private bool waitingForPlayerInput = false;
	private Transform startingPosition;
	private float startTime;
	private float movementTime;
	private float speed = 1.0f;
	private float timeBetweenOrders = 3f;


	void Start() {
		workday = new TimeSpan(8,0,0);
		today = new DateTime(2014,1,1);
		today = today.Add(workday);
		startOfWorkDay = today;
		startingPosition = Camera.main.transform;

	}

	public string getClock() {
		today = today.AddSeconds(Time.deltaTime * 1080);
		return today.ToString("h:mm tt");
	}

	public string getTimeWorked() {
		TimeSpan timeWorked = today-startOfWorkDay;
		return  timeWorked.Hours.ToString();
	}

	// Update is called once per frame
	void Update () {
		if (!endOfDay) {
			timer -= Time.deltaTime;

			if (timer <= 0 && !endless) {
				//Continue onto next day
				PlayerPrefs.SetString ("ENDOFLEVEL_TITLE", endOfLevelTitleString);
				PlayerPrefs.SetString ("ENDOFLEVEL_MESSAGE", endOfLevelMessageString);
				int currentLevel = PlayerPrefs.GetInt("current level");
				PlayerPrefs.SetInt ("current level", currentLevel+1);
				EndOfLevel();
			}
			if (endless && timer <= 0) {
				timer = 30;
				timeBetweenOrders *= .95f;
				if (timeBetweenOrders <= .5f) {
					timeBetweenOrders = .5f;
				}
			}

		}
		else if (!waitingForPlayerInput) {
			//end of day pre-routine
			float distanceCovered = (Time.realtimeSinceStartup - startTime) * speed;
			float fracJourney = distanceCovered / movementTime;
			Camera.main.transform.position = Vector3.Lerp (startingPosition.position, endOfDayCameraPosition.position, fracJourney);			
				if (Vector3.Distance (endOfDayCameraPosition.transform.position, Camera.main.transform.position) <= .1) {
					waitingForPlayerInput = true;
				}
		}

	}

	public void EndOfLevel() {
		endOfLevelTitle.GetComponent<TextMesh>().text = PlayerPrefs.GetString("ENDOFLEVEL_TITLE");
		endOfLevelMessage.GetComponent<TextMesh>().text = PlayerPrefs.GetString("ENDOFLEVEL_MESSAGE");
		Camera.main.GetComponent<CameraShakeScript>().pauseSneezing();
		Time.timeScale = 0;
		startTime = Time.realtimeSinceStartup;
		movementTime = Vector3.Distance(startingPosition.position, endOfDayCameraPosition.position);
		endOfDay = true;

		if (hasTaintedFood()) {
			PlayerPrefs.SetString ("Game Over Message", "You Got Paid and Spread Your Disease!");
		}
		else {
			PlayerPrefs.SetString ("Game Over Message", "Good Job! You Get Paid!");
		}

	}
	
	private bool hasTaintedFood() {
		bool taintedFood = false;
		GameObject[] trays = GameObject.FindGameObjectsWithTag("Tray");
		for (int i = 0; i < trays.Length; i++) {
			if (trays[i].GetComponent<AddIngredientScript>().getTainted()) {
				return true;
			}
		}
		return false;
	}
}
