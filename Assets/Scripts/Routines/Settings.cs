using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Settings : MonoBehaviour {
	public Slider backgroundVolumeSlider;
	public Slider soundEffectsVolumeSlider;
	public AudioSource backgroundMusicSource;
	public AudioSource soundEffectsMusicSource;
	private float sfxvol;
	private float bgvol;
	// Use this for initialization
	void Start () {
		sfxvol = PlayerPrefs.GetFloat("sfx volume", .5f);
		bgvol = PlayerPrefs.GetFloat("background volume", .5f);
		soundEffectsVolumeSlider.value = sfxvol;
		bgvol = PlayerPrefs.GetFloat("background volume", .5f);
		backgroundVolumeSlider.value = bgvol;
	}

	public void setBackgroundVolume(float v) {
//		backgroundVolumeText.text = bgvol.ToString();
		PlayerPrefs.SetFloat("background volume", v);
		backgroundMusicSource.volume = v;
		Debug.Log("Set Volume to " + v);

	}

	public void setSFXVolume(float v) {
		PlayerPrefs.SetFloat("sfx volume", v);
//		soundEffectsMusicSource.volume = v;
		Debug.Log("Set Volume to " + v);
	}

	// Update is called once per frame
	void Update () {
	
	}
}
