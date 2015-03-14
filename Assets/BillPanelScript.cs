using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class BillPanelScript : MonoBehaviour {

	public Text bankAccount;

	// Use this for initialization
	void Start () {
		bankAccount.text = PlayerPrefs.GetFloat("money").ToString("Account Balance: $0.00");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
