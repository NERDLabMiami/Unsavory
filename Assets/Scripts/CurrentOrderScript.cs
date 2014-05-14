using UnityEngine;
using System.Collections;

public class CurrentOrderScript : MonoBehaviour {
	public GameObject[] recipes;
	public float timeBetweenOrders = 8;
	private float nextXPosition;
	// Use this for initialization
	void Start () {
		nextXPosition = 0;
		Spawn ();
	}
	
	// Update is called once per frame
	void Update () {
		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		if (current_recipes.Length > 5) {
			PlayerPrefs.SetString ("Game Over Message", "Too Slow, You're Fired!");

			//Application.LoadLevel(1);
		}
	}

	public void Spawn() {
	
		Vector3 newRecipePosition = gameObject.transform.position;
		Instantiate(recipes[Random.Range (0, recipes.Length)], gameObject.transform.position, Quaternion.identity);
		Invoke("Spawn", timeBetweenOrders);

	}

}
