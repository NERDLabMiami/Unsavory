using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class SoundEffectDefaults : MonoBehaviour {
	public AudioMixer mixer;
	void Start () {
		if(PlayerPrefs.GetFloat("sfx volume", 1) == 1) {
			mixer.SetFloat("sfxVolume", 0);

		}
		else {
			mixer.SetFloat("sfxVolume", -80f);

		}
	}
	
/*	public void setVolume(float sfxLevel) {
		float fxVol = PlayerPrefs.GetFloat("sfx volume", 1);
		mixer.SetFloat("sfx vol", sfxLevel);
//		GetComponent<AudioSource>().volume = fxVol;
	}
*/

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
