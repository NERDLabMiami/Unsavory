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
	public GameObject advanceButton;
	public GameObject homeButton;
	public GameObject retryButton;
	public GameObject quitButton;
	public GameObject trays;
	public GameObject inGameGui;
	public MusicLibrary music;
//	private bool endless = false;

	// Use this for initialization
	void Start () {
		/*
		if (PlayerPrefs.GetInt("endless") == 1) {
			endless = true;
			Debug.Log("ENDLESS MODE ON");
		}
		else {
			endless = false;
			Debug.Log("ENDLESS MODE OFF");
		}
		*/
//		homeButton.SetActive(false);
//		retryButton.SetActive(false);
//		quitButton.SetActive(false);
	}
	/*
	public void enableTrays() {
		trays.SetActive(true);
		Debug.Log("Prepping");
//		Camera.main.GetComponent<Animator>().SetTrigger("Prep");
	}
	*/
	public void reappear() {
		GetComponent<Animator>().SetTrigger ("reappear");
		Camera.main.GetComponent<Animator>().SetTrigger("Pots");
	}

	
	public void advanceScript() {
		if (dialogIndex >= jsonDialog[key][selectedIndex].Count - 1) {
			//HIDES BOSS
			GetComponentInChildren<Animator>().SetTrigger("disappear");
			//SHOWS GUI
			inGameGui.SetActive (true);
			Camera.main.GetComponent<Animator>().SetTrigger("Prep");

		}
		else {
			music.boss();
			dialogIndex++;
			string response_key = key + "_response";
			dialogBox.text = jsonDialog[key][selectedIndex][dialogIndex];
			Text responseText = advanceButton.GetComponentInChildren<Text>();
			responseText.text = jsonDialog[response_key][selectedIndex][dialogIndex];
		}
	}

	public void changeSpeechKey(string _key) {
		jsonDialog = JSON.Parse(dialog.ToString());
		key = _key;
		dialogIndex = 0;
		selectedIndex = Random.Range(0, jsonDialog[key].Count);
		dialogBox.text = jsonDialog[key][selectedIndex][dialogIndex];
		string response_key = _key + "_response";
		Text responseText = homeButton.GetComponentInChildren<Text>();
		Text advanceText = advanceButton.GetComponentInChildren<Text>();
		if (responseText) {
			responseText.text = jsonDialog[response_key][selectedIndex][dialogIndex];
		}
		if (advanceText) {
			advanceText.text = jsonDialog[response_key][selectedIndex][dialogIndex];
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
