using UnityEngine;
using System.Collections;

public class AddIngredientScript : MonoBehaviour {
	public GameObject ingredient = null;
	public GameObject plate;

	public bool has_ingredient = false;
	void Start() {
	}

	private void OnMouseDown() {
		if (ingredient != null && !has_ingredient) {
			Spawn ();
			plate.GetComponent<PlateScript>().ingredients.Add(ingredient);
		}
	}

	void Spawn() {
		int numberOfIngredients = plate.GetComponent<PlateScript>().ingredients.Count;
		Vector3 newIngredientPosition = plate.transform.position;
		newIngredientPosition.x += numberOfIngredients;
		Instantiate(ingredient, newIngredientPosition, Quaternion.identity);
	}

	
}
