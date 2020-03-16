using UnityEngine;
using System.Collections;
//using UnionAssets.FLE;
using SimpleJSON;
using UnityEngine.UI;
using UnityEngine.SocialPlatforms;

public class Tweet : MonoBehaviour {
	public Text message;
	public Animator buttonAnimation;
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void tweet() {
        //TODO: FIND WAY TO TWEET
        /*
        if (SoomlaProfile.IsLoggedIn(Provider.TWITTER)) {
			buttonAnimation.SetTrigger("tweeted");
			SoomlaProfile.UpdateStatus(Provider.TWITTER, message.text + " http://goo.gl/ZUM31T");
		}
		else {
			SoomlaProfile.Login(Provider.TWITTER);
			Debug.Log("Not logged in yet");
		}
        */
	}
	

}
