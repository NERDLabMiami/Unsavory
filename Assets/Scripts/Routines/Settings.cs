using UnityEngine;
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

	[Header("Debug scene (optional): volume sliders")]
	public Slider sfxSlider;
	public Slider backgroundSlider;

	// Use this for initialization
	void Start () {
		if(!PlayerPrefs.HasKey("sfx volume")) {
			PlayerPrefs.SetFloat("sfx volume", 1);
		}

		if(!PlayerPrefs.HasKey("background volume")) {
			PlayerPrefs.SetFloat("background volume", 1);
		}

		// Main menu: toggles show "off" when the volume is 0. Not every scene has every control.
		if (sfx != null && PlayerPrefs.GetFloat("sfx volume",0) == 0) {
			sfx.isOn = true;
		}

		if (music != null && PlayerPrefs.GetFloat("background volume",0) == 0) {
			music.isOn = true;
		}

		if (paidSickDays != null) {
			if (PlayerPrefs.HasKey("using paid sick days")) {
				paidSickDays.isOn = false;
			}
			paidSickDays.interactable = PlayerPrefs.HasKey("paid sick days achieved");
		}

		// Debug scene: reflect the saved volumes without re-saving them.
		if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfx volume", 1));
		if (backgroundSlider != null) backgroundSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("background volume", 1));
	}

	/// <summary>Slider (0..1) for sound effects; the mixer works in decibels.</summary>
	public void setSFXVolume(float volume) {
		PlayerPrefs.SetFloat("sfx volume", volume);
		if (mixer != null) {
			mixer.SetFloat("sfxVolume", volume <= 0.0001f ? -80f : Mathf.Log10(volume) * 20f);
		}
	}

	/// <summary>Slider (0..1) for background music.</summary>
	public void setBackgroundVolume(float volume) {
		PlayerPrefs.SetFloat("background volume", volume);
		if (mainAudio != null) {
			mainAudio.mute = volume <= 0.0001f;
			mainAudio.volume = Mathf.Clamp01(volume);
		}
	}

	public void toggleSoundEffectsVolume(bool off) {
		if (off) {
			PlayerPrefs.SetFloat("sfx volume", 0);
			if (mixer != null) mixer.SetFloat("sfxVolume", -80f);
		}
		else {
			PlayerPrefs.SetFloat("sfx volume", 1);
			if (mixer != null) mixer.SetFloat("sfxVolume", 0);

		}
	}

	public void toggleBackgroundVolume(bool off) {
		if (off) {
			PlayerPrefs.SetFloat("background volume", 0);
			if (mainAudio != null) mainAudio.mute = true;

		}
		else {
			PlayerPrefs.SetFloat("background volume", 1);
			if (mainAudio != null) mainAudio.mute = false;
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
