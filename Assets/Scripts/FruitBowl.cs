using UnityEngine;
using System.Collections;

public class FruitBowl : MonoBehaviour {
	public GameObject[] fruit;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void setFruit(int dayOfWeek) {
		switch(dayOfWeek) {
		case 3:
			fruit[fruit.Length-3].SetActive(false);
			goto case 2;
		case 2:
			fruit[fruit.Length-2].SetActive(false);
			goto case 1;
		case 1:
			fruit[fruit.Length-1].SetActive(false);
			break;
		case 4:
			for (int i = 0; i < fruit.Length; i++) {
				fruit[i].SetActive(false);
			}
			break;
		case 5:
			break;
		}
	}

}
