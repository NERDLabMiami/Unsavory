using UnityEngine;
using System.Collections;

public class NoseWipeScript : MonoBehaviour {
	private float timeBetweenSneezes = 10;
	private float timeBeforeSneezeWarning = 2;
//	private bool Sneezing;
//	private bool SneezeWarning;
	private float sneezeTimer = 10;
//	private float ShakeDecay = .02f;
//	private float ShakeIntensity  = 0.3f;   
//	private Vector3 OriginalPos;
//	private Quaternion OriginalRot;
	public GameObject player;
	private bool canStopSneeze = false;
	private Vector2 firstPosition;
	private Vector2 lastPosition;
	private Vector2 firstPressPos;
	private Vector2 secondPressPos;
	private Vector2 currentSwipe;
	private bool swipeWasActive;
	private Vector2 swipeStart;
	private Vector2 swipeEnd;
	public	float swipeThreshold = 1.2f;


//	private bool isPaused = false;
	// Use this for initialization

	void Start () {
		resetSneezeTimer();
		if (PlayerPrefs.HasKey("sneezed")) {
			canStopSneeze = true;
		}
		else {
			canStopSneeze = false; 
		}
	}
	
	// Update is called once per frame
	void Update () {
		sneezeTimer -= Time.deltaTime;
		if (sneezeTimer <= 0) {
			Debug.Log("Sneezing still");
			Camera.main.GetComponent<CameraShakeScript>().sneeze(timeBetweenSneezes);
			sneezeTimer = timeBetweenSneezes;

			//TAINT INGREDIENTS
			GameObject[] trays = GameObject.FindGameObjectsWithTag("Tray");
			for (int i = 0; i < trays.Length; i++) {
				trays[i].GetComponent<AddIngredientScript>().setTainted(true);
			}
			
		}
		
		if (sneezeTimer <= timeBeforeSneezeWarning) {
			Camera.main.GetComponent<CameraShakeScript>().giveSneezeWarning(timeBeforeSneezeWarning);
		}

		//swipe detection
		if (Input.touchCount == 1) {
			Swipe ();
		}
	}

	
	private void resetSneezeTimer() {
		Debug.Log("Reset Sneeze");
		if (canStopSneeze) {
			sneezeTimer = player.GetComponent<PlayerScript>().health;
			Debug.Log("Sneeze Timer now : " + sneezeTimer);
		}
	}

	public void Swipe()
		
	{
		if (Application.platform == RuntimePlatform.IPhonePlayer) {
			Touch theTouch = Input.touches[0];
			if (theTouch.deltaPosition == Vector2.zero) {
			//nothing
				Debug.Log("Not a swipe...");
			}

			Vector2 speedVec = theTouch.deltaPosition * theTouch.deltaTime;
			float theSpeed = speedVec.magnitude;


			bool swipeActive = false;
			if (theSpeed > swipeThreshold) {
				swipeActive = true;
				Debug.Log("Swipe Active! " + theSpeed);
			}

			if (swipeActive) {
				if (!swipeWasActive) {
					swipeStart = theTouch.position;
				}
			}
			else {
				if (swipeWasActive) {
					swipeEnd = theTouch.position;
					Debug.Log("Swipe Detected: " + theSpeed);
					resetSneezeTimer();
				}
			}
			swipeWasActive = swipeActive;
		}

		else {
			if(Input.GetMouseButtonDown(0)) {
				
				//save began touch 2d point
				
				firstPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
				
			}
			
			if(Input.GetMouseButtonUp(0)) {
				
				//save ended touch 2d point
				
				secondPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
				
				
				
				//create vector from the two points
				
				currentSwipe = new Vector2(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y); 
				
				
				
				//normalize the 2d vector
				
				currentSwipe.Normalize();
				
				
				
				//swipe upwards
				
				if(currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f) {
					
					Debug.Log("up swipe ");
					
				}
				
				//swipe down
				
				if(currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f) {
					
					Debug.Log("down swipe");
					
				}
				
				//swipe left
				
				if(currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
					
					Debug.Log("left swipe: y, " + currentSwipe.y);
					resetSneezeTimer();
					
				}
				
				//swipe right
				
				if(currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
					resetSneezeTimer();
					Debug.Log("right swipe: y, " + currentSwipe.y);
					
				}
				
			}
		}		
	}



}
