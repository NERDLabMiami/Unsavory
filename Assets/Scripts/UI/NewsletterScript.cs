using UnityEngine;
using System.Collections;
using SimpleJSON;
using UnityEngine.UI;

public class NewsletterScript : MonoBehaviour {
	public Text letterContent;

	// Use this for initialization
	void Start () {
			string letters = Resources.Load<TextAsset>("facts").ToString();
			int letterNumber = PlayerPrefs.GetInt("letter", 0);
			JSONNode lettersJSON = JSON.Parse(letters);
			letterContent.text = lettersJSON["letters"][letterNumber];	
//			PlayerPrefs.DeleteKey("letter");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
