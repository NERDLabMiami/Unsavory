using UnityEngine;
using System.Collections;

public class AnimationFunctions : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void disable() {
		gameObject.SetActive(false);
	}
	public void setFinished() {
		GetComponent<Animator>().SetBool("finished", true);
	}

}
