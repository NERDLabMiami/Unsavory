using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraSequencing : MonoBehaviour {
	public GameObject preStartObject;
	public GameObject gamePlayObject;
	public GameObject timer;
	public GameObject pauseButton;
	public GameObject player;
	public CharacterDialog boss;
	// Use this for initialization
	void Start () {
	
	}

	public void enableGamePlay() {
		if (PlayerPrefs.HasKey("tutorial")) {
			preStartObject.SetActive(true);
		}
		else {
			boss.enableTrays();
			gamePlayObject.SetActive(true);
			pauseButton.SetActive(true);
			timer.GetComponent<PauseScript>().gamePlayStarted = true;
			player.GetComponent<StartLevel>().beginLevel();
			timer.GetComponent<TimerScript>().running = true;
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
