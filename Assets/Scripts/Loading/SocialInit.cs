using UnityEngine;
using System.Collections;

#if UNITY_ANDROID

using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

using UnityEngine.SocialPlatforms;

using Soomla.Profile;

public class SocialInit : MonoBehaviour {

	// Use this for initialization
	void Start () {
		#if UNITY_ANDROID

		PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
			// enables saving game progress.
			.EnableSavedGames()
				// registers a callback to handle game invitations received while the game is not running.
				.WithInvitationDelegate(inviteNotRunning)
				// registers a callback for turn based match notifications received while the
				// game is not running.
				.WithMatchDelegate(turnNotification)
				.Build();
		
		PlayGamesPlatform.InitializeInstance(config);
		// recommended for debugging:
		PlayGamesPlatform.DebugLogEnabled = true;
		// Activate the Google Play Games platform
		PlayGamesPlatform.Activate();
		#endif

		Social.localUser.Authenticate((bool success) => {
			Debug.Log("AUTHENTICATION: " + success.ToString());
		});
	
		SoomlaProfile.Initialize();
	}
	#if UNITY_ANDROID

	void inviteNotRunning(GooglePlayGames.BasicApi.Multiplayer.Invitation invitation, bool success) {

	}

	void turnNotification(GooglePlayGames.BasicApi.Multiplayer.TurnBasedMatch turn, bool success) {

	}
#endif

	// Update is called once per frame
	void Update () {
	
	}
}
