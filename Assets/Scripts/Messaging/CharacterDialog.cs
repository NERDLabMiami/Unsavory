using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using SimpleJSON;


public class CharacterDialog : MonoBehaviour {
	public TextAsset dialog;
	public string key;
	public Text dialogBox;
	private int dialogIndex = 0;
	private int selectedIndex;
	private JSONNode jsonDialog;
	public Button advanceButton;

	// Use this for initialization
	void Start () {
		jsonDialog = JSON.Parse(dialog.ToString());
		selectedIndex = Random.Range(0, jsonDialog[key].Count);
		Debug.Log("There are " + jsonDialog[key].Count + " potential messages.");
		dialogBox.text = jsonDialog[key][selectedIndex][dialogIndex];

	}
	
	public void advanceScript() {
		if (dialogIndex >= jsonDialog[key][selectedIndex].Count - 1) {
			//finished script
			GetComponentInChildren<Animator>().SetBool("finishedTalking", true);
			advanceButton.GetComponent<Animator>().SetBool("finished", true);
			Camera.main.GetComponent<Animator>().SetBool("playing", true);
			Camera.main.GetComponent<CameraSequencing>().enableGamePlay();
			//timer.GetComponent<TimerScript>().running = true;

		}
		else {
			dialogIndex++;
			dialogBox.text = jsonDialog[key][selectedIndex][dialogIndex];

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
