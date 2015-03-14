using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Catering : MonoBehaviour {
	public PlateScript plate;
	public Canvas cateringCompleteCanvas;
	public Animator gameScreen;
	public int ordersNeeded = 10;
	private bool finished = false;

	// Use this for initialization
	void Start () {
		if (!PlayerPrefs.HasKey("catering")) {
			Destroy (this);
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (!finished) {
			if (plate.getOrders() >= ordersNeeded) {
				gameScreen.SetBool("ended", true);
				Time.timeScale = 0f;
				cateringCompleteCanvas.enabled = true;
				finished = true;
			}
		}
	}
}
