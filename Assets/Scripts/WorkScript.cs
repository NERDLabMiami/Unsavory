using UnityEngine;
using System.Collections;

public class WorkScript : MonoBehaviour {
	private float health;
	public float decreaseHealthMinimum = 1;
	public float decreaseHealthMaximum = 2;

	void OnMouseDown() {
		health = PlayerPrefs.GetFloat("health");
		health -= Random.Range (decreaseHealthMaximum, decreaseHealthMinimum);
		PlayerPrefs.SetFloat("health",health);
	}
}
