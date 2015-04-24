using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;

public class Settings : MonoBehaviour {
	private float sfxvol;
	private float bgvol;
	public Toggle music;
	public Toggle sfx;
	public Toggle paidSickDays;
	public AudioMixer mixer;
	public AudioSource mainAudio;

	// Use this for initialization
	void Start () {


		if(!PlayerPrefs.HasKey("sfx volume")) {
			PlayerPrefs.SetFloat("sfx volume", 1);
		}

		
		if(!PlayerPrefs.HasKey("background volume")) {
			PlayerPrefs.SetFloat("background volume", 1);
		}

		if (PlayerPrefs.GetFloat("sfx volume",0) == 0) {
			sfx.isOn = true;
		}

		if (PlayerPrefs.GetFloat("background volume",0) == 0) {
			music.isOn = true;

		}

		if (PlayerPrefs.HasKey("using paid sick days")) {
			paidSickDays.isOn = false;
		}

		paidSickDays.interactable = PlayerPrefs.HasKey("paid sick days achieved");

	}

	public void toggleSoundEffectsVolume(bool off) {
		if (off) {
			PlayerPrefs.SetFloat("sfx volume", 0);
			mixer.SetFloat("sfxVolume", -80f);
		}
		else {
			PlayerPrefs.SetFloat("sfx volume", 1);
			mixer.SetFloat("sfxVolume", 0);

		}
	}

	public void toggleBackgroundVolume(bool off) {
		if (off) {
			PlayerPrefs.SetFloat("background volume", 0);
			mainAudio.mute = true;

		}
		else {
			PlayerPrefs.SetFloat("background volume", 1);
			mainAudio.mute = false;
		}

	}

	public void togglePaidSickDays(bool off) {
		if (off) {
			PlayerPrefs.DeleteKey("using paid sick days");
		}
		else {
			PlayerPrefs.SetInt("using paid sick days",1);
		}

	}

	// Update is called once per frame
	void Update () {
	
	}
}
