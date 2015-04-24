using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour {
	private int sceneNumber;
	private static AsyncOperation ao = null;

	void Update() {
		if (ao != null) {
//			Debug.Log("Progress: " + ao.progress);
		}
	}
	private IEnumerator LoadALevel(int level) {
		ao = Application.LoadLevelAsync (level);
		Debug.Log("Loading level...");
		yield return ao;
	}

	public void goHome() {
		Application.LoadLevel(2);
	}
	public void mainMenu() {
		Application.LoadLevel(0);
	}


	public void newScene() {
		Application.LoadLevel(sceneNumber);
	}

	public void startCatering() {
		PlayerPrefs.SetInt("catering",1);
		int timesPlayed = PlayerPrefs.GetInt("times played", 0);
		timesPlayed++;
		PlayerPrefs.SetInt("times played", timesPlayed);
		sceneNumber = 1;
		ao = Application.LoadLevelAsync(sceneNumber);
	}

	public void restartLevel() {
		ao = Application.LoadLevelAsync(1);
	}

	public void RestartGame() {
		/*
		if (Social.localUser.authenticated) {
				Social.ReportProgress( Achievements.WIPED, 100f, (result) => {
					Debug.Log ( result ? "Reported Wiped" : "Failed to report achievement wiped");
				});
			}
*/
		Camera.main.gameObject.GetComponent<Animator>().SetTrigger("fade out");
		string resume = PlayerPrefs.GetString("resume game");
		PlayerPrefs.DeleteKey("catering");
		Debug.Log("Restart Game Run");
		if (resume.Equals("home")) {
			sceneNumber = 2;

		}

		else {
			sceneNumber = 1;
		}
		Debug.Log("Loading Level " + sceneNumber);
		ao = Application.LoadLevelAsync(sceneNumber);

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
