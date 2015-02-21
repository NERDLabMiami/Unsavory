using UnityEngine;
using System.Collections;

public class MusicLibrary : MonoBehaviour {
	public AudioClip characterBackground;
	public AudioClip characterResponse;
	public AudioClip characterResponse2;
	public AudioClip tutorialBackground;
	public AudioClip preGameplayBackground;
	public AudioClip gameplayBackground;
	public AudioClip pauseEffect;
	public AudioClip selectEffect;
	public AudioClip popPillEffect;
	public AudioClip phoneRingEffect;
	public AudioClip spendMoneyEffect;
	public AudioClip unpauseEffect;
	public AudioClip firedEffect;
	public AudioClip endOfDayEffect;
	public AudioClip dayIncompleteEffect;
	public AudioClip orderFinishedEffect;
	public AudioClip orderWrongEffect;
	public AudioClip addBeansEffect;
	public AudioClip addRiceEffect;
	public AudioClip addTortillaEffect;
	public AudioClip addCreamEffect;
	public AudioClip addRocketSauceEffect;
	public AudioClip addCheeseEffect;
	public AudioClip addChickenEffect;
	public AudioClip sneezeEffect;
	public AudioClip swipeNoseEffect;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}


	private void play() {
		GetComponent<AudioSource>().Play();
	}

	public void phoneRing() {
		GetComponent<AudioSource>().clip = phoneRingEffect;
		play();

	}
	public void respond() {
		GetComponent<AudioSource>().clip = characterResponse;
		play();
	}

	public void respond2() {
		GetComponent<AudioSource>().clip = characterResponse2;
		play();

	}

	public void prestart() {
		GetComponent<AudioSource>().clip = preGameplayBackground;
		play();

	}

	public void spendMoney() {
		GetComponent<AudioSource>().clip = spendMoneyEffect;
		play();
		
	}
	public void popPill() {
		GetComponent<AudioSource>().clip = popPillEffect;
		play();
		
	}

	public void pause() {
		GetComponent<AudioSource>().clip = pauseEffect;
		play();

	}

	public void unpause() {
		GetComponent<AudioSource>().clip = unpauseEffect;
		play();

	}

	public void select() {
		GetComponent<AudioSource>().clip = selectEffect;
		play();

	}
	public void misplated() {
		GetComponent<AudioSource>().clip = orderWrongEffect;
		play ();

	}

	public void plated() {
		GetComponent<AudioSource>().clip = orderFinishedEffect;
		play ();
	}
	public void levelCompleted() {
		GetComponent<AudioSource>().clip = endOfDayEffect;
		play ();

	}
	public void levelFailed() {
		GetComponent<AudioSource>().clip = dayIncompleteEffect;
		play ();
	}

	public void wipeNose() {
		GetComponent<AudioSource>().clip = swipeNoseEffect;
		play ();

	}

	public void sneeze() {
		GetComponent<AudioSource>().clip = sneezeEffect;
		play ();

	}
}
