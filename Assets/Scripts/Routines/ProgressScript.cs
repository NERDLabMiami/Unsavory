using UnityEngine;
using UnityEngine.UI;

public class ProgressScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		int finalDay = PlayerPrefs.GetInt("final day", 20);
		int daysLeft = Mathf.Max(0, finalDay - PlayerPrefs.GetInt("current level", 0) + 1);
		{
			float money = PlayerPrefs.GetFloat("money");
			if (PlayerPrefs.HasKey("fired")) {
				GetComponent<Text>().text = "Fired with " + money.ToString ("$0") + " to your name";
			}
			else {
				GetComponent<Text>().text = money.ToString ("$0") + " and " + daysLeft.ToString("0 days left");

			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
