using UnityEngine;

// One tray + plate (+ tortilla and whatever gets added to it) for a single order. Ingredients are children, so
// finiing or botching the order tosses the whole set aside and a fresh one takes its place. Nothing is ever
// cleared in place, so a new set can't lose ingredients to a pending reset.
public class TraySet : MonoBehaviour {
	public const float FadeIn = 0.4f;
	// A new set starts this far right of its resting spot (just past the sauce hand) and slides in.
	static readonly Vector3 SlideInOffset = new Vector3(3.5f, 0f, 0f);
	// A served set crosses the table to the left; a botched one drops off the bottom.
	static readonly Vector3 ServedToss = new Vector3(-7f, 0f, 0f);
	static readonly Vector3 DiscardToss = new Vector3(0f, -4f, 0f);
	const float ServedTime = 0.45f, DiscardTime = 0.4f;

	// Art for the tray body, taken once from the scene's PlateScript sprite (see PlateScript.TakeTemplates).
	public struct TrayArt {
		public Sprite sprite; public Color color; public int sortingLayerID; public int sortingOrder; public Material material;
	}

	public GameObject LastPlaced;
	int _baseOrder;
	int _stack;

	// Sorting order for the next thing placed on the set: each lands above everything before it.
	public int NextOrder() { return _baseOrder + 10 * (++_stack); }
	public int TopOrder() { return _baseOrder + 1000; }

	// Builds a set where the scene's tray sits and slides it in. `plateTemplate` is the scene's inactive Plate sprite, cloned as a child.
	public static TraySet Create(Transform anchor, TrayArt art, GameObject plateTemplate) {
		var go = new GameObject("Tray Set");
		var t = go.transform;
		t.SetParent(anchor.parent, false);
		t.localPosition = anchor.localPosition + SlideInOffset;
		t.localRotation = anchor.localRotation;
		t.localScale = anchor.localScale;

		var body = go.AddComponent<SpriteRenderer>();
		body.sprite = art.sprite;
		body.color = art.color;
		body.sortingLayerID = art.sortingLayerID;
		body.sortingOrder = art.sortingOrder;
		if (art.material != null) body.sharedMaterial = art.material;

		int top = art.sortingOrder;
		if (plateTemplate != null) {
			var plate = Instantiate(plateTemplate, t, false);
			AnimUtil.DisableAnimator(plate); // the old New Plate clip would fight the tween
			plate.SetActive(true);
			foreach (var s in plate.GetComponentsInChildren<SpriteRenderer>(true)) top = Mathf.Max(top, s.sortingOrder);
		}

		var set = go.AddComponent<TraySet>();
		set._baseOrder = top;
		set.SlideIn(anchor.localPosition);
		return set;
	}

	void SlideIn(Vector3 restLocal) {
		Tween.MoveLocal(transform, restLocal, FadeIn, Tween.Ease.OutCubic);
		foreach (var s in GetComponentsInChildren<SpriteRenderer>(true)) {
			float a = s.color.a;
			var c = s.color; c.a = 0f; s.color = c;
			Tween.Alpha(s, a, FadeIn, Tween.Ease.Linear);
		}
	}

	// Throws the set away and removes it. A served set crosses the table to the left; a botched one drops down.
	// When served, the ingredients are already part of the finished taco, so they vanish at once and only the tray and plate leave.
	public void Toss(bool served) {
		// Leave the Player so it keeps flying where it is while the Player moves on.
		transform.SetParent(null, true);
		foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

		float time = served ? ServedTime : DiscardTime;
		foreach (var s in GetComponentsInChildren<SpriteRenderer>(true)) {
			if (served && s.CompareTag("Ingredient")) { Destroy(s.gameObject); continue; }
			Tween.Cancel(s, "alpha");
			Tween.Alpha(s, 0f, time, Tween.Ease.InQuad);
		}
		if (!served) Tween.Rotate(transform, new Vector3(0f, 0f, 12f), time, Tween.Ease.InQuad);
		Tween.Cancel(transform, "pos");
		Tween.Move(transform, transform.position + (served ? ServedToss : DiscardToss), time, Tween.Ease.InQuad, 0f, () => Destroy(gameObject));
	}
}
