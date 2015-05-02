using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Cloud.Analytics;
using SimpleJSON;
using Soomla.Profile;


public class SlacktivistMachine : MonoBehaviour {

	public Text topic;
	public Text currentTweet;
	public Text nextTweetButtonText;
	public Button tweetButton;
	public GameObject nextTopicObject;
	public GameObject previousTopicObject;
	public bool topicSwitchingEnabled = false;
	public TextAsset tweetFile;
	private JSONNode tweets;
	private int currentTopicIndex = 0;
	private int currentTweetIndex = 0;
	private int evaluationGroup;
	private float machineActivated = 0;
	// Use this for initialization
	void Start () {
		machineActivated = Time.time;
		evaluationGroup = 	PlayerPrefs.GetInt("evaluation group", 0);
		if (evaluationGroup == 4) {
			topicSwitchingEnabled = true;
		}
		else {
			Debug.Log("Setting Evaluation Group to " + evaluationGroup);
			currentTopicIndex = evaluationGroup;
			nextTopicObject.SetActive(false);
			previousTopicObject.SetActive(false);
		}

		string allTweets = tweetFile.ToString(); 
		tweets = JSON.Parse(allTweets);

		setTweet();
		PlayerPrefs.DeleteKey("letter");

	}

	public void nextTopic() {
		Debug.Log("Going to Next Topic");
		UnityAnalytics.CustomEvent("tweet_topic_switch", new Dictionary<string, object> {

			{"current_topic", currentTopicIndex}, {"current_tweet", currentTweetIndex}, {"scene", Application.loadedLevel}});

		currentTopicIndex++;
		currentTweetIndex = 0;
		if (currentTopicIndex >= tweets["topics"].Count) {
			currentTopicIndex = 0;
		}
		setTweet();
	}

	public void previousTopic() {
		Debug.Log("Going to Previous Topic");
		UnityAnalytics.CustomEvent("tweet_topic_switch", new Dictionary<string, object> {
			
			{"current_topic", currentTopicIndex}, {"current_tweet", currentTweetIndex}, {"scene", Application.loadedLevel}});

		currentTopicIndex--;
		currentTweetIndex = 0;
		if (currentTopicIndex < 0) {
			currentTopicIndex =  tweets["topics"].Count - 1;
		}

		setTweet();
	}

	public void newTweet() {
		UnityAnalytics.CustomEvent("tweet_message_switch", new Dictionary<string, object> {
			
			{"current_topic", currentTopicIndex}, {"current_tweet", currentTweetIndex}, {"scene", Application.loadedLevel}});

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
		Debug.Log("Setting Tweet for topic" + currentTopicIndex);

		topic.text = tweets["topics"][currentTopicIndex]["title"];
		currentTweet.text = tweets["topics"][currentTopicIndex]["tweets"][currentTweetIndex];
		nextTweetButtonText.text = (currentTweetIndex+1).ToString() + tweets["topics"][currentTopicIndex]["tweets"].Count.ToString("/0 >");

	}


	public void tweet() {
		float timeUntilTweetSelected = Time.time - machineActivated;
		PlayerPrefs.SetInt("tweet"+currentTopicIndex+currentTweetIndex, 1);
		UnityAnalytics.CustomEvent("tweeted", new Dictionary<string, object> {
			{"current_topic", currentTopicIndex}, {"current_tweet", currentTweetIndex}, {"scene", Application.loadedLevel},{"timer", timeUntilTweetSelected}, {"group", evaluationGroup}});

		tweetButton.interactable = false;
		SoomlaProfile.UpdateStatus (Provider.TWITTER, currentTweet.text);
	}


	// Update is called once per frame
	void Update () {
	
	}
}
