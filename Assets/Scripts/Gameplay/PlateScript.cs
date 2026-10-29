using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
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
	public bool perfectLevel = true;

	// Hot sauce never counts toward matching or as a mistake; it is required only by orders whose RecipeData.rocket is set,
	// and otherwise it just makes the rocket taco.
	public bool sauced = false;
	public AudioClip hotSauceSound;
	public AudioClip cashRegisterSound;

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
		item.transform.SetParent(current_set.transform, false); // keep the prefab's local scale (0.3) instead of compensating for the tray scale
		// Each addition renders above everything placed before it, whatever order its prefab was authored with.
		int order = alwaysOnTop ? current_set.TopOrder() : current_set.NextOrder();
		var sprites = item.GetComponentsInChildren<SpriteRenderer>(true);
		for (int i = 0; i < sprites.Length; i++) sprites[i].sortingOrder = order + i;
		IngredientAnim.PlaceIn(item);
		current_set.LastPlaced = item;
		return item;
	}

	/// <summary>True while an order is being confirmed (the serve hold) or thrown away (the wrong-ingredient hold): its deadline no longer applies.</summary>
	public bool IsResolving { get { return _serving || _discarding; } }

	/// <summary>True when every regular ingredient of a rocket order is on the plate and only the hot sauce is missing.</summary>
	public bool AwaitingSauce {
		get {
			if (sauced || IsResolving || tutorialMode) return false;
			GameObject target = GetTargetOrder();
			var rs = target != null ? target.GetComponent<RecipeScript>() : null;
			return rs != null && NeedsRocket(rs) && doOrdersMatch(MatchableIngredients(rs.ingredients), MatchableIngredients(ingredients));
		}
	}

	public void ApplyHotSauce() {
		// Sauce only goes on a plate that has an order to build; pressed between orders it would carry over to the next one.
		if (sauced || !HasOrderToBuild || _discarding) return;
		sauced = true;
		if (rocketSauce != null) SpawnOnTray(rocketSauce, true);
		if (hotSauceSound != null && music != null) music.GetComponent<MusicLibrary>().playEffect(hotSauceSound);

		// An order that was only waiting for its sauce is finished by it.
		GameObject target = GetTargetOrder();
		var rs = target != null ? target.GetComponent<RecipeScript>() : null;
		if (rs == null || !NeedsRocket(rs)) return;
		rs.MarkCorrect(rocketSauce);
		if (doOrdersMatch(MatchableIngredients(rs.ingredients), MatchableIngredients(ingredients))) ConfirmOrder(target);
	}

	// Orders for a rocket taco are not done until the hot sauce is on.
	bool NeedsRocket(RecipeScript rs) {
		return rs.data != null && rs.data.rocket;
	}

	/// <summary>True while there is an order on screen to build. Ingredients are refused otherwise: with no order to judge them
	/// they would sit on the plate unchecked and spoil the next order (e.g. a press just after the tutorial's last order).</summary>
	public bool HasOrderToBuild { get { return !_serving && GetTargetOrder() != null; } }

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
	public bool OnIngredientAdded(GameObject added) {
		if (_discarding || _serving) return false;
		GameObject target = GetTargetOrder();
		if (target == null) return false;
		var rs = target.GetComponent<RecipeScript>();
		if (rs == null) return false;

		var needed = MatchableIngredients(rs.ingredients);
		Debug.Log($"[Plate] added {added.name}; target '{target.name}' needs [{string.Join(", ", needed.Select(n => n.name))}], plate has [{string.Join(", ", MatchableIngredients(ingredients).Select(n => n.name))}] (tutorialMode={tutorialMode}, orders={current.GetActiveOrdersUnsafe().Count})");
		var lastOnTray = current_set != null ? current_set.LastPlaced : null;
		var musicLib = music != null ? music.GetComponent<MusicLibrary>() : null;
		if (MatchableIngredients(new List<GameObject> { added }).Count > 0 && !needed.Contains(added)) {
			// Let the wrong ingredient show (red flash and buzz) before the tray drops.
			_discarding = true;
			rs.MarkWrong(added);
			IngredientFeedback.Wrong(lastOnTray, musicLib);
			// The front timer may have thrown this order away during the hold; it is already counted then.
				Tween.Delay(IngredientFeedback.WrongHold, () => {
					_discarding = false;
					if (target != null && (tutorialMode || current.GetActiveOrdersUnsafe().Contains(target))) DiscardOrder(target);
				});
			return true;
		}
		rs.MarkCorrect(added);
		if (doOrdersMatch(needed, MatchableIngredients(ingredients)) && (!NeedsRocket(rs) || sauced)) {
			ConfirmOrder(target);
			return false;
		}
		// Right ingredient, order not finished yet.
		if (MatchableIngredients(new List<GameObject> { added }).Count > 0) IngredientFeedback.Correct(lastOnTray, musicLib);
		return false;
	}

	bool _discarding;

	// How long the finished plate is shown (the last ingredient landing) before it turns into the taco.
	const float ServeHold = 0.45f;

	void ConfirmOrder(GameObject matchedOrder) {
		orderCounter++;
		matchedAggregate++;
		if (matchedAggregate > bestMatchedAggregate) {
			bestMatchedAggregate = matchedAggregate;
		}
		//COMBO COUNTER
		string combo = ComboMessages.ForStreak(matchedAggregate);
		if (combo != null) playerMessage(combo);


		// Let the last ingredient show on the tortilla before the plate becomes the taco; ignore presses meanwhile.
		_serving = true;
		Tween.Delay(ServeHold, () => { if (this != null) FinishServe(matchedOrder); });
	}

	bool _serving;

	// Swaps the plated ingredients for the completed taco and sends the order off the table.
	void FinishServe(GameObject matchedOrder) {
		_serving = false;
		if (!tutorialMode) CareerStats.OrderServed();
		var matchedRS = matchedOrder != null ? matchedOrder.GetComponent<RecipeScript>() : null;

		if (current_set != null) current_set.ClearIngredients();
		if (matchedRS != null && matchedRS.completed != null && current_set != null)
		{
			var setT = current_set.transform;
			GameObject taco = (GameObject)Instantiate(matchedRS.completed, setT.position, Quaternion.identity);
			// The taco art is sized for the plate at scale 1; follow the tray's scale.
			taco.transform.localScale = Vector3.Scale(taco.transform.localScale, setT.lossyScale);
			foreach (var sr in taco.GetComponentsInChildren<SpriteRenderer>(true)) sr.sortingOrder += current_set.TopOrder();

			if (sauced)
			{
				music.GetComponent<MusicLibrary>().rocket();
				AnimUtil.Get<TacoAnim>(taco).Rocket();
			}
			else
			{
				music.GetComponent<MusicLibrary>().plated();
				// The taco rides the tray off the table.
				taco.transform.SetParent(setT, true);
				AnimUtil.Get<TacoAnim>(taco).Stay();
			}
		}

		if (matchedOrder != null) {
			Vector3 left = current.layout != null ? current.layout.OffscreenLeftWorld : matchedOrder.transform.position + Vector3.left * 10f;
			left.y = matchedOrder.transform.position.y;
			if (cashRegisterSound != null) music.GetComponent<MusicLibrary>().playEffect(cashRegisterSound);
			AnimUtil.Get<RecipeAnim>(matchedOrder).CashOut(current.cashStack, current.cashCrumple, current.cashExplode, current.cashSingle, left);
			if (tutorialMode) Destroy(matchedOrder, RecipeAnim.CashDuration);
			else current.RemoveOrderAfterAnim(matchedOrder, RecipeAnim.CashDuration);
		}

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

			CareerStats.OrderMissed();
			current.numberOfUnmatchedOrders++;
			if (current.warningTimer <= Time.time + 2f) {
				playerWarning(current.numberOfUnmatchedOrders);
				current.warningTimer = Time.time;
			}

			BreakStreak();
			perfectLevel = false;
		}

		NewTray(false);

		if (tutorialMode) {
			tutor.continueTutorial(false);
		}
	}

	/// <summary>A missed order (wrong ingredient or timeout) ends the streak; a long enough one gets a message.</summary>
	public void BreakStreak() {
		string broken = ComboMessages.ForBroken(matchedAggregate);
		if (broken != null) playerMessage(broken);
		matchedAggregate = 0;
	}

	public void PlayMistakeSound() {
		if (music != null) music.GetComponent<MusicLibrary>().misplated();
	}

	public void playerWarning(int warningNumber) {
		GameObject message = (GameObject) Instantiate(warningMessage, messageEmitter.transform.position, Quaternion.identity);
		message.transform.SetParent(messageEmitter.transform);
		AnimUtil.Get<WarningPanelAnim>(message).Show(warningNumber);


	}

	// Popups play one at a time, so a "streak broken" line waits for the combo popup before it.
	readonly Queue<string> _messageQueue = new Queue<string>();
	bool _messageShowing;

	public void playerMessage(string messageToSend) {
		_messageQueue.Enqueue(messageToSend);
		if (!_messageShowing) ShowNextMessage();
	}

	void ShowNextMessage() {
		if (_messageQueue.Count == 0) { _messageShowing = false; return; }
		_messageShowing = true;

		GameObject message = (GameObject) Instantiate(comboMessage, messageEmitter.transform.position, Quaternion.identity);
		message.GetComponent<TextMeshProUGUI>().text = _messageQueue.Dequeue();
		message.transform.SetParent(messageEmitter.transform);
		AnimUtil.Get<ComboTextAnim>(message).Play();

		Tween.Delay(ComboTextAnim.Duration, () => { if (this != null) ShowNextMessage(); });
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
