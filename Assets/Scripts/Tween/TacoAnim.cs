using UnityEngine;

// A finished order leaving the plate: sliding away for a normal serve, flying up for a rocket serve
// (was the Taco controller's Taco Complete / Rocket Taco clips). Removes itself when done.
public class TacoAnim : MonoBehaviour {
	void Awake() { AnimUtil.DisableAnimator(gameObject); }

	public void Normal() {
		// PlateScript spawns the taco on the plate; leave from wherever that is in the current layout.
		var start = transform.position;
		var taco = FindSprite("Taco");
		SetAlpha(taco, 1f);   // the prefab can be saved mid-fade from the old Taco Appears clip
		// The clip crosses almost 100 units in 0.82s; easing in keeps the first part of the sweep visible.
		Tween.Move(transform, new Vector3(-100f, start.y, 0f), 0.817f, Tween.Ease.InQuad, 0f, () => Destroy(gameObject));
		if (taco != null) Tween.Alpha(taco, 0f, 0.434f, Tween.Ease.Linear, 0.383f);
	}

	// The taco stays put and rides along with its parent (the tray set) instead of leaving on its own.
	public void Stay() {
		SetAlpha(FindSprite("Taco"), 1f);
	}

	public void Rocket() {
		var start = transform.position;
		Tween.Move(transform, new Vector3(start.x, TopOfView(), start.z), 1f, Tween.Ease.Linear, 0f, () => Destroy(gameObject));

		SetAlpha(FindSprite("Taco"), 1f);

		// Exhaust flicker.
		var tail = FindSprite("Rocket Tail");
		if (tail != null)
			Tween.Run(tail, "flicker", 0.667f, v => {
				var c = tail.color; c.a = Mathf.Lerp(1f, 0.7f, 1f - Mathf.Abs(2f * v - 1f)); tail.color = c;
			}, Tween.Ease.Linear).Loop(0);
	}

	// A little above the top edge of whatever the camera currently frames.
	static float TopOfView() {
		var cam = Camera.main;
		return cam != null ? cam.transform.position.y + cam.orthographicSize + 1f : 5f;
	}

	static void SetAlpha(SpriteRenderer s, float a) {
		if (s == null) return;
		var c = s.color; c.a = a; s.color = c;
	}

	SpriteRenderer FindSprite(string childName) {
		var t = transform.Find(childName);
		return t != null ? t.GetComponent<SpriteRenderer>() : null;
	}
}
