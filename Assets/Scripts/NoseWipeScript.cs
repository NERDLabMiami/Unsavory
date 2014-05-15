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
		Swipe ();
	}

	
	private void resetSneezeTimer() {
		//TODO: re-enable this and make it for story mode only. also need to flip the switch once the game has ended due to sneezing
		//		if (canStopSneeze) {
			sneezeTimer = player.GetComponent<PlayerScript>().health;
//		}
	}

	public void Swipe()
		
	{
		
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
				
				Debug.Log("up swipe");
				
			}
			
			//swipe down
			
			if(currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f) {
				
				Debug.Log("down swipe");
				
			}
			
			//swipe left
			
			if(currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
				
				Debug.Log("left swipe");
				resetSneezeTimer();
				
			}
			
			//swipe right
			
			if(currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
				resetSneezeTimer();
				Debug.Log("right swipe");
				
			}
			
		}
		
	}



}
