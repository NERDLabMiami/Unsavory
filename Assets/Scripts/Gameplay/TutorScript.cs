using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class TutorScript : MonoBehaviour {
//	public List<GameObject> trays = new List<GameObject>();
//	public GameObject recipes[];
//	public List<GameObject> recipes = new List<GameObject>();
	public List<TutorialRecipe> recipes = new List<TutorialRecipe>();
	public GameObject spawningArea;
//	public GameObject orderHopper;
	public GameObject plate;
	public AudioClip tutorialFinished;
	public Button pauseButton;
	private int trayIndex = 0;
	private int selectedTutorial = 0;
//	private RecipeScript currentRecipe;
	public string tutorialEnd;
//	public GameObject preStartObject;

	public GameObject continueTutorialDialog;
	public GameObject allTrays;
	public TortillaFoundation tortilla;
//	private Vector3 recipePosition;

	// Use this for initialization
	void Start () {

		CustomFunctionScript.collisionOnObjects(allTrays, false);
		tortilla.tutoring = true;
//		Vector3 recipePosition = spawningArea.transform.position;
//		recipePosition.x = 0;
		plate.GetComponent<BoxCollider2D>().enabled = false;
		plate.GetComponent<PlateScript>().tutorialMode = true;
		Time.timeScale = 0;
		pauseButton.enabled = false;
		getTutorial();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void getTutorial() {
		selectedTutorial = Random.Range (0, recipes.Count);
		trayIndex = 0;
		GameObject newRecipe =(GameObject) Instantiate(recipes[selectedTutorial].recipe, gameObject.transform.position, Quaternion.identity);
		newRecipe.transform.parent = spawningArea.transform;
		GetComponentInChildren<Text>().text = recipes[selectedTutorial].trays[trayIndex].GetComponent<AddIngredientScript>().tutorialLesson;
		recipes[selectedTutorial].trays[trayIndex].GetComponent<Animator>().SetBool("jiggling", true);
		recipes[selectedTutorial].trays[trayIndex].GetComponent<BoxCollider2D>().enabled = true;

	}

	public void continueTutorial(bool matched) {
		if (matched) {
			continueTutorialDialog.GetComponent<TutorialComplete>().title.text = "Nice Job!";
			continueTutorialDialog.GetComponent<TutorialComplete>().message.text = "It looks like you've got the hang of this. We've got customers lining up for tacos. Are you ready?";

		}
		else {
			continueTutorialDialog.GetComponent<TutorialComplete>().title.text = "Yikes!";
			continueTutorialDialog.GetComponent<TutorialComplete>().message.text = "I'm not sure you're ready. We've got customers lining up for tacos. Are you ready?";

		}
		continueTutorialDialog.SetActive(true);

	}

	public void tutorialComplete() {
//		GetComponent<Animator>().SetTrigger("disappear");
				CustomFunctionScript.collisionOnObjects(allTrays, true);

	}
	public void confirmInstruction() {
		GetComponent<Animator>().SetTrigger("confirmed");
				if (trayIndex < recipes[selectedTutorial].trays.Count) {
					recipes[selectedTutorial].trays[trayIndex].GetComponent<BoxCollider2D>().enabled = true;
				}
				else {
						plate.GetComponent<BoxCollider2D>().enabled = true;
				}
		}

	public void advanceTutorial() {
		Debug.Log("Advance Tutorial");
		recipes[selectedTutorial].trays[trayIndex].GetComponent<Animator>().SetBool("jiggling", false);
				//disable all collision
				CustomFunctionScript.collisionOnObjects(allTrays, false);
		trayIndex++;
		if (trayIndex < recipes[selectedTutorial].trays.Count) {
			GetComponentInChildren<Text>().text = recipes[selectedTutorial].trays[trayIndex].GetComponent<AddIngredientScript>().tutorialLesson;
			recipes[selectedTutorial].trays[trayIndex].GetComponent<Animator>().SetBool("jiggling", true);
			recipes[selectedTutorial].trays[trayIndex].GetComponent<BoxCollider2D>().enabled = true;
		}
		else {
			Debug.Log("Done Advancing");

			GetComponentInChildren<Text>().text = tutorialEnd;

			plate.GetComponent<BoxCollider2D>().enabled = true;

		}
		GetComponent<Animator>().SetTrigger("next");


	}
}
