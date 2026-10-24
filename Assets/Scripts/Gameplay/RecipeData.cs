using System.Collections.Generic;
using UnityEngine;

// One kind of order. The single Recipe prefab loads one of these (RecipeScript.Init) to build its slip.
[CreateAssetMenu(menuName = "Unsavory/Recipe", fileName = "New Recipe")]
public class RecipeData : ScriptableObject {
	public List<GameObject> ingredients = new List<GameObject>();
	[Tooltip("The finished dish spawned when the order is served.")]
	public GameObject completed;
	public bool rocket;
}
