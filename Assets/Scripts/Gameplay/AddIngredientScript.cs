using UnityEngine;

public class AddIngredientScript : MonoBehaviour, IWorldInteractable {
	public GameObject ingredient = null;
	public GameObject plate;
	public MusicLibrary music;
	public AudioClip soundEffect;
	public Sprite lessSprite; // the tray's "-less" art, shown briefly when an ingredient is taken
	public float lessScale = 1f; // uniform size tweak for the less art (its import size already matches the full pile's width)

	public void Interact() { TryAdd(); }

	public void TryAdd()
	{
		if (ingredient == null) return;
		var plateScript = plate.GetComponent<PlateScript>();
		if (plateScript.hasIngredient(ingredient)) return;
		if (!plateScript.HasOrderToBuild) return;

		plateScript.EnsureTray();
		plateScript.SpawnOnTray(ingredient);
		if (lessSprite) TrayFeedback.Dip(GetComponent<SpriteRenderer>(), lessSprite, lessScale);
		plateScript.ingredients.Add(ingredient);

		if (plateScript.tutorialMode && plateScript.tutor != null) plateScript.tutor.OnTutorialIngredientAdded();
		// Check the order last, so a completing ingredient serves it after the tutorial has noted the add.
		// A wrong ingredient gets only the buzz, not the pickup sound on top of it.
		bool wrong = plateScript.OnIngredientAdded(ingredient);
		if (!wrong && music) music.playEffect(soundEffect);
	}

}
