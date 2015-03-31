using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class ShiftCompleteScript : MonoBehaviour {
	public AudioClip failClip;
	public AudioClip successClip;
	public AudioSource audio;
	public Text fact;
	public FactScript factSheet;

	void Start() {
		fact.text = factSheet.randomFact;
		//TODO: Load this beforehand? crashing
}

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
