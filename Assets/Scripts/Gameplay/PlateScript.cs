using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SocialPlatforms.GameCenter;

public class PlateScript : MonoBehaviour {
//	public GameObject[] droppedIngredients;
	public List<GameObject> ingredients = new List<GameObject>();
	public GameObject currentOrder;
	public GameObject strikeCounter;
	public GameObject tutorial;
	public GameObject rocketSauce;
	public GameObject wrappedTaco;
	public GameObject tray;
	public bool tutorialMode = false;
	private int orderCounter = 0;
	public GameObject music;
	public TortillaFoundation tortilla;

	void Start() {
		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
	}

	private void OnMouseDown() {

		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		bool matched = false;
		int matchedOrderIndex = -1;
		for (int i = 0; i < current_recipes.Length; i++) {
			if (doOrdersMatch(current_recipes[i].GetComponent<RecipeScript>().ingredients,ingredients)) {

			//ADD JUICE!
				//found a match
				matched = true;
				matchedOrderIndex = i;
				orderCounter++;
				if (orderCounter == 25) {
					Social.ReportProgress( "com.dataplayed.unsavory.25tacos", 100f, (result) => {
						Debug.Log ( result ? "Reported Taco Progress" : "Failed to report taco progress");
					});
				}
				if (orderCounter == 100) {
					Social.ReportProgress( "com.dataplayed.unsavory.100tacos", 100f, (result) => {
						Debug.Log ( result ? "Reported Taco Progress" : "Failed to report taco progress");
					});
				}

				if (orderCounter == 500) {
					Social.ReportProgress( "com.dataplayed.unsavory.500tacos", 100f, (result) => {
						Debug.Log ( result ? "Reported Taco Progress" : "Failed to report taco progress");
					});
				}

				
				
				currentOrder.GetComponent<CurrentOrderScript>().setOrdersServed(orderCounter);
				break;
			}

		}


		if (matched) {
			//clean up
			//get rid of the current order
			Destroy(current_recipes[matchedOrderIndex]);
			Vector3 tacoPosition = transform.position;
			GameObject taco = (GameObject) Instantiate(wrappedTaco, tacoPosition, Quaternion.identity);
			//			taco.transform.parent = transform;
			tray.SetActive(false);

			if (containsIngredient(ingredients, rocketSauce)) {
				Debug.Log("Has Rocket Sauce");
				music.GetComponent<MusicLibrary>().rocket();
				taco.GetComponent<Animator>().SetTrigger("rocket");

			}
			else {
				Debug.Log("No Rocket Sauce");
				music.GetComponent<MusicLibrary>().plated();
				taco.GetComponent<Animator>().SetTrigger("normal");

			}
			tray.SetActive(true);

		}
		else {
			music.GetComponent<MusicLibrary>().misplated();
		}
		//remove the list of ingredients

		ingredients.Clear();
		//find all the placed ingredients and delete them
		GameObject[] foodOnTheTable = GameObject.FindGameObjectsWithTag("Ingredient");
		for(int j = 0; j < foodOnTheTable.Length; j++) {
			Destroy (foodOnTheTable[j]);
		}
		//TODO: Check tutorial
		if (tutorialMode) {
			tutorial.GetComponent<TutorScript>().finishTutorial();
			tutorialMode = false;
		}
		tortilla.lockTrays();
	}

	public int getOrders() {
		return orderCounter;
	}

	public bool hasIngredient(GameObject ingredient) {
		if(ingredients.Contains(ingredient)) {
			return true;
		}
		else {
			return false;
		}
	}
	public bool containsIngredient(List<GameObject> list, GameObject ingredient) {
		if (list.Contains(ingredient)) {
			return true;
		}
		else {
			return false;
		}
	}

	public bool doOrdersMatch(List<GameObject> list1, List<GameObject> list2) {
		if (list1.Count != list2.Count) {
			return false;
		}
		else {
			for (int i = 0; i < list1.Count; i++) {
				if (!list2.Contains(list1[i]) | !list1.Contains(list2[i])) {
					return false;
				}
			}
		}
		return true;
	}

}
