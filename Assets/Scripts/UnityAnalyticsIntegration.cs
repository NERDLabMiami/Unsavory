using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Cloud.Analytics;

public class UnityAnalyticsIntegration : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		
		const string projectId = "cd880932-38b5-445e-9697-d4820e85c142";
		UnityAnalytics.StartSDK (projectId);
	}

	public void startGameCareerMode() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);
		UnityAnalytics.CustomEvent("started", new Dictionary<string, object> {
			{"mode", "career"}, {"day", day}, {"money", money}, {"health", health}});
	}
	public void startGameCaterMode() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);

		UnityAnalytics.CustomEvent("started", new Dictionary<string, object> {
			{"mode", "career"}, {"day", day}, {"money", money}, {"health", health}});
	}

	public void careerModeLevelFinished(int level, string reason, float playTime) {
	
		UnityAnalytics.CustomEvent("level", new Dictionary<string, object> {
			 {"day", level}, {"type", reason}, {"time", playTime}});
	}

	public void caterModeLevelFinished(float playTime, int tacosServed, string reason) {
		
		UnityAnalytics.CustomEvent("catered", new Dictionary<string, object> {
			{"time", playTime}, {"served", tacosServed}, {"reason", reason}});
	}

	public void twitterImpression() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);

		UnityAnalytics.CustomEvent("twitter", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"source", "main menu"}});

	}

	public void twitterImpressionFromNewsletter() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		
		UnityAnalytics.CustomEvent("twitter", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"source", "newsletter"}});
		
	}

	public void tweetedInGame(int tweetNumber) {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		
		UnityAnalytics.CustomEvent("tweeted", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"tweetNum", tweetNumber}, {"in game", "true"}});
		
	}

	public void tweeted(int tweetNumber) {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		
		UnityAnalytics.CustomEvent("tweeted", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"tweetNum", tweetNumber}, {"in game", "false"}});
		
	}

	public void stayedHomeSick() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);
		UnityAnalytics.CustomEvent("stayed home", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"health", health}});

	}

	public void weekend() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);
		UnityAnalytics.CustomEvent("weekend", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"health", health}});

	}

	public void purchasedPills() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);
		UnityAnalytics.CustomEvent("purchased pill", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"health", health}});
		
	}

	public void servedOrder(string order, string matched) {
		UnityAnalytics.CustomEvent("order made", new Dictionary<string, object> {
			{"order", order}, {"matched", matched}
		});

	}
	public void usedPill() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);
		UnityAnalytics.CustomEvent("used pill", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"health", health}});
		
	}


	public void fired() {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);

		UnityAnalytics.CustomEvent("fired", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"health", health}});

	}
	//TODO: Integrate
	public void usedPaidSickDays(int sickdaysLeft) {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);
		UnityAnalytics.CustomEvent("paid sick day used", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"health", health}, {"sick days left", sickdaysLeft}});

	}

	public void survived() {
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);

		UnityAnalytics.CustomEvent("survived", new Dictionary<string, object> {
			{"money", money}, {"health", health}, {"all bills paid", "true"}});
		
	}
	public void survivedWithUnpaidBills() {
		float money = PlayerPrefs.GetFloat ("money", 0);
		float health = PlayerPrefs.GetFloat("health", 0);
		
		UnityAnalytics.CustomEvent("survived", new Dictionary<string, object> {
			{"money", money}, {"health", health}, {"all bills paid", "false"}});
		
	}

	public void overdueBill(int bill) {
		int day = PlayerPrefs.GetInt("current level", 0);
		float money = PlayerPrefs.GetFloat ("money", 0);
		
		UnityAnalytics.CustomEvent("unpaid bill", new Dictionary<string, object> {
			{"level", day}, {"money", money}, {"bill", bill}});
		
	}


}