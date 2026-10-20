using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CameraSequencing : MonoBehaviour {
	public GameObject preStartObject;
	public GameObject tutorObject;

	// The camera is already on the gameplay shot (Level eases it there); the countdown or tutorial begins
	// (was the Prep trigger + clip event).
	public void Prep() {
		AnimUtil.DisableAnimator(gameObject);
		startCountdown();
	}

	// Eases to the shot used for boss dialogue (was the Pots trigger).
	public Tween ShowDialogueShot() {
		AnimUtil.DisableAnimator(gameObject);
		return CameraDirector.Go(CameraDirector.Dialogue);
	}

	public void startCountdown() {
		if (PlayerPrefs.HasKey("tutorial")) {
			if (preStartObject) preStartObject.SetActive(true);
		}
		else {
			if (tutorObject) tutorObject.SetActive(true);
		}
	}
	
}
