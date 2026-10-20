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
	public OrderQueueLayout2D layout;
	private readonly List<GameObject> activeOrders = new List<GameObject>();


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
	[Tooltip("Scales the per-level 'waiting time' from levels.txt (1 = as authored). One shared timer covers the front order.")]
	public float waitingTimeMultiplier = 3f;
	private float orderTimer;
	private float arrivalTimer;

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
	public void RemoveOrderAfterAnim(GameObject order, float seconds = 0.4f)
	{
		if (order == null) return;
		activeOrders.Remove(order);
		Destroy(order, seconds);
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
	void Update()
{
    // Keep the queue clean
    PruneNullOrders();
    // Always layout while the level is active (or even always; your choice)
    if (layout != null)
        layout.Layout(activeOrders);

    if (timeTrial)
    {
        // TOO MANY ORDERS ON THE SCREEN
        if (activeOrders.Count > maximumNumberOfBackedUpOrders && spawnMoreOrders && levelTimer.running) {
            spawnMoreOrders = false;
	        // Pay player for time worked
	        level.setTimeWorked(levelTimer.getTimeWorked());
	        level.EndRound(RoundOutcome.Slow);
        }
        return;
    }

    if (!levelTimer.running)
        return;

    // Timed arrivals: the next order slides in behind the ones already waiting, up to the cap.
    arrivalTimer -= Time.deltaTime;
    if (arrivalTimer <= 0f)
    {
        arrivalTimer = timeBetweenOrders;
        if (activeOrders.Count < maximumNumberOfBackedUpOrders)
            newOrder();
    }

    orderTimer -= Time.deltaTime;

    // Alert the FRONT order when time is low
    if (orderTimer < .5f && !orderTimerAlert)
    {
        orderTimerAlert = true;
        var front = FrontOrderOrNull();
        if (front != null)
            AnimUtil.Get<RecipeAnim>(front).Alert();
    }

    // Timer expired: front order becomes unmatched, then we spawn a new one
    if (orderTimer <= 0f)
    {
        Debug.Log("Number of Unmatched Orders: " + numberOfUnmatchedOrders);

        // The front order is thrown away; the next one in the row becomes the front.
        var front = FrontOrderOrNull();
        if (front != null)
        {
            AnimUtil.Get<RecipeAnim>(front).Unmatched();
            RemoveOrderAfterAnim(front, 0.5f);
        }

        if (warningTimer <= Time.time + 2f)
        {
            numberOfUnmatchedOrders++;
            plate.playerWarning(numberOfUnmatchedOrders);
            warningTimer = Time.time;
        }

        // The next order starts from a fresh plate with its tortilla.
        plate.NewTray(false);

        OnFrontFinished();
    }

    // Fail conditions
    if (numberOfUnmatchedOrders >= maxNumberOfUnmatchedOrders)
    {
        levelTimer.running = false;
        level.setTimeWorked(levelTimer.getTimeWorked());
        level.EndRound(RoundOutcome.Slow);
    }

}

	private void PruneNullOrders()
	{
		activeOrders.RemoveAll(o => o == null);
	}

	private GameObject FrontOrderOrNull()
	{
		PruneNullOrders();
		return (activeOrders.Count > 0) ? activeOrders[0] : null;
	}

	// The front order is done (served, thrown away or timed out): the next one in the row gets its own deadline,
	// and if nothing is waiting a new order arrives at once.
	public void OnFrontFinished()
	{
		PruneNullOrders();
		StartFrontDeadline();
		if (activeOrders.Count == 0) newOrder();
	}

	private void StartFrontDeadline()
	{
		orderTimer = timeBetweenOrders * waitingTimeMultiplier;
		orderTimerAlert = false;
	}

// Creates an order just beyond the right edge; OrderQueueLayout2D slides it to the next free slot in the row.
public void newOrder(GameObject order = null)
{
    if (order == null && recipes.Count == 0)
    {
        Debug.LogError("[CurrentOrderScript] recipes is empty, so no order can be created (check 'current level' and levels.txt).");
        return;
    }

    GameObject prefab = (order != null) ? order : recipes[Random.Range(0, recipes.Count)];

    Vector3 spawnWorld;
    if (layout != null)
        spawnWorld = layout.OffscreenRightWorld;
    else
    {
        var c = Camera.main;
        spawnWorld = c != null ? c.ViewportToWorldPoint(new Vector3(1f, 0.5f, 0f)) + Vector3.right * 2f : transform.position;
    }
    // Same lane as the row.
    if (spawningGround != null) spawnWorld.y = spawningGround.transform.position.y;
    spawnWorld.z = 0f;

    // Instantiate AT spawnWorld, then parent keeping world space, so there is no snap.
    GameObject newRecipe = (GameObject)Instantiate(prefab, spawnWorld, Quaternion.identity);
    if (spawningGround != null)
        newRecipe.transform.SetParent(spawningGround.transform, true);

    // The first card in an empty row is the front order, so its deadline starts now.
    if (activeOrders.Count == 0) StartFrontDeadline();
    arrivalTimer = timeBetweenOrders;

    activeOrders.Add(newRecipe);
}

	public List<GameObject> GetActiveOrdersUnsafe()  // quick + dirty for now
	{
		return activeOrders;
	}

}
