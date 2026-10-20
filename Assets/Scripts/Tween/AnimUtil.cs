using UnityEngine;

// Helpers for moving a GameObject from Animator-driven to code-driven animation without scene edits:
// the anim components are added on first use and switch the old Animator off so it stops writing poses.
public static class AnimUtil {
	public static T Get<T>(GameObject go) where T : Component {
		var c = go.GetComponent<T>();
		return c != null ? c : go.AddComponent<T>();
	}

	// The scale an object is authored at in the scene, for pop animations to return to instead of a fixed Vector3.one.
	// Panels are often saved collapsed (scale 0) while hidden; that is not a rest size, so fall back to 1.
	public static Vector3 RestScale(Transform t) {
		var s = t.localScale;
		return s.sqrMagnitude < 0.0001f ? Vector3.one : s;
	}

	public static void DisableAnimator(GameObject go) {
		var a = go.GetComponent<Animator>();
		if (a != null) a.enabled = false;
	}
}
