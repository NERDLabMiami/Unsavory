using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SocialPlatforms;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SimpleJSON;



public class SocialFeatures : MonoBehaviour {

	public GameObject facebookButton;


	void Awake () {
		// Authenticate and register a ProcessAuthentication callback
		// This call needs to be made before we can proceed to other calls in the Social API
//		SocialAdaptor.Authenticate();
		//FB.Init(OnInitComplete, OnHideUnity);
	}

	public void leaderboard() {
//		Social.ShowLeaderboardUI();
	}

	public void achievements() {
//		Social.ShowAchievementsUI();
	}

	public void facebookConnect() {
		//FB.Login("public_profile,email,user_friends, publish_actions", LoginCallback);

	}

    private void OnInitComplete()
    {
//        Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);
		/*if (!FB.IsLoggedIn) {
			facebookButton.SetActive(true);
		}
		else {
			facebookButton.SetActive(false);
		}
		*/

	}

	public void postToFacebook() {
		/*
		var wwwForm = new WWWForm();
		wwwForm.AddField("message", "Mmm... FB integration. A necessary evil.");
		
		FB.API("me/feed", Facebook.HttpMethod.POST, postCallback, wwwForm);
		*/
	}
	/*
     private void postCallback(FBResult result) {
		Debug.Log("Callback for post:" + result);
	}
	*/
	public void askForHelp() {
	//	FB.AppRequest("I can't pay my bills, mind helping me out?",null,null,null,1,"","Overdue Bills",null);


		/*
		if (!FB.IsLoggedIn) {
			facebookButton.SetActive(true);
		}
		else {
			facebookButton.SetActive(false);
		}
		*/
	}

    private void OnHideUnity(bool isGameShown)
    {
        Debug.Log("Is game showing? " + isGameShown);
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
	/*
	void LoginCallback(FBResult result)
	{
		if (result.Error != null) {
			Debug.Log("Error Response: " + result.Error);
		}
		/*
		else if (!FB.IsLoggedIn) {
			Debug.Log("Login Cancelled By Player");
		}

		else {
			Debug.Log("Login Successful");
		}
	}
	*/
	private void CallFBLogout()
	{
		//FB.Logout();
	}

	
	
}
