using UnityEngine;
using System.Collections;

public class SwipeScene : MonoBehaviour {
	private Vector2 firstPosition;
	private Vector2 lastPosition;
	private Vector2 firstPressPos;
	private Vector2 secondPressPos;
	private Vector2 currentSwipe;
	public Transform[] places;
	public int currentPlace;
	private Transform startPosition;
	private Transform endPosition;
	private float currentLerpTime = 0f;
	private float lerpTime = 1f;
	private bool moving = false;
	private float movementStartTime;


	// Use this for initialization
	void Start () {
	}

	void Update() {
		if (moving) {
			currentLerpTime += Time.deltaTime;
			if (currentLerpTime > lerpTime) {
				currentLerpTime = lerpTime;
				moving = false;

			}
			
			float perc = currentLerpTime / lerpTime;
			Camera.main.transform.position = Vector3.Lerp (startPosition.position, endPosition.position,perc);
		}

		Swipe();
	}

	public void Swipe()
		
	{
		
		if(Input.GetMouseButtonDown(0)) {
			firstPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
		}
		
		if(Input.GetMouseButtonUp(0)) {
			secondPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
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
				if (currentPlace < places.Length -1 && !moving) {
					startPosition = places[currentPlace];
					currentPlace++;
					endPosition = places[currentPlace];
					currentLerpTime = 0f;
					moving = true;
				}
			}
			
			//swipe right
			
			if(currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f) {
				if (currentPlace > 0 && !moving) {
					startPosition = places[currentPlace];
					currentPlace--;
					endPosition = places[currentPlace];
					Debug.Log("right swipe");				
					moving = true;
					currentLerpTime = 0f;
				}
			}
			
		}
	}
}