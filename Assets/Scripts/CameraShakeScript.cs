using UnityEngine;
using System.Collections;

public class CameraShakeScript : MonoBehaviour {

	private float timeBetweenSneezes = 10;
	private float timeBeforeSneezeWarning = 2;
	private bool Sneezing;
	private bool SneezeWarning;
	private float sneezeTimer = 10;
	private float ShakeDecay = .02f;
	private float ShakeIntensity  = 0.3f;   
	private Vector3 OriginalPos;
	private Quaternion OriginalRot;
	public GameObject player;
	private GestureLibrary gl;
	private Vector2 firstPosition;
	private Vector2 lastPosition;
	private Vector2 firstPressPos;
	private Vector2 secondPressPos;
	private Vector2 currentSwipe;
	private bool isPaused = false;
	
	void Start()
	{
		gl = new GestureLibrary("gestures");
		resetSneezeTimer();
		Sneezing = false;
		SneezeWarning = false;
		OriginalPos = transform.position;
		OriginalRot = transform.rotation;
	}

	public void pauseSneezing() {
		isPaused = true;
	}

	public void resumeSneezing() {
		isPaused = false;
	}

	private void resetSneezeTimer() {
		sneezeTimer = player.GetComponent<PlayerScript>().health;
	}
	
	// Update is called once per frame
	void Update () 
	{
		sneezeTimer -= Time.deltaTime;
		if (sneezeTimer <= 0) {
			sneezeTimer = timeBetweenSneezes;
			SneezeWarning = false;
			DoShake(.02f, .3f);
			Sneezing = true;
			//TAINT INGREDIENTS
			GameObject[] trays = GameObject.FindGameObjectsWithTag("Tray");
			for (int i = 0; i < trays.Length; i++) {
				trays[i].GetComponent<AddIngredientScript>().setTainted(true);
			}


		}

		if (sneezeTimer <= timeBeforeSneezeWarning) {
			SneezeWarning = true;
			DoShake(timeBeforeSneezeWarning - .1f, .01f);
		}



		if(ShakeIntensity > 0 && (Sneezing || SneezeWarning) && !isPaused) {

			transform.position = OriginalPos + Random.insideUnitSphere * ShakeIntensity;
			transform.rotation = new Quaternion(OriginalRot.x + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f,
			                                    OriginalRot.y + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f,
			                                    OriginalRot.z + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f,
			                                    OriginalRot.w + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f);
			
			ShakeIntensity -= ShakeDecay;
		}
		else if (Sneezing) {
			Sneezing = false;  
		}
		else if (SneezeWarning) {
			SneezeWarning = false;
		}




		//swipe detection
		Swipe ();


	}
	
	

	public void DoShake(float decay, float intensity) {
		ShakeIntensity = intensity;
		ShakeDecay = decay; 
		Sneezing = true;
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
