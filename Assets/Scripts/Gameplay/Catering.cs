using UnityEngine;
using UnityEngine.UI;

public class Catering : MonoBehaviour {
	public PlateScript plate;
	public Image HUDIcon;
	public Text orderCounter;
	public Sprite countingIcon;
	public GameObject[] objectsToHide;
	public int ordersNeeded = 10;
	public CurrentOrderScript orders;
	private Level level;
	private bool finished = false;
	private float timer = 45;

	// Use this for initialization
	void Start () {
		if (!PlayerPrefs.HasKey("catering")) {
			Destroy (this);
		}
		else {
			level = GetComponent<Level>();
			HUDIcon.sprite = countingIcon;
			//remove sneezing tutor
			PlayerPrefs.DeleteKey("sneeze tutor");
			PlayerPrefs.SetInt("tutorial", 1);

			foreach(GameObject obj in objectsToHide) {
				obj.SetActive(false);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (finished) return;

		int served = plate.getOrders();
		orderCounter.text = served.ToString();
		if (served >= ordersNeeded) {
			finished = true;
			level.EndRound(RoundOutcome.Complete);
			return;
		}

		timer -= Time.deltaTime;
		if (timer <= 0) {
			timer = 10;
			orders.incrementRecipeList();
		}
	}
}
