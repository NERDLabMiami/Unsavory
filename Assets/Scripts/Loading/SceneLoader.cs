using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;


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
		Application.LoadLevel(3);
	}
	public void mainMenu() {
		Application.LoadLevel(1);
	}


	public void newScene() {
		Application.LoadLevel(sceneNumber);
	}

	public void startCatering() {
		PlayerPrefs.SetInt("catering",1);
		int timesPlayed = PlayerPrefs.GetInt("times played", 0);
		timesPlayed++;
		PlayerPrefs.SetInt("times played", timesPlayed);
		sceneNumber = 2;
		ao = Application.LoadLevelAsync(sceneNumber);
	}

	public void restartLevel() {
		ao = Application.LoadLevelAsync(2);
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
			sceneNumber = 3;

		}

		else {
			sceneNumber = 2;
		}
		Debug.Log("Loading Level " + sceneNumber);
		ao = Application.LoadLevelAsync(sceneNumber);

	}

	public void setWeek(int week) {
		int health = 30;
		int day = week * 5;
		if (week == 0) {
			health = 60;
			day = 1;
			CustomFunctionScript.resetPlayerData(health, 0);

		}
		else {
			CustomFunctionScript.resetPlayerData(health, 0);
			PlayerPrefs.SetInt("welcomed home", 1);
			PlayerPrefs.SetString("resume game", "home");
		}
		if (week == 1) {
			PlayerPrefs.SetInt("letter",1);
		}

		PlayerPrefs.SetInt("started career", 1);
		PlayerPrefs.SetInt("current level", day);
		PlayerPrefs.SetInt("max warnings", 3);
		PlayerPrefs.SetInt("final day", 20);
		PlayerPrefs.SetFloat ("paycheck", 0);
		PlayerPrefs.SetFloat("earned wages", 322);
		if (week > 1) {
			PlayerPrefs.SetFloat("bank account", week * 322);
			PlayerPrefs.SetFloat ("money", (week-1) * 322);

		}
		PlayerPrefs.SetFloat("hourly rate", 8.05f);
		PlayerPrefs.SetInt("retries", 3);

		//set effect levels
		
		PlayerPrefs.SetInt("health effect", 0);
		PlayerPrefs.SetInt("late effect", 0);
		PlayerPrefs.SetInt("overtime effect", 0);
		PlayerPrefs.SetInt("electricity effect", 0);
		
		//read bills JSON
		string billData =  Resources.Load<TextAsset>("bills").ToString();
		JSONNode bills = JSON.Parse(billData);
		
		for (int i = 0; i < bills["bills"].Count; i++) {
			PlayerPrefs.SetInt("bill" + i,0);
		}

		RestartGame();


	}


	public void QuitInGame() {
		PlayerPrefs.SetString("resume game", "game");
		setSceneNumber(1);
		newScene();

	}
	
	public void QuitAtHome() {
		PlayerPrefs.SetString("resume game", "home");
		setSceneNumber(1);
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
