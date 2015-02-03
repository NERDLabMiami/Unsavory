using UnityEngine;
using System.Collections;

public class SceneLoader : MonoBehaviour {
	public Animator animation;
	private int sceneNumber;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Debug.Log("Running animation");
		if (animation.GetBool("finished")) {
			Debug.Log("Finished!");
			Application.LoadLevel(sceneNumber);
		}
		else {
			Debug.Log("Not Finished");

		}
	}

	public void setSceneNumber(int sceneNum) {
		sceneNumber = sceneNum;
		animation.SetBool("clicked", true);
	}

	public void setEndless(bool endless) {
		if (endless) {
			PlayerPrefs.SetInt("endless", 1);
		}
		else {
			PlayerPrefs.SetInt("endless", 0);
			
		}
	}
}
