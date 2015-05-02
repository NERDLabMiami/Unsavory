using UnityEngine;
using System.Collections;

public class Electricity : MonoBehaviour {
	public Animator lights;
	public BillScript electricityBill;
	private bool switched = false;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (!switched && electricityBill.overdue) {
			lights.SetTrigger("lights off");
			switched = true;
		}

		if (switched && !electricityBill.overdue) {
			lights.SetTrigger("lights on");
		}
	}
}
