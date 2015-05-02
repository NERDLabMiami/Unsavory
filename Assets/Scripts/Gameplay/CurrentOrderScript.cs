using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class CurrentOrderScript : MonoBehaviour {
	public List<GameObject> recipes = new List<GameObject>();
	public TimerScript levelTimer;
	public PlateScript plate;
	public Level level;

	public GameObject spawningGround;

	public int maximumNumberOfBackedUpOrders = 5;
	public int numberOfUnmatchedOrders = 0;
	public int maxNumberOfUnmatchedOrders = 3;
	private int currentLevel = 0;
//	private int numberOfRecipes = 7;
	private int cateringRecipeAmount = 1;
	public float warningTimer = 0;

	public bool spawnMoreOrders = false;
	public bool timeTrial = false;
	private bool orderTimerAlert = false;

	private string levelData;
	private float timeBetweenOrders = 3;
	private float orderTimer;

	//	private bool catering = false;
	//	private int numberOfOrdersServed = 0;


	/*
	public void setOrdersServed(int numOrders) {
		numberOfOrdersServed = numOrders;
	}
	*/
	// Use this for initialization
	void Start () {

//		if (PlayerPrefs.HasKey("tutorial")) {
//			Debug.Log("Turning off tutorial");
//			tutorial = false;
//		}
		if (PlayerPrefs.HasKey("catering")) {
//			catering = true;
			timeBetweenOrders = 5;
			incrementRecipeList();
			incrementRecipeList();
		}
		else {
//			catering = false;
			currentLevel = PlayerPrefs.GetInt("current level",0);
			levelData =  Resources.Load<TextAsset>("levels").ToString();
			JSONNode levels = JSON.Parse(levelData);
			timeBetweenOrders = levels["levels"][currentLevel]["waiting time"].AsFloat;
			if (timeBetweenOrders <= 0) {
				timeBetweenOrders = 5;
			}
			Debug.Log("TIME BETWEEN ORDERS: " + timeBetweenOrders);

//			numberOfRecipes = levels["levels"][currentLevel]["recipes"].AsInt;
			int numberOfSets = levels["levels"][currentLevel]["recipes"].AsArray.Count;
			if (numberOfSets == 0) {
				for (int i = 0; i < currentLevel; i++) {
					incrementRecipeList();
				}

			}
			for (int i = 0; i < numberOfSets; i++) {
				loadRecipeSet(levels["levels"][currentLevel]["recipes"][i].AsInt);
			}
//			loadRecipes(numberOfRecipes);
//			Debug.Log(numberOfRecipes + " Recipes Loaded");
		}
	}

	public void incrementRecipeList() {
		switch(cateringRecipeAmount) {
		case 1:
			recipes.Add((GameObject)Resources.Load("Recipes/2-A"));
				break;
		case 2:
				recipes.Add((GameObject)Resources.Load("Recipes/3-B"));
				break;
		case 3:
			recipes.Add((GameObject)Resources.Load("Recipes/4-A"));
			break;
		case 4:
			recipes.Add((GameObject)Resources.Load("Recipes/3-D"));
			break;
		case 5:
			recipes.Add((GameObject)Resources.Load("Recipes/4-C"));
			break;
		case 6:
			recipes.Add((GameObject)Resources.Load("Recipes/5-D"));
			break;
		case 7:
			recipes.Add((GameObject)Resources.Load("Recipes/5-E"));
			break;
		case 8:
			recipes.Add((GameObject)Resources.Load("Recipes/2-A-R"));
			break;
		case 9:
			recipes.Add((GameObject)Resources.Load("Recipes/3-A-R"));
			break;
		case 10:
			recipes.Add((GameObject)Resources.Load("Recipes/3-C-R"));
			break;
		case 11:
			recipes.Add((GameObject)Resources.Load("Recipes/4-B-R"));
			break;
		case 12:
			recipes.Add((GameObject)Resources.Load("Recipes/4-D-R"));
			break;
		case 13:
			recipes.Add((GameObject)Resources.Load("Recipes/5-A-R"));
			break;
		case 14:
			recipes.Add((GameObject)Resources.Load("Recipes/5-B-R"));
			break;
		case 15:
			recipes.Add((GameObject)Resources.Load("Recipes/6-A"));
			break;
		case 16:
			recipes.Add((GameObject)Resources.Load("Recipes/6-B-R"));
			break;

		}
		cateringRecipeAmount++;
	}

	void loadRecipeSet(int set) {
		switch(set) {
			case 1:
				recipes.Add((GameObject)Resources.Load("Recipes/2-A"));
				recipes.Add((GameObject)Resources.Load("Recipes/3-B"));
				recipes.Add((GameObject)Resources.Load("Recipes/4-A"));
				break;
			case 2:
				recipes.Add((GameObject)Resources.Load("Recipes/3-D"));
				recipes.Add((GameObject)Resources.Load("Recipes/4-C"));
				break;
			case 3:
				recipes.Add((GameObject)Resources.Load("Recipes/2-A"));
				recipes.Add((GameObject)Resources.Load("Recipes/3-B"));
				break;
			case 4:
				recipes.Add((GameObject)Resources.Load("Recipes/6-A"));
				recipes.Add((GameObject)Resources.Load("Recipes/5-E"));
				recipes.Add((GameObject)Resources.Load("Recipes/5-D"));
				break;
			case 5:
				recipes.Add((GameObject)Resources.Load("Recipes/5-E"));
				recipes.Add((GameObject)Resources.Load("Recipes/5-D"));
				break;
			case 6:
				recipes.Add((GameObject)Resources.Load("Recipes/2-A-R"));
				recipes.Add((GameObject)Resources.Load("Recipes/3-A-R"));
				break;
			case 7:
				recipes.Add((GameObject)Resources.Load("Recipes/3-B"));
				recipes.Add((GameObject)Resources.Load("Recipes/6-A"));
				recipes.Add((GameObject)Resources.Load("Recipes/5-D"));
				recipes.Add((GameObject)Resources.Load("Recipes/2-A-R"));
				recipes.Add((GameObject)Resources.Load("Recipes/4-A"));
				break;
			case 8:
				recipes.Add((GameObject)Resources.Load("Recipes/3-C-R"));
				break;
			case 9:
				recipes.Add((GameObject)Resources.Load("Recipes/4-B-R"));
				recipes.Add((GameObject)Resources.Load("Recipes/4-D-R"));
				break;
			case 10:
				recipes.Add((GameObject)Resources.Load("Recipes/6-B-R"));
				break;
			case 11:
				recipes.Add((GameObject)Resources.Load("Recipes/5-B-R"));
				recipes.Add((GameObject)Resources.Load("Recipes/5-A-R"));
				break;
			case 12:
				recipes.Add((GameObject)Resources.Load("Recipes/2-A"));
				recipes.Add((GameObject)Resources.Load("Recipes/3-B"));
				recipes.Add((GameObject)Resources.Load("Recipes/3-C-R"));
				recipes.Add((GameObject)Resources.Load("Recipes/4-A"));
				recipes.Add((GameObject)Resources.Load("Recipes/4-B-R"));
				recipes.Add((GameObject)Resources.Load("Recipes/5-D"));
				recipes.Add((GameObject)Resources.Load("Recipes/6-A"));
				break;

		}
	}

	
	void pickShortRecipes(int amount) {
		recipes.Add((GameObject)Resources.Load("Recipes/2-A"));
		recipes.Add((GameObject)Resources.Load("Recipes/3-B"));
		recipes.Add((GameObject)Resources.Load("Recipes/4-A"));
		recipes.Add((GameObject)Resources.Load("Recipes/3-D"));
		recipes.Add((GameObject)Resources.Load("Recipes/4-C"));


	}

	void pickShortRocketRecipes(int amount) {
		recipes.Add((GameObject)Resources.Load("Recipes/2-A-R"));
		recipes.Add((GameObject)Resources.Load("Recipes/3-A-R"));
		recipes.Add((GameObject)Resources.Load("Recipes/3-C-R"));
		recipes.Add((GameObject)Resources.Load("Recipes/4-B-R"));
		recipes.Add((GameObject)Resources.Load("Recipes/4-D-R"));

	}

	void pickLongRecipes(int amount) {
		recipes.Add((GameObject)Resources.Load("Recipes/6-A"));
		recipes.Add((GameObject)Resources.Load("Recipes/5-E"));
		recipes.Add((GameObject)Resources.Load("Recipes/5-D"));

	}

	void pickLongRocketRecipes(int amount) {
		recipes.Add((GameObject)Resources.Load("Recipes/6-B-R"));
		recipes.Add((GameObject)Resources.Load("Recipes/5-B-R"));
		recipes.Add((GameObject)Resources.Load("Recipes/5-A-R"));

	}

	void loadRecipes(int amount) {
		switch (amount) {

			case 16:
				recipes.Add((GameObject)Resources.Load("Recipes/6-B-R"));
				goto case 15;

			case 15:
				recipes.Add((GameObject)Resources.Load("Recipes/6-A"));
				goto case 14;

			case 14:
				recipes.Add((GameObject)Resources.Load("Recipes/5-B-R"));
				goto case 13;
	
			case 13:
				recipes.Add((GameObject)Resources.Load("Recipes/5-A-R"));
				goto case 12;

			case 12:
				recipes.Add((GameObject)Resources.Load("Recipes/4-D-R"));
				goto case 11;
	
			case 11:
				recipes.Add((GameObject)Resources.Load("Recipes/4-B-R"));
				goto case 10;

			case 10:
				recipes.Add((GameObject)Resources.Load("Recipes/3-C-R"));
				goto case 9;

			case 9:
				recipes.Add((GameObject)Resources.Load("Recipes/3-A-R"));
				goto case 8;

			case 8:
				recipes.Add((GameObject)Resources.Load("Recipes/2-A-R"));
				goto case 7;
		
			case 7:
				recipes.Add((GameObject)Resources.Load("Recipes/5-E"));
				goto case 6;
		
			case 6:
				recipes.Add((GameObject)Resources.Load("Recipes/5-D"));
				goto case 5;
	
			case 5:
				recipes.Add((GameObject)Resources.Load("Recipes/4-C"));
				goto case 4;
	
			case 4:
				recipes.Add((GameObject)Resources.Load("Recipes/3-D"));
				goto case 3;
	
			case 3:
				recipes.Add((GameObject)Resources.Load("Recipes/4-A"));
				goto case 2;		

			case 2:
				recipes.Add((GameObject)Resources.Load("Recipes/3-B"));
				goto case 1;	
		
			case 1:
			recipes.Add((GameObject)Resources.Load("Recipes/2-A"));
			break;
		}
	}
	

	// Update is called once per frame
	void Update () {
		if (timeTrial) {
			GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
			//TOO MANY ORDERS ON THE SCREEN
			if (current_recipes.Length > maximumNumberOfBackedUpOrders && spawnMoreOrders && levelTimer.running) {
				spawnMoreOrders = false;
				if (levelTimer.catering) {
					level.EndCatering(true);
				}
				else {
					//Pay player for time worked
					level.setTimeWorked(levelTimer.getTimeWorked());
//					player.addWages( levelTimer.getTimeWorked());
					level.EndOfLevel(false, false);

				}
			}
		}
		else if(levelTimer.running){
			orderTimer -= Time.deltaTime;
			if (orderTimer < .5 && !orderTimerAlert) {
				orderTimerAlert = true;
				GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
				current_recipes[0].GetComponent<Animator>().SetTrigger("alert");
			}

			if (orderTimer <= 0) {
				Debug.Log("Number of Unmatched Orders: " + numberOfUnmatchedOrders);
				GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
				current_recipes[0].GetComponent<Animator>().SetTrigger("unmatched");

				if (warningTimer <= Time.time + 2f) {
					numberOfUnmatchedOrders++;
					plate.playerWarning(numberOfUnmatchedOrders);
					warningTimer = Time.time;
				}
				newOrder(null,orderTimer);
			}

			if (numberOfUnmatchedOrders >= maxNumberOfUnmatchedOrders && !levelTimer.catering) {
				levelTimer.running = false;
				level.setTimeWorked(levelTimer.getTimeWorked());
				level.EndOfLevel(false,false);

			}

			if (numberOfUnmatchedOrders >= maxNumberOfUnmatchedOrders && levelTimer.catering) {
				levelTimer.running = false;
				level.EndCatering(true);
			}



		}
	}

	public void newOrder(GameObject order = null, float timeBonus = 0) {
		orderTimerAlert = false;
		Debug.Log("New Order");
		GameObject newRecipe;
		if (order != null) {
			 newRecipe =(GameObject) Instantiate(order, gameObject.transform.position, Quaternion.identity);
		}
		else {
			newRecipe =(GameObject) Instantiate(recipes[Random.Range (0, recipes.Count)], gameObject.transform.position, Quaternion.identity);

		}
		newRecipe.transform.parent = spawningGround.transform;
		orderTimer = timeBetweenOrders + timeBonus;
		Debug.Log("Order Timer is Now: " + orderTimer);

	}

	public void Spawn() {
		if (spawnMoreOrders) {
				GameObject newRecipe =(GameObject) Instantiate(recipes[Random.Range (0, recipes.Count)], gameObject.transform.position, Quaternion.identity);
				newRecipe.transform.parent = spawningGround.transform;
		}

		Invoke("Spawn", timeBetweenOrders);

	}

}
