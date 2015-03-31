using UnityEngine;
using System.Collections;
using UnionAssets.FLE;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

public class Tweet : MonoBehaviour {
	public Text message;
	// Use this for initialization
	void Start () {
//		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void tweet() {

		if (SPTwitter.instance.IsAuthed) {
			SPTwitter.instance.Post(message.text);
			Social.ReportProgress( "activist", 100, (result) => {
				Debug.Log ( result ? "Reported #activist" : "Failed to report #activist");
			});

		}
		else {
			SPTwitter.instance.AuthenticateUser();
		}
	}
	

}
