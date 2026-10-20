using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class PlateScript : MonoBehaviour, IWorldInteractable {
//	public GameObject[] droppedIngredients;
	public List<GameObject> ingredients = new List<GameObject>();
	public CurrentOrderScript current;
//	private int strikeCounter;
	public GameObject messageEmitter;
	public GameObject comboMessage;
	public GameObject warningMessage;
	public TutorScript tutor;
	public GameObject rocketSauce;
	public GameObject wrappedTaco;
	public GameObject tray;
	public bool tutorialMode = false;
	private int orderCounter = 0;
	private int matchedAggregate = 0;
	public int bestMatchedAggregate = 0;
	public GameObject music;
	public TortillaFoundation tortilla;
	public UnityAnalyticsIntegration analytics;
	public bool perfectLevel = true;

	// Hot sauce is an optional bonus: it never counts toward matching or as a mistake, it only makes the rocket taco.
	public bool sauced = false;
	public AudioClip hotSauceSound;

	// Plate clicks no longer submit: orders are checked as each ingredient is added.
	public void Interact() { }

	// The scene's own tray sprite and `tray` (the Plate child) are only templates: every order gets a TraySet built from them.
	TraySet current_set;
	TraySet.TrayArt _art;

	bool _templatesTaken;

	// Takes the scene's tray art once and hides the originals. Lazy, so it works whichever of Start or the first order runs first.
	void TakeTemplates() {
		if (_templatesTaken) return;
		_templatesTaken = true;
		var body = GetComponent<SpriteRenderer>();
		if (body != null) {
			_art = new TraySet.TrayArt {
				sprite = body.sprite, color = body.color, sortingLayerID = body.sortingLayerID,
				sortingOrder = body.sortingOrder, material = body.sharedMaterial
			};
			body.sprite = null;
		}
		if (tray != null) tray.SetActive(false);
	}

	void Start() { TakeTemplates(); }

	// Makes sure a tray + plate + tortilla is on the table. Does nothing if one already is.
	public void EnsureTray() {
		if (current_set == null) NewTray(false, false);
	}

	// Tosses the current set (if any) and brings in a fresh tray, plate and tortilla, ready for the next ingredient.
	public void NewTray(bool served) { NewTray(served, true); }

	// Tosses the current set and leaves the table empty; EnsureTray brings the next one in when it is needed.
	// Used when the tutorial is won, so no fresh tray shows up just before the boss dialogue hides the kitchen.
	void ReleaseTray(bool served) {
		if (current_set != null) current_set.Toss(served);
		current_set = null;
		ingredients.Clear();
		sauced = false;
	}

	void NewTray(bool served, bool tossOld) {
		TakeTemplates();
		if (current_set != null && tossOld) current_set.Toss(served);
		ingredients.Clear();
		sauced = false;
		current_set = TraySet.Create(transform, _art, tray);
		PlaceTortilla();
	}

	// Every order starts with a tortilla on the plate, with no input from the player.
	void PlaceTortilla() {
		if (tortilla == null) return;
		var tortillaTray = tortilla.GetComponent<AddIngredientScript>();
		if (tortillaTray == null || tortillaTray.ingredient == null) return;
		ingredients.Add(tortillaTray.ingredient);
		SpawnOnTray(tortillaTray.ingredient);
	}

	// Puts a copy of an ingredient prefab on top of the current set.
	public GameObject SpawnOnTray(GameObject prefab, bool alwaysOnTop = false) {
		EnsureTray();
		GameObject item = (GameObject)Instantiate(prefab, current_set.transform.position, Quaternion.identity);
		item.transform.parent = current_set.transform;
		// Each addition renders above everything placed before it, whatever order its prefab was authored with.
		int order = alwaysOnTop ? current_set.TopOrder() : current_set.NextOrder();
		var sprites = item.GetComponentsInChildren<SpriteRenderer>(true);
		for (int i = 0; i < sprites.Length; i++) sprites[i].sortingOrder = order + i;
		IngredientAnim.PlaceIn(item);
		current_set.LastPlaced = item;
		return item;
	}

	public void ApplyHotSauce() {
		if (sauced) return;
		sauced = true;
		if (rocketSauce != null) SpawnOnTray(rocketSauce, true);
		if (hotSauceSound != null && music != null) music.GetComponent<MusicLibrary>().playEffect(hotSauceSound);
	}

	/// <summary>True while there is an order on screen to build. Ingredients are refused otherwise: with no order to judge them
	/// they would sit on the plate unchecked and spoil the next order (e.g. a press just after the tutorial's last order).</summary>
	public bool HasOrderToBuild { get { return GetTargetOrder() != null; } }

	GameObject GetTargetOrder() {
		if (tutorialMode && tutor != null && tutor.CurrentRecipe != null) return tutor.CurrentRecipe;
		var orders = current.GetActiveOrdersUnsafe();
		for (int i = 0; i < orders.Count; i++) {
			if (orders[i] != null) return orders[i];
		}
		return null;
	}

	// Tortilla and hot sauce are handled separately, so they never take part in matching.
	List<GameObject> MatchableIngredients(List<GameObject> list) {
		GameObject seeded = tortilla != null && tortilla.GetComponent<AddIngredientScript>() != null
			? tortilla.GetComponent<AddIngredientScript>().ingredient : null;
		return list.Where(i => i != rocketSauce && i != seeded).ToList();
	}

	// Called after each ingredient actually lands on the plate: a wrong one discards the order, a completing one serves it.
	public void OnIngredientAdded(GameObject added) {
		if (_discarding) return;
		GameObject target = GetTargetOrder();
		if (target == null) return;
		var rs = target.GetComponent<RecipeScript>();
		if (rs == null) return;

		var needed = MatchableIngredients(rs.ingredients);
		Debug.Log($"[Plate] added {added.name}; target '{target.name}' needs [{string.Join(", ", needed.Select(n => n.name))}], plate has [{string.Join(", ", MatchableIngredients(ingredients).Select(n => n.name))}] (tutorialMode={tutorialMode}, orders={current.GetActiveOrdersUnsafe().Count})");
		var lastOnTray = current_set != null ? current_set.LastPlaced : null;
		var musicLib = music != null ? music.GetComponent<MusicLibrary>() : null;
		if (MatchableIngredients(new List<GameObject> { added }).Count > 0 && !needed.Contains(added)) {
			// Let the wrong ingredient show (red flash and buzz) before the tray drops.
			_discarding = true;
			rs.MarkWrong(added);
			IngredientFeedback.Wrong(lastOnTray, musicLib);
			Tween.Delay(IngredientFeedback.WrongHold, () => { _discarding = false; if (target != null) DiscardOrder(target); });
			return;
		}
		rs.MarkCorrect(added);
		if (doOrdersMatch(needed, MatchableIngredients(ingredients))) {
			ConfirmOrder(target);
			return;
		}
		// Right ingredient, order not finished yet.
		if (MatchableIngredients(new List<GameObject> { added }).Count > 0) IngredientFeedback.Correct(lastOnTray, musicLib);
	}

	bool _discarding;

	void ConfirmOrder(GameObject matchedOrder) {
		orderCounter++;
		matchedAggregate++;
		if (matchedAggregate > bestMatchedAggregate) {
			bestMatchedAggregate = matchedAggregate;
		}
		//COMBO COUNTER
		if (matchedAggregate > 4) {
			playerMessage(matchedAggregate + " IN A ROW!");
		}

		analytics.servedOrder(matchedOrder.name, "true");

		var matchedRS = matchedOrder.GetComponent<RecipeScript>();
		if (matchedRS != null && matchedRS.completed != null)
		{
			GameObject taco = (GameObject)Instantiate(matchedRS.completed, transform.position, Quaternion.identity);
			// The taco art is sized for the plate at scale 1; follow the plate if the layout scales it.
			taco.transform.localScale = Vector3.Scale(taco.transform.localScale, transform.lossyScale);

			if (sauced)
			{
				music.GetComponent<MusicLibrary>().rocket();
				AnimUtil.Get<TacoAnim>(taco).Rocket();
			}
			else
			{
				analytics.servedOrder(matchedOrder.name, "false");
				music.GetComponent<MusicLibrary>().plated();
				AnimUtil.Get<TacoAnim>(taco).Normal();
			}
		}

		AnimUtil.Get<RecipeAnim>(matchedOrder).PopOut();

		if (tutorialMode) Destroy(matchedOrder, 0.35f);
		else current.RemoveOrderAfterAnim(matchedOrder, 0.35f);

		if (tutorialMode) ReleaseTray(true);
		else NewTray(true);

		if (tutorialMode) {
			tutor.continueTutorial(true);
		}
		else {
			current.OnFrontFinished();
		}
	}

	// A wrong ingredient throws away the whole order and earns a warning.
	void DiscardOrder(GameObject target) {
		AnimUtil.Get<RecipeAnim>(target).Unmatched();

		if (!tutorialMode) {
			current.RemoveOrderAfterAnim(target, 0.45f);
			current.OnFrontFinished();

			current.numberOfUnmatchedOrders++;
			if (current.warningTimer <= Time.time + 2f) {
				playerWarning(current.numberOfUnmatchedOrders);
				current.warningTimer = Time.time;
			}

			matchedAggregate = 0;
			perfectLevel = false;
		}

		NewTray(false);

		if (tutorialMode) {
			tutor.continueTutorial(false);
		}
	}

	public void playerWarning(int warningNumber) {
		GameObject message = (GameObject) Instantiate(warningMessage, messageEmitter.transform.position, Quaternion.identity);
		message.transform.SetParent(messageEmitter.transform);
		AnimUtil.Get<WarningPanelAnim>(message).Show(warningNumber);


	}

	public void playerMessage(string messageToSend) {
		GameObject message = (GameObject) Instantiate(comboMessage, messageEmitter.transform.position, Quaternion.identity);
		message.GetComponent<Text>().text = messageToSend;		
		message.transform.SetParent(messageEmitter.transform);
		AnimUtil.Get<ComboTextAnim>(message).Play();

	}

	public int getOrders() {
		return orderCounter;
	}

	public bool hasIngredient(GameObject ingredient) {
		return ingredients.Contains(ingredient);
	}


	public bool doOrdersMatch(List<GameObject> list1, List<GameObject> list2) {
		if (list1.Count != list2.Count) {
			return false;
		}
		else {
			for (int i = 0; i < list1.Count; i++) {
				if (!list2.Contains(list1[i]) || !list1.Contains(list2[i])) {
					return false;
				}
			}
		}
		return true;
	}

}
