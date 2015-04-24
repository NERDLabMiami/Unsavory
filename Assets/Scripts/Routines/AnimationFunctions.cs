using UnityEngine;
using System.Collections;

public class AnimationFunctions : MonoBehaviour {
	public GameObject objectsWithCollisionToDisable;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void destroy() {
		Destroy (gameObject);
	}

	public void fadeAllRecipes() {
		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		for (int i = 0; i < current_recipes.Length; i++) {
			current_recipes[i].GetComponent<Animator>().SetTrigger("disappear");
		}
		Debug.Log("Fading Recipes");
	}

	public void disableCollisionOnObjects() {
		Debug.Log("RUNNING DISABLE COLLISION ON OBJECTS");
		if (objectsWithCollisionToDisable != null) {
			foreach(BoxCollider2D child in objectsWithCollisionToDisable.GetComponentsInChildren<BoxCollider2D>()) {
				Debug.Log("Disabling " + child.name);
				child.enabled = false;
			}
		}
	}

	public void enableCollisionOnObjects() {
		Debug.Log("RUNNING ENABLE COLLISION ON OBJECTS");

		if (objectsWithCollisionToDisable != null) {
			foreach(BoxCollider2D child in objectsWithCollisionToDisable.GetComponentsInChildren<BoxCollider2D>()) {
				child.enabled = true;
			}
		}
	}

	public void disable() {
		gameObject.SetActive(false);
	}

	public void setFinished() {
		GetComponent<Animator>().SetBool("finished", true);
	}

}
