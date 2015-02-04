﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class CurrentOrderScript : MonoBehaviour {
	private List<GameObject> recipes = new List<GameObject>();
	public GameObject levelTimer;
	public GameObject retryButton;
	public GameObject orderTutor;
	public GameObject player;
	private float timeBetweenOrders = 3;
	public int maximumNumberOfBackedUpOrders = 5;
	public string tooSlowTitleString;
	public string tooSlowMessageString;
	public bool tutorial = true;
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
		if (PlayerPrefs.HasKey("tutorial")) {
			tutorial = false;
			Debug.Log("Turning off tutorial");
		}
		else {
			Debug.Log("Doesn't Have Tutorial Key");
			Debug.Log("key = " + PlayerPrefs.GetInt ("tutorial"));
		}

		if (PlayerPrefs.GetInt("endless") == 1) {

		}
		else {
			currentLevel = PlayerPrefs.GetInt("current level");
			currentLevel = 0;
			levelData =  Resources.Load<TextAsset>("levels").ToString();
			JSONNode levels = JSON.Parse(levelData);
			timeBetweenOrders = levels["levels"][currentLevel]["waiting time"].AsFloat;
			numberOfRecipes = levels["levels"][currentLevel]["recipes"].AsInt;
			currentLevel = PlayerPrefs.GetInt("current level");

		}

		Debug.Log("Starting Day " + currentLevel);
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

		if (tutorial) {
			//now stop this madness
			tutorial = false;
		}

	}

	// Update is called once per frame
	void Update () {

		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		if (current_recipes.Length > maximumNumberOfBackedUpOrders && spawnMoreOrders) {
			spawnMoreOrders = false;
			retryButton.SetActive(true);
			if (levelTimer.GetComponent<TimerScript>().endless) {
					//endless, too many orders
				player.GetComponent<PlayerScript>().EndOfLevel(false, true, false);

			}
			else {
				//Pay player for time worked
				player.GetComponent<PlayerScript>().addWages( levelTimer.GetComponent<TimerScript>().getTimeWorked());
				player.GetComponent<PlayerScript>().EndOfLevel(false, false, false);

			}
		//TODO: Game over should be a retry or main menu button instead of Go Home.
			//			continueButton.GetComponent<TextMesh>().text = retryButtonText;
//			continueButton.GetComponent<LoadSceneTimer>().sceneNumber = retrySceneNumber;
		//too many orders
		}
	}

	public void Spawn() {
		Vector3 newRecipePosition = gameObject.transform.position;
		if (spawnMoreOrders) {
			if (tutorial) {
				Debug.Log("Attaching Tutorial to First Recipe");
				GameObject firstRecipe = Instantiate(recipes[Random.Range (0, recipes.Count)], gameObject.transform.position, Quaternion.identity) as GameObject;
				firstRecipe.GetComponent<RecipeScript>().setTutorialObject(orderTutor);
				firstRecipe.GetComponent<RecipeScript>().setTutorialActive();	
				tutorial = false;
			}
			else {
				Instantiate(recipes[Random.Range (0, recipes.Count)], gameObject.transform.position, Quaternion.identity);
			}
		}

		Invoke("Spawn", timeBetweenOrders);

	}

}
