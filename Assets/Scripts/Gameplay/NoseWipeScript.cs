using UnityEngine;
using System.Collections;
using UnityEngine.SocialPlatforms.GameCenter;
using UnityEngine.SocialPlatforms;

public class NoseWipeScript : MonoBehaviour {
	private float timeBetweenSneezes = 10;
	private float timeBeforeSneezeWarning = 1.5f;
	public float sweatingBeforeSneezeWarning = 1f;
//	private bool Sneezing;
//	private bool SneezeWarning;
	private float sneezeTimer;
//	private float ShakeDecay = .02f;
//	private float ShakeIntensity  = 0.3f;   
//	private Vector3 OriginalPos;
//	private Quaternion OriginalRot;
	public GameObject player;
	public GameObject sneezeTutor;
	private bool canStopSneeze = false;
	public bool sneezeAllowed = true;
	private bool snotEmitting = false;
	public ParticleSystem snot;
	public GameObject snotImage;
	public ParticleSystem sweat;
//	public MusicLibrary music;
	private bool sweating = false;
	private Vector2 firstPosition;
	private Vector2 lastPosition;
	private Vector2 firstPressPos;
	private Vector2 secondPressPos;
	private Vector2 currentSwipe;
	public float avrgTime = 0.5f;
	public float peakLevel = 0.6f;
	public float endCountTime = 0.6f;
	public int shakeDir;
	public int shakeCount;
	Vector3 avrgAcc = Vector3.zero;
	int countPos;
	int countNeg;
	int lastPeak;
	int firstPeak;
	bool counting;
	float timer;

	float health;
	public float startingHealth;

	void Start () {
		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
		float healthEffect = PlayerPrefs.GetFloat("health effect", 0);
		health = PlayerPrefs.GetFloat("health");
		health = health - healthEffect;
		startingHealth = health;
		Debug.Log("Starting Health: " + health);
		initializeSneezing();
		sweatingBeforeSneezeWarning+= timeBeforeSneezeWarning;
		canStopSneeze = true;
		/*
		if (PlayerPrefs.HasKey("sneezed")) {
			canStopSneeze = true;
			Debug.Log("Player Should Be Able to Stop Sneeze");
		}
		else {
			Debug.Log("Player Should Not Be Able to Stop Sneeze");

			canStopSneeze = false; 
		}
		*/
	}

	public void initializeSneezing() {
		sneezeTimer = health;
		resetSneezeTimer();
	}

	// Update is called once per frame
	void Update () {
		if (snotEmitting) {
			if(snot.isStopped) {
				Camera.main.GetComponent<CameraShakeScript>().resetCameraToOriginalPosition();
				Time.timeScale = 0;
				PlayerPrefs.SetInt("activated",1);
				if (Social.localUser.authenticated) {
					Social.ReportProgress( Achievements.WIPED, 100.0f, (result) => {
						Debug.Log ( result ? "Reported Boogeyman" : "Failed to report boogeyman");
					});
				}

				//TODO: Check if catering or not
//				player.GetComponent<PlayerScript>().levelCompleteCanvas.SetActive(true);

			}
		}
		if (sneezeAllowed) {
			sneezeTimer -= Time.deltaTime;
			if (sneezeTimer <= 0) {
				Debug.Log("Sneezing still");
				Camera.main.GetComponent<CameraShakeScript>().sneeze(timeBetweenSneezes);
				sneezeTimer = timeBetweenSneezes;
				snot.Play();
				snotImage.SetActive(true);
//				music.sneeze();
				snotEmitting = true;
				//TAINT INGREDIENTS
				GameObject[] trays = GameObject.FindGameObjectsWithTag("Tray");
				for (int i = 0; i < trays.Length; i++) {
					trays[i].GetComponent<AddIngredientScript>().setTainted(true);
				}
				
			}
			if (sneezeTimer <= sweatingBeforeSneezeWarning && !sweating) {
				//TODO: sweat before sneezing

				sweat.Play();
				sweat.loop = true;
				sweating = true;
				Debug.Log("Sweating!");
				GetComponent<Animator>().SetTrigger("twitch");

			}
			if (sneezeTimer <= timeBeforeSneezeWarning) {
				Camera.main.GetComponent<CameraShakeScript>().giveSneezeWarning(timeBeforeSneezeWarning);
			}

			//swipe detection
			Swipe ();
		}
	}

