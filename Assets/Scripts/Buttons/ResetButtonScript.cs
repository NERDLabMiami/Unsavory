using UnityEngine;
using System.Collections;

public class ResetButtonScript : MonoBehaviour {

	void OnMouseDown() {
		CustomFunctionScript.resetPlayerData (100f, 0);
	}
}
