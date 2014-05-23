using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class CurrentOrderScript : MonoBehaviour {
	private List<GameObject> recipes = new List<GameObject>();
	public GameObject levelTimer;
	public GameObject continueButton;
	private float timeBetweenOrders = 3;
	public int maximumNumberOfBackedUpOrders = 5;
	public string tooSlowTitleString;
	public string tooSlowMessageString;
	public string retryButtonText;
	public int retrySceneNumber;
	private bool spawnMoreOrders = true;
	private string levelData;
	private int currentLevel = 0;
	private int numberOfRecipes = 7;
	private int numberOfOrdersServed = 0;

	public void setOrdersServed(int numOrders) {
		numberOfOrdersServed = numOrders;
	}

	// Use this for initialization
	void Start () {
		if (PlayerPrefs.GetInt("endless") == 1) {

		}
		else {
			currentLevel = PlayerPrefs.GetInt("current level");
			currentLevel = 0;
			levelData =  Resources.Load<TextAsset>("levels").ToString();
			JSONNode levels = JSON.Parse(levelData);
			timeBetweenOrders = levels["levels"][currentLevel]["waiting time"].AsFloat;
			numberOfRecipes = levels["levels"][currentLevel]["recipes"].AsInt;

		}

		Debug.Log("Starting Level " + currentLevel);
		switch(numberOfRecipes) {
			case 7:
				goto case 6;
			case 6:
				goto case 5;
			case 5:
				goto case 4;
			case 4:
				goto case 3;
			case 3:
				recipes.Add((GameObject)Resources.Load("Recipes/OGB Recipe"));
				goto case 2;
			case 2:
				recipes.Add((GameObject)Resources.Load("Recipes/OGM Recipe"));
				goto case 1;
			case 1:
				recipes.Add((GameObject)Resources.Load("Recipes/RGB Recipe"));
				break;
		}
		Spawn ();
	}

	// Update is called once per frame
	void Update () {
		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		if (current_recipes.Length > maximumNumberOfBackedUpOrders && spawnMoreOrders) {
			spawnMoreOrders = false;
			PlayerPrefs.SetString ("ENDOFLEVEL_TITLE", tooSlowTitleString);
			if (levelTimer.GetComponent<TimerScript>().endless) {
				PlayerPrefs.SetString ("ENDOFLEVEL_MESSAGE", numberOfOrdersServed + " Customers Served");
			}
			else {
				PlayerPrefs.SetString ("ENDOFLEVEL_MESSAGE", tooSlowMessageString);

			}
			continueButton.GetComponent<TextMesh>().text = retryButtonText;
			continueButton.GetComponent<LoadSceneTimer>().sceneNumber = retrySceneNumber;
			levelTimer.GetComponent<TimerScript>().EndOfLevel();
		}
	}

	public void Spawn() {
		Debug.Log("Spawning New Order");
		Vector3 newRecipePosition = gameObject.transform.position;
		if (spawnMoreOrders) {
			Instantiate(recipes[Random.Range (0, recipes.Count)], gameObject.transform.position, Quaternion.identity);
		}

		Invoke("Spawn", timeBetweenOrders);

	}

}
