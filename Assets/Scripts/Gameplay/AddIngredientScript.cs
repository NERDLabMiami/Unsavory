using UnityEngine;
using System.Collections;

public class AddIngredientScript : MonoBehaviour, IWorldInteractable {
	public GameObject ingredient = null;
	public GameObject plate;
	public MusicLibrary music;
	public AudioClip soundEffect;
	private bool tainted = false;
	public string tutorialLesson;
	public GameObject tutorial;

	public bool getTainted() {
		return tainted;
	}
	public void setTainted(bool taint) {
		tainted = taint;
	}
	public void Interact() { TryAdd(); }

	public void TryAdd()
	{
		GetComponent<Animator>().SetTrigger("tapped");
		bool added = false;
		if (ingredient != null)
		{
			if (!plate.GetComponent<PlateScript>().hasIngredient(ingredient))
			{
				Spawn();
				if (music) music.playEffect(soundEffect);
				plate.GetComponent<PlateScript>().ingredients.Add(ingredient);
				added = true;
			}
		}

		if (added) {
			var plateScript = plate.GetComponent<PlateScript>();
			if (plateScript.tutorialMode && plateScript.tutor != null) plateScript.tutor.OnTutorialIngredientAdded();
			// Check the order last, so a completing ingredient serves it after the tutorial has noted the add.
			plateScript.OnIngredientAdded(ingredient);
		}
	}

	void Spawn() {
		Vector3 newIngredientPosition = plate.transform.position;
		GameObject item = (GameObject) Instantiate(ingredient, newIngredientPosition, Quaternion.identity);
		item.transform.parent = plate.transform;
	}

	
}
