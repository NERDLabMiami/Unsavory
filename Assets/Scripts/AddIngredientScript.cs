using UnityEngine;
using System.Collections;

public class AddIngredientScript : MonoBehaviour {
	public GameObject ingredient = null;
	public GameObject plate;
	private bool tainted = false;

	void Start() {
	}

	private void OnMouseDown() {
		if (ingredient != null) {
			Spawn ();
			plate.GetComponent<PlateScript>().ingredients.Add(ingredient);
		}
	}
	public bool getTainted() {
		return tainted;
	}
	public void setTainted(bool taint) {
		tainted = taint;
	}

	void Spawn() {
		int numberOfIngredients = plate.GetComponent<PlateScript>().ingredients.Count;
		Vector3 newIngredientPosition = plate.transform.position;
		newIngredientPosition.x += numberOfIngredients - 2;
		Instantiate(ingredient, newIngredientPosition, Quaternion.identity);
	}

	
}
