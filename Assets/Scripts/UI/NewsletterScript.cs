using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.UI;

public class NewsletterScript : MonoBehaviour {
	public Text letterContent;
	public Sprite[] letterTypes;
	public Image letter;
	private int letterType;
	public string actionUrl;
	public GameObject actionButton;

	// Use this for initialization
	void Start () {
		string letters = Resources.Load<TextAsset>("facts").ToString();
		Debug.Log("string " + letters);
		int letterNumber = PlayerPrefs.GetInt("letter", 0);
		Debug.Log("Letter #" + letterNumber);
		JSONNode lettersJSON = JSON.Parse(letters);

		//check for employer layoff
		if (lettersJSON["letters"][letterNumber]["fired"].AsBool)
        {
			PlayerPrefs.SetInt("fired", 1);

		}

		if (lettersJSON["letters"][letterNumber]["action"].ToString().Length > 0)
		{
			actionButton.SetActive(true);
			// Create a UniWebViewSafeBrowsing instance with a URL.
			actionUrl = lettersJSON["letters"][letterNumber]["action"];
			Debug.Log("Action Associated with Letter: " + lettersJSON["letters"][letterNumber]["action"]);
		}
		if (lettersJSON["unemployed_letters"][letterNumber]["action"].ToString().Length > 0)
		{
			actionButton.SetActive(true);
			// Create a UniWebViewSafeBrowsing instance with a URL.
			actionUrl = lettersJSON["unemployed_letters"][letterNumber]["action"];
			Debug.Log("Action Associated with Letter: " + lettersJSON["letters"][letterNumber]["action"]);
		}

		if (PlayerPrefs.GetInt("fired", 0) == 0) {
				letterType = lettersJSON["letters"][letterNumber]["type"].AsInt;
				letterContent.text = lettersJSON["letters"][letterNumber]["text"];
			Debug.Log("Letter text: " + lettersJSON["letters"][letterNumber]["text"]);

		}
		else {
				letterType = lettersJSON["unemployed_letters"][letterNumber]["type"].AsInt;
				letterContent.text = lettersJSON["unemployed_letters"][letterNumber]["text"];
			Debug.Log("Letter text: " + lettersJSON["unemployed_letters"][letterNumber]["text"]);
		}

		letter.sprite = letterTypes[letterType];
		switch (letterType)
        {
			case 0:
				letterContent.text = letterContent.text + "\n\n";
				letterContent.text = letterContent.text + "Your Dear Leader";
				break;
			case 2:
				letterContent.text = letterContent.text + "\n\n";
				letterContent.text = letterContent.text + "The Management";
				break;

		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Open()
	{
		var safeBrowsing = UniWebViewSafeBrowsing.Create(actionUrl);

		// Show it on screen.
		safeBrowsing.Show();

	}
}
