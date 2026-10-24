using UnityEngine;
using System.Collections;

public class CameraShakeScript : MonoBehaviour {

//	private float timeBetweenSneezes = 10;
//	private float timeBeforeSneezeWarning = 2;
	private bool Sneezing;
	private bool SneezeWarning;
	private bool didSneeze = false;
	private float sneezeTimer = 10;
	private float ShakeDecay = .02f;
	private float ShakeIntensity  = 0.3f;   
	private Quaternion OriginalRot;
	public GameObject player;
	private bool isPaused = false;

	void Start()
	{
		Sneezing = false;
		SneezeWarning = false;
		// The camera's own rest rotation. (This used to copy the Player's pose, which put the camera at the origin after a sneeze.)
		OriginalRot = transform.rotation;
	}

	public void resetCameraToOriginalPosition() {
		// Only the shake is undone; the camera's position belongs to CameraDirector.
		transform.rotation = OriginalRot;
	}
	public void giveSneezeWarning(float timeBeforeWarning) {
		SneezeWarning = true;
		DoShake(timeBeforeWarning - .1f, .01f);
		Sneezing = false;

	}

	public void sneeze(float nextSneezeTime) {
		Debug.Log("Sneezed");
		sneezeTimer = nextSneezeTime;
		Sneezing = true;
		SneezeWarning = false;
		DoShake(.02f, .3f);

	}

	/// <summary>The day is over: forget any sneeze or warning in progress and put the camera back.</summary>
	public void ResetSneeze() {
		Sneezing = false;
		SneezeWarning = false;
		didSneeze = false;
		ShakeIntensity = 0f;
		resetCameraToOriginalPosition();
	}

	public void pauseSneezing() {
		isPaused = true;
	}

	public void resumeSneezing() {
		isPaused = false;
	}
	

	// Update is called once per frame
	void Update () 
	{
		sneezeTimer -= Time.deltaTime;
		if(ShakeIntensity > 0 && (Sneezing || SneezeWarning) && !isPaused) {		
			transform.rotation = new Quaternion(OriginalRot.x + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f,
			                                    OriginalRot.y + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f,
			                                    OriginalRot.z,
			                                    OriginalRot.w + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f);
			
			ShakeIntensity -= ShakeDecay;
		}
		else if (Sneezing) {
			//TODO: Send message to pan and game over to the right
			didSneeze = true;
			//Sneezing = false;  
		}
		else if (SneezeWarning) {
			SneezeWarning = false;
		}

	}

	public bool sneezed() {
		if (didSneeze) {
			Debug.Log("I done did sneeze");
		}
		return didSneeze;
	}

	public void DoShake(float decay, float intensity) {
		ShakeIntensity = intensity;
		ShakeDecay = decay;
	}   

}
