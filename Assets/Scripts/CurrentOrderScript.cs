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
	
	}

	public void Spawn() {
		Vector3 newRecipePosition = gameObject.transform.position;
		newRecipePosition.x += nextXPosition;
		Instantiate(recipes[Random.Range (0, recipes.Length)], newRecipePosition, Quaternion.identity);
		nextXPosition += 1;
		Invoke("Spawn", timeBetweenOrders);

	}

}
