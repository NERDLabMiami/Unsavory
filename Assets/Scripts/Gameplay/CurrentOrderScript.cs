using UnityEngine;
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
	public GameObject spawningGround;

	private float timeBetweenOrders = 3;
	public int maximumNumberOfBackedUpOrders = 5;
	public bool tutorial = true;
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
		if (PlayerPrefs.HasKey("tutorial")) {
			Debug.Log("Turning off tutorial");
			tutorial = false;
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
				recipes.Add((GameObject)Resources.Load("Recipes/Beans and Rice"));
				goto case 2;
			case 2:
				recipes.Add((GameObject)Resources.Load("Recipes/Chicken and Beans"));
				goto case 1;
			case 1:
				recipes.Add((GameObject)Resources.Load("Recipes/Chicken and Rice"));
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
			if (tutorial) {
				Debug.Log("Attaching Tutorial to First Recipe");
				GameObject firstRecipe = Instantiate(recipes[Random.Range (0, recipes.Count)], gameObject.transform.position, Quaternion.identity) as GameObject;
				firstRecipe.GetComponent<RecipeScript>().setTutorialObject(orderTutor);
				firstRecipe.GetComponent<RecipeScript>().setTutorialActive();	
				tutorial = false;
			}
			else {
				GameObject newRecipe =(GameObject) Instantiate(recipes[Random.Range (0, recipes.Count)], gameObject.transform.position, Quaternion.identity);
				newRecipe.transform.parent = spawningGround.transform;
			}
		}

		Invoke("Spawn", timeBetweenOrders);

	}

}
