using UnityEngine;
using System.Collections;

public class ButtonScript : MonoBehaviour {
	public bool hidden;

	void Start() {
		if (hidden) {
			setVisibility(!hidden);
		}

	}

	public void setVisibility(bool visible) {
		renderer.enabled = visible;
		Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
		for (int i = 0; i < renderers.Length; i++) {
			renderers[i].enabled = visible;
		}

	}
}
