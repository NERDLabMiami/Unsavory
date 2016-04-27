using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;
using UnityEngine.SocialPlatforms;
//using GooglePlayGames;
//using Soomla.Profile;


public class SocialFeatures : MonoBehaviour {
//	private bool hitActivateButton = false;
	public GameObject twitterInterface;
	public GameObject twitterNotFoundPanel;
	public GameObject activateButton;
	private bool openTwitterAfterLogin = false;

	void Start() {
		if (PlayerPrefs.HasKey("activated") && activateButton != null) {
			activateButton.SetActive(true);
		}
//		ProfileEvents.OnLoginFinished += onLoginFinished;
//		ProfileEvents.OnLoginCancelled += onLoginCancelled;
//		ProfileEvents.OnLoginFailed += onLoginFailed;
	}


	void Awake () {


	}
		/*
	public void onLoginFinished(Soomla.Profile.UserProfile profile, string payload) {

		if (openTwitterAfterLogin) {
			twitterInterface.SetActive(true);
			openTwitterAfterLogin = false;
		}
	}

	public void onLoginCancelled(Provider provider, string payload) {
		twitterNotFoundPanel.SetActive(true);		

	}

	public void onLoginFailed(Provider provider, string message, string payload) {
		Debug.Log("Login Failed: " + message);
		twitterNotFoundPanel.SetActive(true);		

	}

*/
	public void leaderboard() {
//		Social.ShowLeaderboardUI();

	}

	public void achievements() {
			if (Social.localUser.authenticated) {
			Social.ShowAchievementsUI();
		}
		else {
			Debug.Log("Not Authenticated. Can't show achievements");
			Social.localUser.Authenticate((bool success) => {
				if (success) {
					Social.ShowAchievementsUI();
				}
				Debug.Log("AUTHENTICATION: " + success.ToString());
			});

		}
	}

	public void facebookConnect() {
		//FB.Login("public_profile,email,user_friends, publish_actions", LoginCallback);

	}

    private void OnInitComplete()
    {
//        Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);

	}

	public void postToFacebook() {
	//	var wwwForm = new WWWForm();
	//	wwwForm.AddField("message", "Mmm... FB integration. A necessary evil.");
		
	//	FB.API("me/feed", Facebook.HttpMethod.POST, postCallback, wwwForm);
	}
	public void askForHelp() {
	//	FB.AppRequest("I can't pay my bills, mind helping me out?",null,null,null,1,"","Overdue Bills",null);

	}


	private void CallFBLoginForPublish()
	{
		// It is generally good behavior to split asking for read and publish
		// permissions rather than ask for them all at once.
		//
		// In your own game, consider postponing this call until the moment
		// you actually need it.
//		FB.Login("publish_actions", LoginCallback);
	}
	private void CallFBLogout()
	{
		//FB.Logout();
	}
	public void connectToTwitter() {
//		if(SoomlaProfile.IsLoggedIn(Provider.TWITTER)) {
//			twitterInterface.SetActive(true);
//		}
//		else {
//			if (SoomlaProfile.IsProviderNativelyImplemented(Provider.TWITTER)) {
//				openTwitterAfterLogin = true;
//				SoomlaProfile.Login(Provider.TWITTER);
//			}
//			else {
//				twitterNotFoundPanel.SetActive(true);
//			}
//		}
	}

	
	
}