	public void checkSneezeTutor() {
		if (PlayerPrefs.HasKey("sneeze tutor")) {
			//TODO: Launch Sneeze Tutor
			Time.timeScale = 0f;
			sneezeTutor.SetActive(true);
			PlayerPrefs.DeleteKey("sneeze tutor");
		}

	}

	public void endSneezeTutorial() {
		Time.timeScale = 1f;
	}
	private void resetSneezeTimer() {
		Debug.Log("Reset Sneeze");
//		GetComponent<Animator>().SetTrigger("relief");
		sweating = false;
		sweat.loop = false;
		if (canStopSneeze) {
			health = startingHealth;
			sneezeTimer = health;
			Debug.Log("Sneeze Timer now : " + sneezeTimer);
		}
		if (Social.localUser.authenticated) {
			Social.ReportProgress( Achievements.SNEEZED, 100.0f, (result) => {
				Debug.Log ( result ? "Reported Nose Wipe" : "Failed to report nose wipe");
			});
		}
	}
	
	public void Swipe()
		
	{
		float distance = 0f;
		if(Input.GetMouseButtonDown(0)) {
			
			//save began touch 2d point
			firstPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
			
		}
		
		if(Input.GetMouseButtonUp(0)) {
			

			secondPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
			
			
			
			//create vector from the two points
			
			currentSwipe = new Vector2(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y); 
			
			
			distance = Mathf.Abs(currentSwipe.x);
			//normalize the 2d vector

			currentSwipe.Normalize();
			
			
			
			//swipe upwards
			
			if(currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f) {
				
				Debug.Log("up swipe");
				
			}
			
			//swipe down
			
			if(currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f) {
				
				Debug.Log("down swipe");
				
			}
			
			//swipe left
			
			if(currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f && distance >= 80) {
				GetComponent<Animator>().SetTrigger("wipe right");
				GetComponent<AudioSource>().Play();
				resetSneezeTimer();
				
			}
			
			//swipe right
			
			if(currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f && distance >= 80) {
				GetComponent<Animator>().SetTrigger("wipe left");
				GetComponent<AudioSource>().Play();
				resetSneezeTimer();

			}
			
		}

		if (	Input.acceleration.magnitude < .5) {
//			resetSneezeTimer();
		}

		
	}


	bool ShakeDetector(){
		// read acceleration:
		Vector3 curAcc = Input.acceleration; 
		// update average value:
		avrgAcc = Vector3.Lerp(avrgAcc, curAcc, avrgTime * Time.deltaTime);
		// calculate peak size:
		curAcc -= avrgAcc;
		// variable peak is zero when no peak detected...
		int peak = 0;
		// or +/- 1 according to the peak polarity:
		if (curAcc.y > peakLevel) peak = 1;
		if (curAcc.y < -peakLevel) peak = -1;
		// do nothing if peak is the same of previous frame:
		if (peak == lastPeak) 
			return false;
		// peak changed state: process it
		lastPeak = peak; // update lastPeak
		if (peak != 0){ // if a peak was detected...
			timer = 0; // clear end count timer...
			if (peak > 0) // and increment corresponding count
				countPos++;
			else
				countNeg++;
			if (!counting){ // if it's the first peak...
				counting = true; // start shake counting
				firstPeak = peak; // save the first peak direction
			}
		} 
		else // but if no peak detected...
		if (counting){ // and it was counting...
			timer += Time.deltaTime; // increment timer
			if (timer > endCountTime){ // if endCountTime reached...
				counting = false; // finish counting...
				shakeDir = firstPeak; // inform direction of first shake...
				if (countPos > countNeg) // and return the higher count
					shakeCount = countPos;
				else
					shakeCount = countNeg;
				// zero counters and become ready for next shake count
				countPos = 0;
				countNeg = 0;
				return true; // count finished
			}
		}
		return false;
	}

}
