using UnityEngine;
using UnityEngine.UI;
//using UnityEngine.SocialPlatforms;

public class SocialFeatures : MonoBehaviour {

	public GameObject facebookButton;


	void Awake () {
		// Authenticate and register a ProcessAuthentication callback
		// This call needs to be made before we can proceed to other calls in the Social API
		SocialAdaptor.Authenticate();
		FB.Init(OnInitComplete, OnHideUnity);
	}

	public void leaderboard() {
		Social.ShowLeaderboardUI();
	}

	public void achievements() {
		Social.ShowAchievementsUI();
	}

	public void facebookConnect() {
		FB.Login("public_profile,email,user_friends", LoginCallback);

	}

    private void OnInitComplete()
    {
        Debug.Log("FB.Init completed: Is user logged in? " + FB.IsLoggedIn);
		if (!FB.IsLoggedIn) {
			facebookButton.SetActive(true);
		}
		else {
			facebookButton.SetActive(false);
		}

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
		FB.Login("publish_actions", LoginCallback);
	}
	
	void LoginCallback(FBResult result)
	{
		if (result.Error != null) {
			Debug.Log("Error Response: " + result.Error);
		}

		else if (!FB.IsLoggedIn) {
			Debug.Log("Login Cancelled By Player");
		}

		else {
			Debug.Log("Login Successful");
		}
	}
	
	private void CallFBLogout()
	{
		FB.Logout();
	}

	
	
}
