using UnityEngine;
using System.Collections;

public class CurrentOrderScript : MonoBehaviour {
	public GameObject[] recipes;
	public GameObject levelTimer;
	public GameObject continueButton;
	public float timeBetweenOrders = 8;
	public int maximumNumberOfBackedUpOrders = 5;
	public string tooSlowTitleString;
	public string tooSlowMessageString;
	public string retryButtonText;
	public int retrySceneNumber;
	private float nextXPosition;
	private bool spawnMoreOrders = true;
	// Use this for initialization
	void Start () {
		nextXPosition = 0;
		Spawn ();
	}
	
	// Update is called once per frame
	void Update () {
		GameObject[] current_recipes = GameObject.FindGameObjectsWithTag("Recipe");
		if (current_recipes.Length > maximumNumberOfBackedUpOrders && spawnMoreOrders) {
			spawnMoreOrders = false;
			//TODO: Change Continue Button to Retry
			PlayerPrefs.SetString ("ENDOFLEVEL_TITLE", tooSlowTitleString);
			PlayerPrefs.SetString ("ENDOFLEVEL_MESSAGE", tooSlowMessageString);
			continueButton.GetComponent<TextMesh>().text = retryButtonText;
			continueButton.GetComponent<LoadSceneTimer>().sceneNumber = retrySceneNumber;
			levelTimer.GetComponent<TimerScript>().EndOfLevel();
			//Application.LoadLevel(1);
		}
	}

	public void Spawn() {
//TODO: Stop creating when level has finished	
		Vector3 newRecipePosition = gameObject.transform.position;
		if (spawnMoreOrders) {
			Instantiate(recipes[Random.Range (0, recipes.Length)], gameObject.transform.position, Quaternion.identity);
		}

		Invoke("Spawn", timeBetweenOrders);

	}

}
