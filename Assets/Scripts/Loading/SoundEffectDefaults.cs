using UnityEngine;
using UnityEngine.Audio;

public class SoundEffectDefaults : MonoBehaviour {
	public AudioMixer mixer;
	void Start () {
		float vol = PlayerPrefs.GetFloat("sfx volume", 1);
		float db = vol <= 0.0001f ? -80f : Mathf.Max(-80f, Mathf.Log10(vol) * 20f);
		mixer.SetFloat("sfxVolume", db);
	}
	
	public void sfxToggle(bool off) {
		if (off) {
			mixer.SetFloat("sfxVolume", -80f);
		}
		else {
			mixer.SetFloat("sfxVolume", 0);
		}
	}
	// Update is called once per frame
	void Update () {
	
	}
}
