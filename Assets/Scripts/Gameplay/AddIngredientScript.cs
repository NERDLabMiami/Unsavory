using UnityEngine;
using System.Collections;

public class AddIngredientScript : MonoBehaviour {
	public GameObject ingredient = null;
	public GameObject plate;
	private bool tainted = false;
	public string tutorialLesson;
	public GameObject tutorial;
	void Start() {
	}

	private void OnMouseDown() {
		if (ingredient != null) {
			if (!plate.GetComponent<PlateScript>().hasIngredient(ingredient)) {
				Debug.Log("Ingredient not on plate yet");
				Spawn ();
				plate.GetComponent<PlateScript>().ingredients.Add(ingredient);
			}
			else {
				Debug.Log("Ingredient already on plate");
			}
		}
		bool jiggling = gameObject.GetComponent<Animator>().GetBool("jiggling");
		if (jiggling) {
			GetComponent<Animator>().SetBool("jiggling", false);
			tutorial.GetComponent<TutorScript>().advanceTutorial();
			GetComponent<BoxCollider2D>().enabled = false;
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
		//newIngredientPosition.x += numberOfIngredients - 2;
		GameObject item = (GameObject) Instantiate(ingredient, newIngredientPosition, Quaternion.identity);
		item.transform.parent = plate.transform;
	}

	
}
