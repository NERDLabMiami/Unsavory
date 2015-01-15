using UnityEngine;
using System.Collections;

public class ReturnHomeScript : MonoBehaviour {
	public GameObject desk;
	public GameObject check;
	public GameObject fired;
	private bool playerWasFired = false;
	// Use this for initialization
	void Start () {
		bool weekend = false;
		int day = PlayerPrefs.GetInt("current level",1);
		float[] wages = PlayerPrefsX.GetFloatArray("wages");
		if (PlayerPrefs.HasKey ("fired")) {
			playerWasFired = true;
			Debug.Log("Fired...");
			fired.GetComponent<PanPositionScript>().move ();
			CustomFunctionScript.resetPlayerData(100,0);
		}
		else  {
			Debug.Log("It's day " + day);
			if (day % 5 == 0) {
				weekend = true;
				Debug.Log("It's the weekend");
				string daysWorked = "";
				if (wages.Length >= day) {
	//				for (int i = 0; i < 5; i++) {
					for (int i = day; i > day - 5; i--) {
						Debug.Log("Day: " + i);
						daysWorked += "$" + wages[i-1].ToString() + "\n";
						Debug.Log ("Pay for " + wages[i-1]);
					}
					GetComponent<TextMesh>().text = daysWorked;
				}
				else {
					Debug.Log("There are less wages than there are days...");
				}
				check.GetComponent<PanPositionScript>().move ();
			}
			else {
				desk.GetComponent<PanPositionScript>().move ();

				Debug.Log("It's not the weekend");
			}
			//advance the day now that you're home.
			PlayerPrefs.SetInt ("current level", day+1);
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
