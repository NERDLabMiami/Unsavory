using UnityEngine;
using System.Collections;

public class StayHomeScript : MonoBehaviour {
	private float health;
	public float increaseHealthMinimum = 1;
	public float increaseHealthMaximum = 2;
	void OnMouseDown() {
		health = PlayerPrefs.GetFloat("health");
		health += Random.Range (increaseHealthMaximum, increaseHealthMinimum);
		PlayerPrefs.SetFloat("health",health);
		int currentLevel = PlayerPrefs.GetInt("current level");
		currentLevel++;
		PlayerPrefs.SetInt("current level", currentLevel);
	}
}
