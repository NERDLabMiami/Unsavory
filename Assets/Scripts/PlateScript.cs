using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlateScript : MonoBehaviour {
//	public GameObject[] droppedIngredients;
	public List<GameObject> ingredients = new List<GameObject>();
	public GameObject currentOrder;
	public GameObject strikeCounter;

	private void OnMouseDown() {
		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		bool matched = false;
		int matchedOrderIndex = -1;
		for (int i = 0; i < current_recipes.Length; i++) {

//			if (ScrambledGOEquals(current_recipes[i].GetComponent<RecipeScript>().ingredients,ingredients)) {
			if (doOrdersMatch(current_recipes[i].GetComponent<RecipeScript>().ingredients,ingredients)) {

			//ADD JUICE!
				//found a match
				matched = true;
				matchedOrderIndex = i;
				break;
			}

		}


		if (matched) {
			//clean up
			//get rid of the current order
			Destroy(current_recipes[matchedOrderIndex]);
		}
		//remove the list of ingredients

		ingredients.Clear();
		//find all the placed ingredients and delete them
		GameObject[] foodOnTheTable = GameObject.FindGameObjectsWithTag("Ingredient");
		for(int j = 0; j < foodOnTheTable.Length; j++) {
			Destroy (foodOnTheTable[j]);
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
