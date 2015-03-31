using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnionAssets.FLE;
using SimpleJSON;
using UnityEngine.SocialPlatforms;


public class SocialFeatures : MonoBehaviour {
	public bool gcAuthenticated = false;
	public bool twitterAuthenticated = false;
	private bool hitActivateButton = false;
	public GameObject twitterInterface;
	public GameObject twitterNotFoundPanel;
	public GameObject activateButton;

	void Start() {
		if (PlayerPrefs.HasKey("activated")) {
			activateButton.SetActive(true);
		}

		Social.localUser.Authenticate (ProcessAuthentication);
	}

	void ProcessAuthentication (bool success) {
		if (success) {
			Debug.Log ("Authenticated social api");
			Social.LoadAchievements (ProcessLoadedAchievements);
			gcAuthenticated = true;
		}
		else {
			Debug.Log ("Failed to authenticate");
		}
	}


	void ProcessLoadedAchievements (IAchievement[] achievements) {
		if (achievements.Length == 0) {
			Debug.Log ("Error: no achievements found");
		}
			else {
			Debug.Log ("Got " + achievements.Length + " achievements");
		}
	}

	void Awake () {
		SPTwitter.instance.addEventListener(TwitterEvents.TWITTER_INITED,  OnInit);
		SPTwitter.instance.addEventListener(TwitterEvents.AUTHENTICATION_SUCCEEDED,  OnAuth);
		
		SPTwitter.instance.addEventListener(TwitterEvents.POST_SUCCEEDED,  OnPost);
		SPTwitter.instance.addEventListener(TwitterEvents.POST_FAILED,  OnPostFailed);
		SPTwitter.instance.addEventListener(TwitterEvents.AUTHENTICATION_FAILED, OnAuthFail);
		SPTwitter.instance.addEventListener(TwitterEvents.USER_DATA_LOADED,  OnUserDataLoaded);
		SPTwitter.instance.addEventListener(TwitterEvents.USER_DATA_FAILED_TO_LOAD,  OnUserDataLoadFailed);

		SPTwitter.instance.Init();
		


	}

	private void OnAuth() {
		Debug.Log("Authenticated");
		twitterAuthenticated = true;
	}
	private void OnAuthFail() {
		Debug.Log("Failed authenticating, no accounts?");
		if (hitActivateButton) {
			twitterNotFoundPanel.SetActive(true);
		}
	}

	private void OnInit() {
		Debug.Log("Initialized");
		if(SPTwitter.instance.IsAuthed) {
			OnAuth();
		}

	}

	private void OnPost() {
		Debug.Log("Posted");
	}

	private void OnPostFailed() {
		Debug.Log("Post Failed");
	}

	private void OnUserDataLoaded() {
		Debug.Log ("User Data Loaded");

	}

	private void OnUserDataLoadFailed() {
		Debug.Log ("User Data Failed");
	}

	public void leaderboard() {
//		Social.ShowLeaderboardUI();

	}

	public void achievements() {
			if (Social.localUser.authenticated) {
			Social.ShowLeaderboardUI();
//			Social.ShowAchievementsUI();
		}
		else {
			Debug.Log("Not Authenticated. Can't show achievements");
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

	public void checkTwitter() {
		Debug.Log("Checking Twitter");
		if (!twitterAuthenticated) {
			Debug.Log("Not Authenticated");
			hitActivateButton = true;
			SPTwitter.instance.AuthenticateUser();
		}
		else {
			Debug.Log("Authenticated");
			twitterInterface.SetActive(true);
		}
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

	void tweeted() {
		Debug.Log("Just tweeted");
	}


	
	
}
