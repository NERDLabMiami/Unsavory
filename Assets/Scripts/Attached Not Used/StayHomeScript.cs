using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StayHomeScript : MonoBehaviour {
	private float health;
	public float increaseHealthMinimum = 1;
	public float increaseHealthMaximum = 2;
	void OnMouseDown() {
		health = PlayerPrefs.GetFloat("health");
		health += Random.Range (increaseHealthMaximum, increaseHealthMinimum);
		PlayerPrefs.SetFloat("health",health);
		int currentLevel = PlayerPrefs.GetInt("current level");
		PlayerPrefs.SetInt("current level", currentLevel+1);
		List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
		wages.Add (0);
		Debug.Log("Made $0 for staying home");
		PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
	}
}
