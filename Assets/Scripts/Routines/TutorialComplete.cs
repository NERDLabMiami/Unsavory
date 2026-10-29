using UnityEngine;
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
		if (tutor.pauseButton != null) tutor.pauseButton.enabled = true;
		PlayerPrefs.SetInt("tutorial", 1);
		PlayerPrefs.SetInt ("sneeze tutor", 0);
		tutor.plate.GetComponent<PlateScript>().tutorialMode = false;
		CustomFunctionScript.collisionOnObjects(tutor.allTrays, true);

	}

	public void startCountdown() {
		preStartObject.SetActive(true);
		
	}

	public void continueTutorial() {
		// Panel shrinks away and switches itself off (was the Tutorial Continues clip).
		var panel = AnimUtil.Get<ScalePanelAnim>(gameObject);
		panel.Hide(() => gameObject.SetActive(false), 0.5f);
		tutor.getTutorial();
	}
}
