using UnityEngine;
using System.Collections;
//using UnionAssets.FLE;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;
//using Soomla.Profile;

public class Tweet : MonoBehaviour {
	public Text message;
	public Animator buttonAnimation;
	// Use this for initialization
	void Start () {
//		GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
//		ProfileEvents.OnSocialActionFinished += onSocialActionFinished;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
		/*
	public void onSocialActionFinished(Provider provider, SocialActionType action, string payload) {
		GKAchievementReporter.ReportAchievement( Achievements.ACTIVIST, 100.00f, true);

		// provider is the social provider
		// action is the social action (like, post status, etc..) that finished
		// payload is an identification string that you can give when you initiate the social action operation and want to receive back upon its completion
		
		// ... your game specific implementation here ...
	}
*/
	public void tweet() {
	//	if (SoomlaProfile.IsLoggedIn(Provider.TWITTER)) {
	//		buttonAnimation.SetTrigger("tweeted");
	//		SoomlaProfile.UpdateStatus(Provider.TWITTER, message.text + " http://goo.gl/ZUM31T");
	//	}
	//	else {
	//		SoomlaProfile.Login(Provider.TWITTER);
	//		Debug.Log("Not logged in yet");
	//	}
		/*
		if (SPTwitter.instance.IsAuthed) {
			SPTwitter.instance.Post(message.text);
			Social.ReportProgress( "CgkIjOCjtq4PEAIQAg", 100, (result) => {
				Debug.Log ( result ? "Reported #activist" : "Failed to report #activist");
			});

		}
		else {
			SPTwitter.instance.AuthenticateUser();
		}
		*/
	}
	

}
