using UnityEngine;
using System.Collections;

public class ShiftCompleteScript : MonoBehaviour {
	public AudioClip failClip;
	public AudioClip successClip;
	public AudioSource audio;


	public void failed() {
		Debug.Log("FFFAIL");
		audio.clip = failClip;
		audio.Play();
	}

	public void success() {
		audio.clip = successClip;
		audio.Play();
	}


}
