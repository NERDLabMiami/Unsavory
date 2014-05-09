using UnityEngine;
using System.Collections;

public class GameOverScript : MonoBehaviour {

	void OnGUI() {
		string message = PlayerPrefs.GetString("Game Over Message");
		GUI.Label(new Rect(Screen.width/2 - 40, 50, 80, 80), message);
		if (GUI.Button(new Rect(Screen.width/2 - 30, 350, 60, 30), "Retry?")) {
			Application.LoadLevel(0);
		}
	}
}
