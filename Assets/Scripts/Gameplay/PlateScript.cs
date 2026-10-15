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

	void Start() {
		// The plate slides in on a clip; run it on real time so it still plays while the tutorial holds timeScale at 0.
		if (tray != null) {
			var a = tray.GetComponent<Animator>();
			if (a != null) a.updateMode = AnimatorUpdateMode.UnscaledTime;
		}
	}

	Coroutine _seedRoutine;

	// Every order starts with a tortilla already on the plate. This is the only place one is created.
	// It does nothing if the plate already holds just a tortilla, so extra calls (tutorial end, game start) never
	// destroy and re-place it. A delay lets the plate slide in or the old items fade out first.
	public void SeedTortilla(bool clearExisting = true, float delay = 0f) {
		if (_seedRoutine != null) { StopCoroutine(_seedRoutine); _seedRoutine = null; }
		if (delay > 0f) _seedRoutine = StartCoroutine(SeedAfter(clearExisting, delay));
		else PlaceTortilla(clearExisting);
	}

	IEnumerator SeedAfter(bool clearExisting, float delay) {
		yield return new WaitForSecondsRealtime(delay);
		_seedRoutine = null;
		PlaceTortilla(clearExisting);
	}

	void PlaceTortilla(bool clearExisting) {
		if (tortilla == null) return;
		var tortillaTray = tortilla.GetComponent<AddIngredientScript>();
		if (tortillaTray == null || tortillaTray.ingredient == null) return;

		int placed = 0;
		for (int i = 0; i < transform.childCount; i++)
			if (transform.GetChild(i).CompareTag("Ingredient")) placed++;
		if (placed == 1 && ingredients.Count == 1 && ingredients[0] == tortillaTray.ingredient) return;

		if (clearExisting) {
			for (int i = transform.childCount - 1; i >= 0; i--) {
				var child = transform.GetChild(i);
				if (child.CompareTag("Ingredient")) Destroy(child.gameObject);
			}
		}
		ingredients.Clear();
		sauced = false;
		ingredients.Add(tortillaTray.ingredient);
		GameObject item = (GameObject)Instantiate(tortillaTray.ingredient, transform.position, Quaternion.identity);
		item.transform.parent = transform;
	}

	// Clears the plate (finished or discarded) and brings the next tortilla in once the plate is ready for it.
	public void ResetPlate(bool matched) {
		ingredients.Clear();
		sauced = false;
		GameObject[] foodOnTheTable = GameObject.FindGameObjectsWithTag("Ingredient");
		for (int j = 0; j < foodOnTheTable.Length; j++) {
			if (matched) {
				Destroy(foodOnTheTable[j]);
			}
			else {
				var a = foodOnTheTable[j].GetComponent<Animator>();
				if (a) a.SetTrigger("mismatch");
			}
		}
		// Served: the plate slides back in (0.33s). Discarded: the old items fade out (0.25s).
		SeedTortilla(true, matched ? 0.35f : 0.3f);
	}

	public void ApplyHotSauce() {
		if (sauced) return;
		sauced = true;
		if (rocketSauce != null) {
			GameObject item = (GameObject)Instantiate(rocketSauce, transform.position, Quaternion.identity);
			item.transform.parent = transform;
		}
		if (hotSauceSound != null && music != null) music.GetComponent<MusicLibrary>().playEffect(hotSauceSound);
	}

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
		GameObject target = GetTargetOrder();
		if (target == null) return;
		var rs = target.GetComponent<RecipeScript>();
		if (rs == null) return;

		var needed = MatchableIngredients(rs.ingredients);
		if (MatchableIngredients(new List<GameObject> { added }).Count > 0 && !needed.Contains(added)) {
			DiscardOrder(target);
			return;
		}
		if (doOrdersMatch(needed, MatchableIngredients(ingredients))) {
			ConfirmOrder(target);
		}
	}

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

			tray.SetActive(false);
			if (sauced)
			{
				music.GetComponent<MusicLibrary>().rocket();
				taco.GetComponent<Animator>().SetTrigger("rocket");
			}
			else
			{
				analytics.servedOrder(matchedOrder.name, "false");
				music.GetComponent<MusicLibrary>().plated();
				taco.GetComponent<Animator>().SetTrigger("normal");
			}
			tray.SetActive(true);
		}

		var anim = matchedOrder.GetComponent<Animator>();
		if (anim) anim.SetTrigger("matched");

		if (tutorialMode) Destroy(matchedOrder, 0.35f);
		else current.RemoveOrderAfterAnim(matchedOrder, 0.35f);

		tray.SetActive(true);
		ResetPlate(true);

		if (tutorialMode) {
			tutor.continueTutorial(true);
		}
		else {
			current.newOrder();
		}
	}

	// A wrong ingredient throws away the whole order and earns a warning.
	void DiscardOrder(GameObject target) {
		var anim = target.GetComponent<Animator>();
		if (anim) anim.SetTrigger("unmatched");

		if (!tutorialMode) {
			current.RemoveOrderAfterAnim(target, 0.35f);
			current.newOrder(null);

			current.numberOfUnmatchedOrders++;
			if (current.warningTimer <= Time.time + 2f) {
				playerWarning(current.numberOfUnmatchedOrders);
				current.warningTimer = Time.time;
			}

			matchedAggregate = 0;
			perfectLevel = false;
		}

		ResetPlate(false);

		if (tutorialMode) {
			tutor.continueTutorial(false);
		}
	}

	public void playerWarning(int warningNumber) {
		GameObject message = (GameObject) Instantiate(warningMessage, messageEmitter.transform.position, Quaternion.identity);
		message.transform.SetParent(messageEmitter.transform);
		message.GetComponent<Animator>().SetInteger("warnings", warningNumber);
		message.GetComponent<Animator>().SetTrigger("warning");


	}

	public void playerMessage(string messageToSend) {
		GameObject message = (GameObject) Instantiate(comboMessage, messageEmitter.transform.position, Quaternion.identity);
		message.GetComponent<Text>().text = messageToSend;		
		message.transform.SetParent(messageEmitter.transform);

	}

	public int getOrders() {
		return orderCounter;
	}

	public bool hasIngredient(GameObject ingredient) {
		if(ingredients.Contains(ingredient)) {
			return true;
		}
		else {
			return false;
		}
	}
	public bool containsIngredient(List<GameObject> list, GameObject ingredient) {
		if (list.Contains(ingredient)) {
			return true;
		}
		else {
			return false;
		}
	}


	public bool doOrdersMatch(List<GameObject> list1, List<GameObject> list2) {
		if (list1.Count != list2.Count) {
			return false;
		}
		else {
			for (int i = 0; i < list1.Count; i++) {
				if (!list2.Contains(list1[i]) | !list1.Contains(list2[i])) {
					return false;
				}
			}
		}
		return true;
	}

}
