using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour {
	private int sceneNumber;

	
	public void goHome() {
		Application.LoadLevel(2);
	}
	public void mainMenu() {
		Application.LoadLevel(0);
	}


	public void newScene() {
		Application.LoadLevel(sceneNumber);
	}

	public void RestartGame() {
		string resume = PlayerPrefs.GetString("resume game");
		Debug.Log("Restart Game Run");
		if (resume.Equals("home")) {
			sceneNumber = 2;

		}

		else if (resume.Equals("game")) {
			sceneNumber = 1;

		}
		else {
			sceneNumber = 1;
		}
		Application.LoadLevel(sceneNumber);

	}

	

	public void QuitInGame() {
		PlayerPrefs.SetString("resume game", "game");
		setSceneNumber(0);
		newScene();

	}

	public void QuitAtHome() {
		PlayerPrefs.SetString("resume game", "home");
		setSceneNumber(0);
		newScene();
	}

	public void setSceneNumber(int sceneNum) {
		sceneNumber = sceneNum;
	}

	public void setEndless(bool endless) {
		if (endless) {
			PlayerPrefs.SetInt("endless", 1);
		}
		else {
			PlayerPrefs.SetInt("endless", 0);
			
		}
	}
}
