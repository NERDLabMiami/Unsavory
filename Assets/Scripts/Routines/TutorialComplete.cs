using UnityEngine;
using System.Collections;
using UnityEngine.UI;


public class TutorialComplete : MonoBehaviour {
	public GameObject preStartObject;
	public TutorScript tutor;
	public Text message;
	public Text title;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void finishTutorial() {
		tutor.tortilla.tutoring = false;
		Time.timeScale = 1.0f;
		tutor.pauseButton.enabled = true;
		PlayerPrefs.SetInt("tutorial", 1);
		PlayerPrefs.SetInt ("sneeze tutor", 0);
		tutor.plate.GetComponent<PlateScript>().tutorialMode = false;
		CustomFunctionScript.collisionOnObjects(tutor.allTrays, true);

	}

	public void startCountdown() {
		preStartObject.SetActive(true);
		
	}

	public void continueTutorial() {
		GetComponent<Animator>().SetTrigger("continue");
		tutor.getTutorial();
		tutor.gameObject.GetComponent<Animator>().SetTrigger("next");
	}
}
