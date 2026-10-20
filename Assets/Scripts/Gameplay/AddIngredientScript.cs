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
		if (ingredient == null) return;
		var plateScript = plate.GetComponent<PlateScript>();
		if (plateScript.hasIngredient(ingredient)) return;
		if (!plateScript.HasOrderToBuild) return;

		plateScript.EnsureTray();
		plateScript.SpawnOnTray(ingredient);
		if (music) music.playEffect(soundEffect);
		plateScript.ingredients.Add(ingredient);

		if (plateScript.tutorialMode && plateScript.tutor != null) plateScript.tutor.OnTutorialIngredientAdded();
		// Check the order last, so a completing ingredient serves it after the tutorial has noted the add.
		plateScript.OnIngredientAdded(ingredient);
	}

}
