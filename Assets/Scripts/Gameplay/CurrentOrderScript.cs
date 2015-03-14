using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine.UI;

public class CurrentOrderScript : MonoBehaviour {
	public List<GameObject> recipes = new List<GameObject>();
	public GameObject levelTimer;
	public GameObject retryButton;
	public GameObject orderTutor;
	public GameObject player;
	public GameObject spawningGround;

	private float timeBetweenOrders = 3;
	public int maximumNumberOfBackedUpOrders = 5;
//	public bool tutorial = true;
	public bool spawnMoreOrders = false;
	private string levelData;
	private int currentLevel = 0;
	private int numberOfRecipes = 7;
	private int numberOfOrdersServed = 0;

	public void setOrdersServed(int numOrders) {
		numberOfOrdersServed = numOrders;
	}

	// Use this for initialization
	void Start () {

//		if (PlayerPrefs.HasKey("tutorial")) {
//			Debug.Log("Turning off tutorial");
//			tutorial = false;
//		}

		currentLevel = PlayerPrefs.GetInt("current level",0);
		levelData =  Resources.Load<TextAsset>("levels").ToString();
		JSONNode levels = JSON.Parse(levelData);
		timeBetweenOrders = levels["levels"][currentLevel]["waiting time"].AsFloat;
		Debug.Log("TIME BETWEEN ORDERS: " + timeBetweenOrders);
		numberOfRecipes = levels["levels"][currentLevel]["recipes"].AsInt;
		Debug.Log("Starting Day " + currentLevel);
		Debug.Log(numberOfRecipes + " Recipes Loaded");
		switch(currentLevel) {
		case 1:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));

//			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			break;
		case 2:
			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			break;
		case 3:
			//ditch quesadilla
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			break;
		case 4:
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme"));
			break;
		case 5:
			//ditching bean and cheese
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			break;
		case 6:
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Beans and Rice"));
			break;
		case 7:
			//ditching supreme
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			break;
		case 8:
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			break;
		case 9:
			//ditching rocket bean and rice
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			break;
		case 10:
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			break;
		case 11:
			recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			break;
		case 12:
			//ditching beans and rice
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			break;
		case 13:
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			break;
		case 14:
			//ditching bean cheese sour cream
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			break;
		case 15:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
			break;
		case 16:
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
			break;
		case 17:
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
			break;
		case 18:
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			break;
		case 19:
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme"));
			break;
		case 20:
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
			recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
			recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
			recipes.Add((GameObject)Resources.Load("Recipes/Supreme"));
			recipes.Add((GameObject)Resources.Load("Recipes/Rocket Beans and Rice"));
			break;

		}
		/*
		switch(numberOfRecipes) {
			case 16:
//				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Veggie Supreme No Sour Cream"));
				goto case 15;
			case 15:
//				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Beans and Rice"));
				goto case 14;
			case 14:
//				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Sour Cream"));
				goto case 13;
			case 13:
//				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme No Rice"));
				goto case 12;
			case 12:
//				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Supreme"));
				goto case 11;
			case 11:
//				recipes.Add((GameObject)Resources.Load("Recipes/Rocketdilla"));
				goto case 10;
			case 10:
//				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese with Sour Cream"));
				goto case 9;
			case 9:
//				recipes.Add((GameObject)Resources.Load("Recipes/Rocket Bean and Cheese"));
				goto case 8;
			case 8:
//				recipes.Add((GameObject)Resources.Load("Recipes/Veggie Supreme No Sour Cream"));
				goto case 7;
			case 7:
//				recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Sour Cream"));
				goto case 6;
			case 6:
//				recipes.Add((GameObject)Resources.Load("Recipes/Supreme No Rice"));
				goto case 5;
			case 5:
//				recipes.Add((GameObject)Resources.Load("Recipes/Supreme"));
				goto case 4;
			case 4:
//				recipes.Add((GameObject)Resources.Load("Recipes/Quesadilla"));
				goto case 3;
			case 3:
//				recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
				goto case 2;
			case 2:
//				recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese with Sour Cream"));
				goto case 1;
			case 1:
//				recipes.Add((GameObject)Resources.Load("Recipes/Bean and Cheese"));
				break;
		}
		*/
//		Spawn ();

//		if (tutorial) {
			//now stop this madness
//			tutorial = false;
//		}

	}

	// Update is called once per frame
	void Update () {

		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		if (current_recipes.Length > maximumNumberOfBackedUpOrders && spawnMoreOrders && levelTimer.GetComponent<TimerScript>().running) {
			spawnMoreOrders = false;
			retryButton.SetActive(true);
			if (levelTimer.GetComponent<TimerScript>().endless) {
					//endless, too many orders
				player.GetComponent<PlayerScript>().EndOfLevel(false, true, false);

			}
			else if (levelTimer.GetComponent<TimerScript>().catering) {
				//catering too slow
			}
			else {
				//Pay player for time worked
				player.GetComponent<PlayerScript>().addWages( levelTimer.GetComponent<TimerScript>().getTimeWorked());
				player.GetComponent<PlayerScript>().EndOfLevel(false, false, false);

			}
		}
	}

	public void Spawn() {
//		Vector3 newRecipePosition = gameObject.transform.position;
		if (spawnMoreOrders) {
				GameObject newRecipe =(GameObject) Instantiate(recipes[Random.Range (0, recipes.Count)], gameObject.transform.position, Quaternion.identity);
				newRecipe.transform.parent = spawningGround.transform;
		}
		Debug.Log("SPAWN RAN");

		Invoke("Spawn", timeBetweenOrders);

	}

}
