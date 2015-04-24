using UnityEngine;
using System.Collections;

public class BackgroundSoundDefaults : MonoBehaviour {

	// Use this for initialization
	void Start () {
		setVolume();
	}

	public void setVolume() {
		float bgVol = PlayerPrefs.GetFloat("background volume", 1);
		if (bgVol == 0) {
			GetComponent<AudioSource>().mute = true;

		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
