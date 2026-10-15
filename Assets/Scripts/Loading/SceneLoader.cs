using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using UnityEngine.SceneManagement;


public class SceneLoader : MonoBehaviour {
	private int sceneNumber;
	private static AsyncOperation ao = null;
	

	public void goHome() {
		SceneManager.LoadScene("Home");
	}
	public void mainMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}

	public void startCatering() {
		PlayerPrefs.SetInt("catering",1);
		int timesPlayed = PlayerPrefs.GetInt("times played", 0);
		timesPlayed++;
		PlayerPrefs.SetInt("times played", timesPlayed);
		sceneNumber = 1;
		SceneManager.LoadScene("Game");
	}

	public void restartLevel() {
		ao = Application.LoadLevelAsync(1);
	}

	public void RestartGame() {
		string resume = PlayerPrefs.GetString("resume game");
		PlayerPrefs.DeleteKey("catering");
		Debug.Log("Restart Game Run");
		if (resume.Equals("home")) {
			SceneManager.LoadScene("Home");
		}
		else {
			SceneManager.LoadScene("Game");
		}

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
		PlayerPrefs.Save();
		SceneManager.LoadScene("MainMenu");
	}
	
	public void QuitAtHome() {
		PlayerPrefs.SetString("resume game", "home");
		PlayerPrefs.Save();
		SceneManager.LoadScene("MainMenu");

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
