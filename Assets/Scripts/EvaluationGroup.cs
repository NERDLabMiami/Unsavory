using UnityEngine;
using System.Collections;

public class EvaluationGroup : MonoBehaviour {
	private int evaluationGroup;
	// Use this for initialization
	void Start () {
		if (!PlayerPrefs.HasKey("evaluation group")) {
			evaluationGroup = Random.Range (0,7);
			PlayerPrefs.SetInt("evaluation group", evaluationGroup);
		}

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
