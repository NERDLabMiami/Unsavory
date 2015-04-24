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
	public Level level;
	public Button pauseButton;
	public CurrentOrderScript currentOrder;
	public MusicLibrary music;
	private bool finished = false;
	// Use this for initialization
	void Start () {
		nextTime = Time.realtimeSinceStartup + secondsBetweenWords;
		display.text = words[wordIndex];
		Camera.main.GetComponent<AudioSource>().Stop ();
//		music.prestart();
		pauseButton.enabled = false;
	}


	public void startGame() {
	
//		gamePlayObject.SetActive(true);
//		orderHopper.GetComponent<CurrentOrderScript>().spawnMoreOrders = true;
		timer.GetComponent<PauseScript>().gamePlayStarted = true;
		level.beginLevel();
		timer.GetComponent<TimerScript>().running = true;
		//TODO: Switch between modes
		//		currentOrder.spawnMoreOrders = true;
//		currentOrder.Spawn();
		currentOrder.newOrder();
		Time.timeScale = 1f;
		GetComponent<AudioSource>().Stop();
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
					GetComponentInChildren<Animator>().SetTrigger("finished");
					Debug.Log("Starting Game");
					display.text = "Go!";
					finished = true;
					startGame();
				}
				else {
					display.text = words[wordIndex];
					GetComponentInChildren<Animator>().SetTrigger("wobble");
				}
			}
		}

	}
}
