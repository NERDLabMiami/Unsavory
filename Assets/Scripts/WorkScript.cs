using UnityEngine;
using System.Collections;

public class WorkScript : MonoBehaviour {
	private float health;
	public float decreaseHealthMinimum = 10f;
	public float decreaseHealthMaximum = 20f;

	void OnMouseDown() {
		health = PlayerPrefs.GetFloat("health");
		health -= Random.Range (decreaseHealthMaximum, decreaseHealthMinimum);
		PlayerPrefs.SetFloat("health",health);
	}
}
