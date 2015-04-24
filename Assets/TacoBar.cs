using UnityEngine;
using System.Collections;

public class TacoBar : MonoBehaviour {
	public Animator[] tacos;
	public Level currentLevel;

	// Use this for initialization
	void Start () {
		for(int i = 0; i < currentLevel.retries; i++) {
			Debug.Log("Enabling Taco " + i);
			tacos[i].SetTrigger("enabled");
		}
		for (int i = tacos.Length - 1; i >= currentLevel.retries; i--) {
			Debug.Log("Disabling Taco " + i);
			tacos[i].SetTrigger("disabled");
		}
	}

	public void addTacos(int startingTaco, int endingTaco) {
		for (int i = startingTaco; i < endingTaco; i++) {
			tacos[i].SetTrigger("add");
		}
	}

	public void useTaco(int tacoId) {
		tacos[tacoId].SetTrigger("use");
	}

	// Update is called once per frame
	void Update () {
	
	}
}
