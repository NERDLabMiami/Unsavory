using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;

public class Timeline : MonoBehaviour
{
	// Start is called before the first frame update
	public Text narrative;
    void Start()
    {
		string narration = Resources.Load<TextAsset>("facts").ToString();
		int letterNumber = PlayerPrefs.GetInt("letter", 0);
		JSONNode lettersJSON = JSON.Parse(narration);
		Debug.Log("Narrative: " + letterNumber);
		
		if (PlayerPrefs.GetInt("fired", 0) == 0)
		{
			narrative.text = lettersJSON["timeline"][letterNumber]["text_employed"];

		}
		else
		{
			narrative.text = lettersJSON["timeline"][letterNumber]["text_unemployed"];
		}

	}

	// Update is called once per frame
	void Update()
    {
        
    }
}
