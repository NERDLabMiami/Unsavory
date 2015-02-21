using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class WorkScript : MonoBehaviour {
	private float health;
	public float decreaseHealthMinimum = .1f;
	public float decreaseHealthMaximum = .2f;
	public float increaseHealthMinimum = 1;
	public float increaseHealthMaximum = 2;

	public void decreaseHealth() {
		health = PlayerPrefs.GetFloat("health");
		health -= Random.Range (decreaseHealthMaximum, decreaseHealthMinimum);
		PlayerPrefs.SetFloat("health",health);
	}

	public void increaseHealth() {
		health = PlayerPrefs.GetFloat("health");
		health += Random.Range (increaseHealthMaximum, increaseHealthMinimum);
		PlayerPrefs.SetFloat("health",health);
//		int currentLevel = PlayerPrefs.GetInt("current level");
		List<float> wages = PlayerPrefsX.GetFloatArray("wages").Cast<float>().ToList();
		wages.Add (0);
		Debug.Log("Made $0 for staying home");
		PlayerPrefsX.SetFloatArray("wages", wages.ToArray());
	}
}
