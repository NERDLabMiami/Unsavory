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
	private Vector3 OriginalPos;
	private Quaternion OriginalRot;
	public GameObject player;
	private bool isPaused = false;
	
	void Start()
	{
		Sneezing = false;
		SneezeWarning = false;
		OriginalPos = transform.position;
		OriginalRot = transform.rotation;
	}

	public void giveSneezeWarning(float timeBeforeWarning) {
		SneezeWarning = true;
		DoShake(timeBeforeWarning - .1f, .01f);

	}

	public void sneeze(float nextSneezeTime) {
		sneezeTimer = nextSneezeTime;
		SneezeWarning = false;
		DoShake(.02f, .3f);
		Sneezing = true;

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

			transform.position = OriginalPos + Random.insideUnitSphere * ShakeIntensity;
			transform.rotation = new Quaternion(OriginalRot.x + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f,
			                                    OriginalRot.y + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f,
			                                    OriginalRot.z + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f,
			                                    OriginalRot.w + Random.Range(-ShakeIntensity, ShakeIntensity)*.2f);
			
			ShakeIntensity -= ShakeDecay;
		}
		else if (Sneezing) {
			//TODO: Send message to pan and game over to the right
			didSneeze = true;
			Sneezing = false;  
		}
		else if (SneezeWarning) {
			SneezeWarning = false;
		}

	}

	public bool sneezed() {
		return didSneeze;
	}

	public void DoShake(float decay, float intensity) {
		ShakeIntensity = intensity;
		ShakeDecay = decay; 
		Sneezing = true;
	}   

}
