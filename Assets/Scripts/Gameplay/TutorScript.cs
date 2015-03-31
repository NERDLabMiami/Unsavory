using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TutorScript : MonoBehaviour {
	public List<GameObject> trays = new List<GameObject>();
	public GameObject recipe;
	public GameObject spawningArea;
	public GameObject orderHopper;
	public GameObject plate;
	public AudioClip tutorialFinished;
	public Button pauseButton;
	private int trayIndex = 0;
	private RecipeScript currentRecipe;
	public string tutorialEnd;
	public GameObject preStartObject;

	// Use this for initialization
	void Start () {
		Vector3 recipePosition = spawningArea.transform.position;
		recipePosition.x = 0;
		GameObject newRecipe =(GameObject) Instantiate(recipe, recipePosition, Quaternion.identity);
		newRecipe.transform.parent = spawningArea.transform;
		currentRecipe = recipe.GetComponent<RecipeScript>();
		GetComponentInChildren<Text>().text = trays[trayIndex].GetComponent<AddIngredientScript>().tutorialLesson;
		trays[trayIndex].GetComponent<Animator>().SetBool("jiggling", true);
		for (int i = 0; i < trays.Count; i++) {
			trays[i].GetComponent<BoxCollider2D>().enabled = false;
		}
		trays[trayIndex].GetComponent<BoxCollider2D>().enabled = true;
		plate.GetComponent<BoxCollider2D>().enabled = false;
		plate.GetComponent<PlateScript>().tutorialMode = true;
		Time.timeScale = 0;
		pauseButton.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void finishTutorial() {
		for (int i = 0; i < trays.Count; i++) {
			trays[i].GetComponent<BoxCollider2D>().enabled = true;
		}
		//TODO: Start Current Order Script Up
		//orderHopper.GetComponent<CurrentOrderScript>().spawnMoreOrders = true;
		Time.timeScale = 1.0f;
		pauseButton.enabled = true;
		preStartObject.SetActive(true);
		PlayerPrefs.SetInt("tutorial", 1);
		PlayerPrefs.SetInt ("sneeze tutor", 0);
		GetComponent<Animator>().SetTrigger("disappear");
		GetComponent<AudioSource>().clip = tutorialFinished;
		GetComponent<AudioSource>().Play();
	}

	public void advanceTutorial() {
		Debug.Log("Advance Tutorial");
		trays[trayIndex].GetComponent<Animator>().SetBool("jiggling", false);
		trayIndex++;
		if (trayIndex < trays.Count) {
			GetComponentInChildren<Text>().text = trays[trayIndex].GetComponent<AddIngredientScript>().tutorialLesson;
			trays[trayIndex].GetComponent<Animator>().SetBool("jiggling", true);
			trays[trayIndex].GetComponent<BoxCollider2D>().enabled = true;
		}
		else {
			Debug.Log("Done Advancing");
			GetComponentInChildren<Text>().text = tutorialEnd;
			plate.GetComponent<BoxCollider2D>().enabled = true;

		}


	}
}
