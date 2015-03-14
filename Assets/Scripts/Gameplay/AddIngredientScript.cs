using UnityEngine;
using System.Collections;

public class AddIngredientScript : MonoBehaviour {
	public GameObject ingredient = null;
	public GameObject plate;
	public MusicLibrary music;
	public AudioClip soundEffect;
	private bool tainted = false;
	public string tutorialLesson;
	public GameObject tutorial;

	private void OnMouseDown() {
		GetComponent<Animator>().SetTrigger("tapped");
		if (ingredient != null) {
			if (!plate.GetComponent<PlateScript>().hasIngredient(ingredient)) {
				Debug.Log("Ingredient not on plate yet");
				Spawn ();
				if (music) {
					music.playEffect(soundEffect);
				}
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
		Vector3 newIngredientPosition = plate.transform.position;
		GameObject item = (GameObject) Instantiate(ingredient, newIngredientPosition, Quaternion.identity);
		item.transform.parent = plate.transform;
	}

	
}
