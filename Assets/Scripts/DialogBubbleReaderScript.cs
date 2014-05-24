using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


[System.Serializable]
public class SpeechPoint {
	public GameObject anchor;
	public TextAsset[] speech;
	public bool randomize = true;
	public string keyName;
	public int index;
}

public class DialogBubbleReaderScript : MonoBehaviour {
	private List<string> dialog = new List<string>();
	private int currentDialogIndex = 0;
	private int currentSpeechIndex = 0;
	private int currentAnchorIndex = 0;
	private int speechPointIndex = 0;
	private bool moving = false;
	private float startTime;
	private Transform startingPoint;
	private Vector3 objectOffset;
	private float movementTime;
	private float speed = 1.0f;
	private int assignedSpeech = 0;
	public bool pauseGame = false;
	public bool finishedTalking = false;
	public bool startTalking = false;
	public int maximumCharactersPerLine = 30;
	public SpeechPoint[] speechPoints;
	public GameObject nextObject;
	public GameObject panEventAfterDialog;

	// Use this for initialization

	void Start () {
		if (pauseGame) {
			Time.timeScale = 0f;
		}

		loadSpeech();
		objectOffset = Camera.main.transform.position - gameObject.transform.position;
	}

	void loadSpeech() {
		int offset = 0;
		currentDialogIndex = 0;
		dialog.Clear();
		if (speechPoints[speechPointIndex].randomize) {
			assignedSpeech = UnityEngine.Random.Range (0, speechPoints[speechPointIndex].speech.Length);
		}
		else {
			if(PlayerPrefs.HasKey(speechPoints[speechPointIndex].keyName)) {
			//has one, let's use it
				Debug.Log("Has the key, must have already run");
				assignedSpeech = PlayerPrefs.GetInt(speechPoints[speechPointIndex].keyName, speechPoints[speechPointIndex].index);
				if(PlayerPrefs.GetInt(speechPoints[speechPointIndex].keyName) >= speechPoints[speechPointIndex].speech.Length) {
					//went too far, need to reset the counter
					assignedSpeech = 0;
				}

			}
			else {
				Debug.Log("No key, let's generate one");
				PlayerPrefs.SetInt (speechPoints[speechPointIndex].keyName, 0);
			}
			//increment the speech index
			PlayerPrefs.SetInt(speechPoints[speechPointIndex].keyName,assignedSpeech +1); 
			Debug.Log("SPEECH INDEX: " + PlayerPrefs.GetInt(speechPoints[speechPointIndex].keyName));
		}
		Debug.Log("Assigned Speech: " + assignedSpeech);
		string dialogText =  Regex.Replace(speechPoints[speechPointIndex].speech[assignedSpeech].text, @"\s{2,}"," ");
		while (offset < dialogText.Length) {
			
			int index = dialogText.LastIndexOf(" ", Math.Min (dialogText.Length, offset+maximumCharactersPerLine));
			string line = dialogText.Substring(offset, (index - offset <= 0 ? dialogText.Length : index) - offset);
			offset+=line.Length + 1;
			dialog.Add (line);
		}
		gameObject.GetComponentInChildren<TextMesh>().text = dialog[currentDialogIndex];
		if (!speechPoints[speechPointIndex].randomize) {
		}
	}

	// Update is called once per frame
	void Update () {
		if (startTalking) {
			if (moving) {
				float distanceCovered = (Time.realtimeSinceStartup - startTime) * speed;
				float fracJourney = distanceCovered / movementTime;
				
				Camera.main.transform.position = Vector3.Lerp (startingPoint.position, speechPoints[speechPointIndex].anchor.transform.position, fracJourney);
				gameObject.transform.position = Camera.main.transform.position - objectOffset;

				if (Vector3.Distance (speechPoints[speechPointIndex].anchor.transform.position, Camera.main.transform.position) <= .1) {
					moving = false;
					Camera.main.transform.position = speechPoints[speechPointIndex].anchor.transform.position;
					speechPointIndex++;
				}
			}
			if (finishedTalking) {
				Debug.Log("Destroying Object");
				if (nextObject) {
					nextObject.GetComponent<DialogBubbleReaderScript>().beginTalking();
				}
				Destroy(gameObject);
			}
		}
	}

	public void beginTalking() {
		Debug.Log("I should start talking");
		startTalking = true;
		gameObject.SetActive(true);
		GetComponent<Animator>().SetTrigger("Fade In");

	}

	public void move() {
		startingPoint =  Camera.main.transform;
		startTime = Time.realtimeSinceStartup;
		movementTime = Vector3.Distance(startingPoint.position, speechPoints[speechPointIndex].anchor.transform.position);
		moving = true;
	}

	private void objectFadeAway() {
		moving = false;
		Time.timeScale = 1.0f;
		GetComponent<Animator>().SetTrigger("Fade Out");

	}

	void OnMouseDown() {
		currentDialogIndex++;
		if (currentDialogIndex < dialog.Count) {
			gameObject.GetComponentInChildren<TextMesh>().text = dialog[currentDialogIndex];
		}
		else {
			speechPointIndex++;
			if (speechPointIndex < speechPoints.Length) {
				loadSpeech();
				move ();
			}
			else {
				//resume game
				finishedTalking = true;
				if (panEventAfterDialog != null) {
					panEventAfterDialog.GetComponent<PanPositionScript>().move ();
				}
				else {
					objectFadeAway ();
				}
			}
		}
	}
}