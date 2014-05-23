using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;


[System.Serializable]
public class SpeechPoint {
	public GameObject anchor;
	public TextAsset speech;
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
		string dialogText =  Regex.Replace(speechPoints[speechPointIndex].speech.text, @"\s{2,}"," ");
		while (offset < dialogText.Length) {
			
			int index = dialogText.LastIndexOf(" ", Math.Min (dialogText.Length, offset+maximumCharactersPerLine));
			string line = dialogText.Substring(offset, (index - offset <= 0 ? dialogText.Length : index) - offset);
			offset+=line.Length + 1;
			dialog.Add (line);
		}
		gameObject.GetComponentInChildren<TextMesh>().text = dialog[currentDialogIndex];
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
		startTalking = true;
		gameObject.SetActive(true);
	}

	public void move() {
		startingPoint =  Camera.main.transform;
		startTime = Time.realtimeSinceStartup;
		movementTime = Vector3.Distance(startingPoint.position, speechPoints[speechPointIndex].anchor.transform.position);
		moving = true;
	}

	private void objectFadeAway() {
		Debug.Log("Fading Away...");
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