using UnityEngine;

// A panel that grows in each time its object is enabled (was the default state of the Pause Panel, Shift Complete,
// Catering and Fail canvases). Optionally holds at zero first, and locks the gameplay gate like the old clip event did.
public class PanelPopAnim : MonoBehaviour {
	public string targetPath = "";   // child to scale; blank = this object
	public float hold;
	public float duration = 0.5f;
	public bool lockGate;

	// Safe on an inactive object: nothing plays until it is enabled.
	public static PanelPopAnim Prepare(GameObject go, string targetPath, float hold, float duration, bool lockGate) {
		if (go == null) return null;
		var p = go.GetComponent<PanelPopAnim>();
		if (p == null) {
			p = go.AddComponent<PanelPopAnim>();
			p.targetPath = targetPath; p.hold = hold; p.duration = duration; p.lockGate = lockGate;
		}
		return p;
	}

	Vector3 _rest;
	bool _haveRest;

	void Awake() { AnimUtil.DisableAnimator(gameObject); }

	void OnEnable() {
		Transform target = string.IsNullOrEmpty(targetPath) ? transform : transform.Find(targetPath);
		if (target != null) {
			// Remember the scene's size the first time; later enables start from zero and come back to it.
			if (!_haveRest) { _rest = AnimUtil.RestScale(target); _haveRest = true; }
			target.localScale = Vector3.zero;
			Tween.Scale(target, _rest, duration, Tween.Ease.Linear, hold);
		}
		if (lockGate) {
			// The old clip's event only reached an AnimationFunctions on this same object.
			var af = GetComponent<AnimationFunctions>();
			if (af != null) af.disableCollisionOnObjects();
		}
	}
}
