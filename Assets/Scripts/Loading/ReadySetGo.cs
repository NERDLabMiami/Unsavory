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
	public GameObject timer;
	public GameObject player;
	public Button pauseButton;
	public CurrentOrderScript currentOrder;
	public MusicLibrary music;
	private bool finished = false;
	// Use this for initialization
	void Start () {
		nextTime = Time.realtimeSinceStartup + secondsBetweenWords;
		display.text = words[wordIndex];
		Camera.main.GetComponent<AudioSource>().Stop ();
		music.prestart();
		pauseButton.enabled = false;
	}


	public void startGame() {
	
//		gamePlayObject.SetActive(true);
//		orderHopper.GetComponent<CurrentOrderScript>().spawnMoreOrders = true;
		timer.GetComponent<PauseScript>().gamePlayStarted = true;
		player.GetComponent<StartLevel>().beginLevel();
		timer.GetComponent<TimerScript>().running = true;
		currentOrder.spawnMoreOrders = true;
		currentOrder.Spawn();
		Time.timeScale = 1f;
		music.countdownFinished();
		Debug.Log("Let's go");
		pauseButton.enabled = true;

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
					Debug.Log("Starting Game");
					finished = true;
					startGame();
				}
				else {
					display.text = words[wordIndex];
				}
			}
		}

	}
}
