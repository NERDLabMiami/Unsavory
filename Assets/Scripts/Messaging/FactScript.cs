using UnityEngine;
using System.Collections;
using SimpleJSON;


public class FactScript : MonoBehaviour {
	public string randomFact;
	// Use this for initialization
	void Start () {
		string factSheet = Resources.Load<TextAsset>("facts").ToString();
		JSONNode facts = JSON.Parse(factSheet);
		int selectedFactIndex = Random.Range(0, facts["facts"].Count);
		randomFact = facts["facts"][selectedFactIndex]["fact"];

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
