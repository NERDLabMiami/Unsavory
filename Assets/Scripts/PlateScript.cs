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
			if (ScrambledEquals(current_recipes[i].GetComponent<RecipeScript>().ingredients,ingredients)) {
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

		//create another order
//		currentOrder.GetComponent<CurrentOrderScript>().Spawn();
	}

	public static bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2) {
		var cnt = new Dictionary<T, int>();
		foreach (T s in list1) {
			if (cnt.ContainsKey(s)) {
				cnt[s]++;
			} else {
				cnt.Add(s, 1);
			}
		}
		foreach (T s in list2) {
			if (cnt.ContainsKey(s)) {
				cnt[s]--;
			} else {
				return false;
			}
		}
		return cnt.Values.All(c => c == 0);
	}


}
