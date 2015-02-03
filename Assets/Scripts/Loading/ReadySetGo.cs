using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class ReadySetGo : MonoBehaviour {
	private float startTime;
	private float nextTime;
	private int wordIndex = 0;
	public Text display;
	public string[] words;
	public float secondsBetweenWords;
	public GameObject gamePlayObject;
	public GameObject pauseButton;
	public GameObject timer;
	public GameObject player;
	private bool finished = false;

	// Use this for initialization
	void Start () {
		nextTime = Time.realtimeSinceStartup + secondsBetweenWords;
		display.text = words[wordIndex];
	}


	// Update is called once per frame
	void Update () {
		if (!finished) {
			if (nextTime <= Time.realtimeSinceStartup) {
				nextTime = Time.realtimeSinceStartup + secondsBetweenWords;

				//move to the next word
				wordIndex++;
				if (wordIndex >= words.Length) {
					GetComponentInChildren<Animator>().SetBool("finished", true);
					gamePlayObject.SetActive(true);
					pauseButton.SetActive(true);
					timer.GetComponent<PauseScript>().gamePlayStarted = true;
					player.GetComponent<StartLevel>().beginLevel();
					timer.GetComponent<TimerScript>().running = true;

					finished = true;
				}
				else {
					display.text = words[wordIndex];
				}
			}
		}
	}
}
