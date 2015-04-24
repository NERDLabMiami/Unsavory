using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJSON;
using Soomla.Profile;


public class SlacktivistMachine : MonoBehaviour {

	public Text topic;
	public Text currentTweet;
	public Text nextTweetButtonText;
	public Button tweetButton;
	public TextAsset tweetFile;
	private JSONNode tweets;
	private int currentTopicIndex = 0;
	private int currentTweetIndex = 0;

	// Use this for initialization
	void Start () {
		string allTweets = tweetFile.ToString(); 
		tweets = JSON.Parse(allTweets);
		setTweet();

	}

	public void nextTopic() {
		currentTopicIndex++;
		currentTweetIndex = 0;
		if (currentTopicIndex >= tweets["topics"].Count) {
			currentTopicIndex = 0;
		}
		setTweet();
	}

	public void previousTopic() {
		currentTopicIndex--;
		currentTweetIndex = 0;
		if (currentTopicIndex < 0) {
			currentTopicIndex =  tweets["topics"].Count - 1;
		}

		setTweet();
	}

	public void newTweet() {
		currentTweetIndex++;
	
		if(currentTweetIndex >= tweets["topics"][currentTopicIndex]["tweets"].Count) {
			currentTweetIndex = 0;
		}
		setTweet();
	}

	public void setTweet() {
		if (PlayerPrefs.HasKey("tweet"+currentTopicIndex+currentTweetIndex)) {
			//TODO: Disable Tweet Button, Change Text to "Already Tweeted" or something...panel color?
			tweetButton.interactable = false;
		}
		else {
			tweetButton.interactable = true;
		}

		topic.text = tweets["topics"][currentTopicIndex]["title"];
		currentTweet.text = tweets["topics"][currentTopicIndex]["tweets"][currentTweetIndex];
		nextTweetButtonText.text = (currentTweetIndex+1).ToString() + tweets["topics"][currentTopicIndex]["tweets"].Count.ToString("/0 >");

	}


	public void tweet() {
		PlayerPrefs.SetInt("tweet"+currentTopicIndex+currentTweetIndex, 1);
		tweetButton.interactable = false;
//		SoomlaProfile.UpdateStatus (Provider.TWITTER, currentTweet.text);
		//TODO: Tweet Animation
	}


	// Update is called once per frame
	void Update () {
	
	}
}
