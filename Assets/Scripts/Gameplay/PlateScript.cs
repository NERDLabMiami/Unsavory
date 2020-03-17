using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class PlateScript : MonoBehaviour {
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

	void Start() {
		
	}

	private void OnMouseDown() {
		if (ingredients.Count > 0) {
			string msg = "";
			//TODO: Create new recipe on right/wrong match
			bool newOrderCreated = false;
			GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
			bool matched = false;
			int matchedOrderIndex = -1;
			for (int i = 0; i < current_recipes.Length; i++) {
				if (doOrdersMatch(current_recipes[i].GetComponent<RecipeScript>().ingredients,ingredients)) {

				//ADD JUICE!
					//found a match
					matched = true;
					matchedOrderIndex = i;
					orderCounter++;
					matchedAggregate++;
					if (matchedAggregate > bestMatchedAggregate) {
						bestMatchedAggregate = matchedAggregate;
					}
	//COMBO COUNTER
					if (matchedAggregate > 4) {
						msg = matchedAggregate + " IN A ROW!";
						playerMessage(msg);

					}

					if (Social.localUser.authenticated) {
						if (orderCounter == 25) {
							//GKAchievementReporter.ReportAchievement( Achievements.TACOS_25, 100f, true);
						}
						if (orderCounter == 100) {

							//GKAchievementReporter.ReportAchievement( Achievements.TACOS_100, 100f, true);
						}

						if (orderCounter == 500) {
							//GKAchievementReporter.ReportAchievement( Achievements.TACOS_500, 100f, true);
						}
					}
					
					
	//				currentOrder.GetComponent<CurrentOrderScript>().setOrdersServed(orderCounter);
	//				current.setOrdersServed(orderCounter);
					break;
				}

			}


			if (matched) {
				//clean up
				//get rid of the current order
				analytics.servedOrder(current_recipes[matchedOrderIndex].name,"true");
				Vector3 tacoPosition = transform.position;
				GameObject taco = (GameObject) Instantiate(current_recipes[0].GetComponent<RecipeScript>().completed, tacoPosition, Quaternion.identity);

				for (int i = 0; i < current_recipes.Length; i++) {
					current_recipes[i].GetComponent<Animator>().SetTrigger("matched");

				}
	//			Destroy(current_recipes[matchedOrderIndex]);
				//			taco.transform.parent = transform;
				tray.SetActive(false);
				if (containsIngredient(ingredients, rocketSauce)) {
					Debug.Log("Has Rocket Sauce");
					music.GetComponent<MusicLibrary>().rocket();
					taco.GetComponent<Animator>().SetTrigger("rocket");

				}
				else {
					Debug.Log("No Rocket Sauce");
					analytics.servedOrder(current_recipes[matchedOrderIndex].name,"false");
					music.GetComponent<MusicLibrary>().plated();
					taco.GetComponent<Animator>().SetTrigger("normal");

				}
				tray.SetActive(true);

			}
			else if (!tutorialMode) {
	//			music.GetComponent<MusicLibrary>().misplated();
				current.newOrder(current_recipes[0]);
				newOrderCreated = true;
				for (int i = 0; i < current_recipes.Length; i++) {
					current_recipes[i].GetComponent<Animator>().SetTrigger("unmatched");					
				}
				current.numberOfUnmatchedOrders++;
				if (current.warningTimer <= Time.time + 2f) {
					playerWarning(current.numberOfUnmatchedOrders);
					current.warningTimer = Time.time;
				}

				matchedAggregate = 0;
				perfectLevel = false;
			}
			//remove the list of ingredients

			ingredients.Clear();
			//find all the placed ingredients and delete them
			GameObject[] foodOnTheTable = GameObject.FindGameObjectsWithTag("Ingredient");
			for(int j = 0; j < foodOnTheTable.Length; j++) {
				//TODO: Animation instead of destroying it
					if (matched) {
						Destroy (foodOnTheTable[j]);
					}
					else {
						foodOnTheTable[j].GetComponent<Animator>().SetTrigger("mismatch");
					}
			}
			if (tutorialMode) {
				//TODO: Pop up continue box
				tutor.continueTutorial(matched);
				if (!matched) {
					current_recipes[0].GetComponent<Animator>().SetTrigger("unmatched");

				}
				//			tutorialMode = false;
				msg = "";
			}
			else if (!newOrderCreated) {
				current.newOrder();

			}
	//		playerMessage(msg);
			tortilla.lockTrays();
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
