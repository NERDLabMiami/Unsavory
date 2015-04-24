using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SocialPlatforms;

public class Catering : MonoBehaviour {
	public PlateScript plate;
	public GameObject cateringCompleteCanvas;
	public Image HUDIcon;
	public Text orderCounter;
	public Sprite countingIcon;
	public Animator gameScreen;
	public GameObject[] objectsToHide;
	public int ordersNeeded = 10;
	public CurrentOrderScript orders;
	private bool finished = false;
	public Text finishedDescription;
	public Text finishedTitle;
	private float timer = 45;

	// Use this for initialization
	void Start () {
		if (!PlayerPrefs.HasKey("catering")) {
			Destroy (this);
		}
		else {
			HUDIcon.sprite = countingIcon;
			//remove sneezing tutor
			PlayerPrefs.DeleteKey("sneeze tutor");
			PlayerPrefs.SetInt("tutorial", 1);

		}
		foreach(GameObject obj in objectsToHide) {
			obj.SetActive(false);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!finished) {
			//TODO: Count # of orders for achievement?
			/*
			if (plate.getOrders() >= ordersNeeded) {
				//gameScreen.SetBool("ended", true);
				cateringCompleteCanvas.SetActive(true);
				Debug.Log("FINISHED CATERING");
				Time.timeScale = 0f;
				finished = true;
			}
			*/
			orderCounter.text = plate.getOrders().ToString();

		}
		timer -= Time.deltaTime;
		if (timer <= 0) {
			timer = 10;
			orders.incrementRecipeList();
		}

	}

	public void sick() {
		if (Social.localUser.authenticated) {
			Social.ReportScore (plate.getOrders(), "challenge", success => {
				Debug.Log(success ? "Reported score successfully" : "Failed to report score");
			});
		}
		

		finishedTitle.text = "You Blew It!";
		finishedDescription.text = "You sneezed all over the food, that's disgusting.";
		Time.timeScale = 0f;
		cateringCompleteCanvas.SetActive(true);
	}

	public void tooSlow() {
		if (Social.localUser.authenticated) {
			Social.ReportScore (plate.getOrders(), "challenge", success => {
				Debug.Log(success ? "Reported score successfully" : "Failed to report score");
			});
		}
		finishedTitle.text = "Too Slow";
		finishedDescription.text = "You've got too many orders backed up.";
		Time.timeScale = 0f;
		cateringCompleteCanvas.SetActive(true);
		Debug.Log("Activated catering canvas");

	}
}
