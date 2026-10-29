using UnityEngine;
using System.Collections.Generic;
using SimpleJSON;

public class CurrentOrderScript : MonoBehaviour {
	public List<RecipeData> recipes = new List<RecipeData>();
	public const string RecipePrefabPath = "Recipes/Recipe";
	public const string RecipeDataFolder = "Recipes/Data/";
	GameObject recipePrefab;
	public TimerScript levelTimer;
	public PlateScript plate;
	public Level level;
	public OrderQueueLayout2D layout;
	// Keyframes for the cash that replaces a served order slip.
	public Sprite cashStack, cashCrumple, cashExplode, cashSingle;
	private readonly List<GameObject> activeOrders = new List<GameObject>();


	public GameObject spawningGround;

	public int maximumNumberOfBackedUpOrders = 5;
	public int numberOfUnmatchedOrders = 0;
	public int maxNumberOfUnmatchedOrders = 3;
	private int currentLevel = 0;
	private int cateringRecipeAmount = 1;
	public float warningTimer = 0;

	public bool spawnMoreOrders = false;
	public bool timeTrial = false;
	private bool orderTimerAlert = false;
	private bool sauceGraceGiven = false;
	const float SauceGrace = 4f;   // seconds a rocket order may still wait for its sauce once every other ingredient is down

	private string levelData;
	private float timeBetweenOrders = 3;
	[Tooltip("Scales the per-level 'waiting time' from levels.txt (1 = as authored). One shared timer covers the front order.")]
	public float waitingTimeMultiplier = 3f;
	private float orderTimer;
	private float arrivalTimer;
	// Day length in orders, and the seconds the front order waits (0 = waiting time x multiplier).
	private int ordersInDay = 8;
	private float paceSeconds = 0f;
	private int ordersSpawned;

	// Use this for initialization
	void Start () {

		if (PlayerPrefs.HasKey("catering")) {
			timeBetweenOrders = 5;
			levelTimer.SetTotalOrders(ordersInDay);
			incrementRecipeList();
			incrementRecipeList();
		}
		else {
			currentLevel = PlayerPrefs.GetInt("current level",0);
			levelData =  Resources.Load<TextAsset>("levels").ToString();
			JSONNode levels = JSON.Parse(levelData);
			int lastLevel = levels["levels"].Count - 1;
			int levelIndex = CareerFlowManager.LevelIndex(currentLevel);
			if (levelIndex > lastLevel) {
				Debug.LogWarning("[CurrentOrderScript] day " + currentLevel + " is past the last entry in levels.txt; using entry " + lastLevel + ".");
				levelIndex = lastLevel;
			}
			JSONNode entry = levels["levels"][levelIndex];
			timeBetweenOrders = entry["waiting time"].AsFloat;
			if (timeBetweenOrders <= 0) {
				timeBetweenOrders = 5;
			}

			var orders = entry["orders"].AsInt;
			if (orders > 0) ordersInDay = orders;
			paceSeconds = entry["pace"].AsFloat;
			levelTimer.SetTotalOrders(ordersInDay);

			var names = entry["recipes"].AsArray;
			for (int i = 0; i < names.Count; i++) AddRecipe(names[i].Value);
		}
		// A sneeze gives the player the same seconds a clock step is worth.
		levelTimer.SetWipeWindow(FrontDeadlineSeconds);
	}

	private float FrontDeadlineSeconds { get { return paceSeconds > 0f ? paceSeconds : timeBetweenOrders * waitingTimeMultiplier; } }
	public void RemoveOrderAfterAnim(GameObject order, float seconds = 0.4f)
	{
		if (order == null) return;
		activeOrders.Remove(order);
		Destroy(order, seconds);
	}
	// Catering orders unlock one more recipe each time it is called, easiest first.
	static readonly string[] CateringRecipes = {
		"Quesadilla", "Beans and Cheese", "Garden Bean", "Loaded Bean", "Spicy Quesadilla",
		"Spicy Bean and Cheese", "Tomato Crema", "Garden Fire", "Spicy Chicken", "Spicy Chicken XL",
	};

	public void incrementRecipeList() {
		if (cateringRecipeAmount <= CateringRecipes.Length) AddRecipe(CateringRecipes[cateringRecipeAmount - 1]);
		cateringRecipeAmount++;
	}

