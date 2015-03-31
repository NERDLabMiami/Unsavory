using UnityEngine;
using System.Collections;

public class MusicLibrary : MonoBehaviour {
//	public AudioClip characterBackground;
	public AudioClip characterResponse;
	public AudioClip characterResponse2;
	public AudioClip bossTalk;
//	public AudioClip bossReappears;
	public AudioClip tutorialBackground;
	public AudioClip preGameplayBackground;
	public AudioClip letsGo;
	public AudioClip gameplayBackground;
	public AudioClip pauseEffect;
	public AudioClip selectEffect;
	public AudioClip popPillEffect;
	public AudioClip phoneRingEffect;
	public AudioClip spendMoneyEffect;
	public AudioClip unpauseEffect;
	public AudioClip orderFinishedEffect;
	public AudioClip rocketSauceEffect;
	public AudioClip orderWrongEffect;
	public AudioClip sneezeEffect;
	public AudioClip swipeNoseEffect;

	// Use this for initialization
	void Start () {
		setVolume();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	public void playEffect(AudioClip clip) {
		GetComponent<AudioSource>().PlayOneShot(clip);
	}

	public void setVolume() {
		float sfVol = PlayerPrefs.GetFloat("sfx volume",1);
		GetComponent<AudioSource>().volume = sfVol;

	}

	private void play() {
		GetComponent<AudioSource>().Play();
	}

	public void phoneRing() {
		GetComponent<AudioSource>().clip = phoneRingEffect;
		play();

	}
	public void boss() {
		GetComponent<AudioSource>().clip = bossTalk;
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
		GetComponent<AudioSource>().loop = true;
		play();

	}

	public void countdownFinished() {
		GetComponent<AudioSource>().loop = false;
		GetComponent<AudioSource>().clip = letsGo;
		play ();
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

	public void rocket() {
		GetComponent<AudioSource>().clip = rocketSauceEffect;
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
