using UnityEngine;

public class AnimationFunctions : MonoBehaviour {
	public GameObject objectsWithCollisionToDisable;
	// Use this for initialization
	public string gateReason = "AnimationFunctions";

	// Animation event: ingredient/recipe/panel clips call this when they finish ("destroy").
	public void destroy() {
		Destroy(gameObject);
	}

	public void fadeAllRecipes() {
		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		for (int i = 0; i < current_recipes.Length; i++) {
			current_recipes[i].GetComponent<Animator>().SetTrigger("disappear");
		}
	}

	public void disableCollisionOnObjects()
	{
		if (GameplayGate.Instance != null)
			GameplayGate.Instance.Lock(gateReason);
	}

	public void enableCollisionOnObjects()
	{
		if (GameplayGate.Instance != null)
			GameplayGate.Instance.Unlock(gateReason);
	}

	public void disable() {
		gameObject.SetActive(false);
	}

	public void setFinished() {
		GetComponent<Animator>().SetBool("finished", true);
	}

}
