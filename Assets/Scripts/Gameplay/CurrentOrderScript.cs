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
	private int numberOfRecipes = 7;
	private int cateringRecipeAmount = 1;

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
			timeBetweenOrders = 2;
			incrementRecipeList();
			incrementRecipeList();
		}
		else {
//			catering = false;
			currentLevel = PlayerPrefs.GetInt("current level",0);
			levelData =  Resources.Load<TextAsset>("levels").ToString();
			JSONNode levels = JSON.Parse(levelData);
			timeBetweenOrders = levels["levels"][currentLevel]["waiting time"].AsFloat;
			Debug.Log("TIME BETWEEN ORDERS: " + timeBetweenOrders);
			numberOfRecipes = levels["levels"][currentLevel]["recipes"].AsInt;
			Debug.Log("Starting Day " + currentLevel);
			loadRecipes(numberOfRecipes);
			Debug.Log(numberOfRecipes + " Recipes Loaded");
		}
	}

	public void incrementRecipeList() {
		switch(cateringRecipeAmount) {
		case 1:
			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
				break;
		case 2:
				recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
				break;
		case 3:
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			break;
		case 4:
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			break;
		case 5:
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme"));
			break;
		case 6:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			break;
		case 7:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Beans and Rice"));
			break;
		case 8:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			break;
		case 9:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			break;
		case 10:
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			break;
		case 11:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			break;
		case 12:
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			break;
		case 13:
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			break;
		case 14:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
			break;
		case 15:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			break;
		case 16:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			break;

		}
		cateringRecipeAmount++;
	}


	void loadRecipes(int amount) {
		switch (amount) {

			case 16:
				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
				goto case 15;

			case 15:
				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
				goto case 14;

			case 14:
				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
				goto case 13;
	
			case 13:
				recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
				goto case 12;

			case 12:
				recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
				goto case 11;
	
			case 11:
				recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
				goto case 10;

			case 10:
				recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
				goto case 9;

			case 9:
				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
				goto case 8;

			case 8:
				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
				goto case 7;
		
			case 7:
				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Beans and Rice"));
				goto case 6;
		
			case 6:
				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
				goto case 5;
	
			case 5:
				recipes.Add((GameObject)Resources.Load("Recipes/Supreme"));
				goto case 4;
	
			case 4:
				recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
				goto case 3;
	
			case 3:
				recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
				goto case 2;		

			case 2:
				recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
				goto case 1;	
		
			case 1:
			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
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
				numberOfUnmatchedOrders++;
				GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
				current_recipes[0].GetComponent<Animator>().SetTrigger("unmatched");
				//TODO: order message pop up
				plate.playerWarning(numberOfUnmatchedOrders);
				newOrder();
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

	public void newOrder(GameObject order = null) {
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
		orderTimer = timeBetweenOrders;
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