	// Adds a recipe asset (Resources/Recipes/Data/<name>) to the pool once; a name listed twice would double its odds.
	void AddRecipe(string recipeName) {
		var data = Resources.Load<RecipeData>(RecipeDataFolder + recipeName);
		if (data == null) { Debug.LogError("[CurrentOrderScript] no recipe asset '" + recipeName + "' in Resources/" + RecipeDataFolder); return; }
		if (!recipes.Contains(data)) recipes.Add(data);
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
        if (activeOrders.Count < maximumNumberOfBackedUpOrders && ordersSpawned < ordersInDay)
            newOrder();
    }

    orderTimer -= Time.deltaTime;

    // Every ingredient is down and only the hot sauce is missing: the order gets a few extra seconds to wait for it.
    if (!sauceGraceGiven && plate.AwaitingSauce)
    {
        sauceGraceGiven = true;
        orderTimer = Mathf.Max(orderTimer, SauceGrace);
        orderTimerAlert = false;
        JustInTimeTip.Run(JustInTimeTip.RocketKey,
            "Rocket order! " + JustInTimeTip.Device("Press RB or RT", "Press E") + " to squeeze on the hot sauce.",
            () => plate.sauced || !plate.AwaitingSauce || (GameInput.Instance != null && GameInput.Instance.HotSaucePressed));
    }

    // Alert the FRONT order when time is low
    if (orderTimer < .5f && !orderTimerAlert)
    {
        orderTimerAlert = true;
        var front = FrontOrderOrNull();
        if (front != null)
            AnimUtil.Get<RecipeAnim>(front).Alert();
    }

    // Timer expired: front order becomes unmatched, then we spawn a new one
    // (An order already being served or thrown away is settled by that; its own finish starts the next deadline.)
    if (orderTimer <= 0f && !plate.IsResolving)
    {

        // The front order is thrown away; the next one in the row becomes the front.
        var front = FrontOrderOrNull();
        if (front != null)
        {
            AnimUtil.Get<RecipeAnim>(front).Unmatched();
            RemoveOrderAfterAnim(front, 0.5f);
        }

        // The warning panel no longer buzzes on its own (a wrong ingredient already did), so a timeout does it here.
        plate.PlayMistakeSound();
        CareerStats.OrderMissed();
        if (warningTimer <= Time.time + 2f)
        {
            numberOfUnmatchedOrders++;
            plate.playerWarning(numberOfUnmatchedOrders);
            warningTimer = Time.time;
        }
        plate.BreakStreak();

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
		levelTimer.AdvanceOrder();
		StartFrontDeadline();
		if (activeOrders.Count == 0 && ordersSpawned < ordersInDay) newOrder();
	}

	private void StartFrontDeadline()
	{
		orderTimer = FrontDeadlineSeconds;
		orderTimerAlert = false;
		sauceGraceGiven = false;
	}

// Creates an order just beyond the right edge; OrderQueueLayout2D slides it to the next free slot in the row.
public void newOrder(RecipeData order = null)
{
    if (order == null && recipes.Count == 0)
    {
        Debug.LogError("[CurrentOrderScript] recipes is empty, so no order can be created (check 'current level' and levels.txt).");
        return;
    }

    RecipeData data = (order != null) ? order : recipes[Random.Range(0, recipes.Count)];
    if (recipePrefab == null) recipePrefab = Resources.Load<GameObject>(RecipePrefabPath);
    if (data == null || recipePrefab == null)
    {
        Debug.LogError("[CurrentOrderScript] missing recipe data or Resources/" + RecipePrefabPath + ".");
        return;
    }

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
    GameObject newRecipe = (GameObject)Instantiate(recipePrefab, spawnWorld, Quaternion.identity);
    newRecipe.GetComponent<RecipeScript>().Init(data);
    if (spawningGround != null)
        newRecipe.transform.SetParent(spawningGround.transform, true);

    // The first card in an empty row is the front order, so its deadline starts now.
    if (activeOrders.Count == 0) StartFrontDeadline();
    arrivalTimer = timeBetweenOrders;

    activeOrders.Add(newRecipe);
    if (order == null) ordersSpawned++;
}

	public List<GameObject> GetActiveOrdersUnsafe()  // quick + dirty for now
	{
		return activeOrders;
	}

}
