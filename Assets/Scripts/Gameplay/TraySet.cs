using UnityEngine;

// One tray + plate (+ tortilla and whatever gets added to it) for a single order. Ingredients are children, so
// finishing or botching the order slides the whole set off to the left and a fresh one slides in from the right.
// Nothing is ever cleared in place, so a new set can't lose ingredients to a pending reset.
public class TraySet : MonoBehaviour {
	public const float SlideTime = 0.4f;
	// A set enters from just off the right of the view and leaves just off the left; this is the gap past the edge.
	const float OffscreenMargin = 0.5f;
	const float ExitTime = 0.5f;

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
		t.localPosition = anchor.localPosition;
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

	// Starts fully off the right edge of the camera view and slides to the resting spot.
	void SlideIn(Vector3 restLocal) {
		var b = WorldBounds();
		var cam = Camera.main;
		if (cam != null) transform.position += Vector3.right * (RightEdge(cam) + OffscreenMargin - b.min.x);
		Tween.MoveLocal(transform, restLocal, SlideTime, Tween.Ease.OutCubic);
	}

	static float RightEdge(Camera cam) { return cam.transform.position.x + cam.orthographicSize * cam.aspect; }
	static float LeftEdge(Camera cam) { return cam.transform.position.x - cam.orthographicSize * cam.aspect; }

	// World bounds of every sprite in the set (tray, plate, and whatever is on it).
	Bounds WorldBounds() {
		var rs = GetComponentsInChildren<SpriteRenderer>(true);
		var b = new Bounds(transform.position, Vector3.zero);
		for (int i = 0; i < rs.Length; i++) if (rs[i].sprite != null) b.Encapsulate(rs[i].bounds);
		return b;
	}

	// Removes the ingredients (tortilla, fillings, sauce) so only the tray and plate are left.
	public void ClearIngredients() {
		foreach (var s in GetComponentsInChildren<SpriteRenderer>(true)) if (s.CompareTag("Ingredient")) Destroy(s.gameObject);
	}

	// Slides the set off the left of the view and removes it, for a served order and a botched one alike.
	// A served set carries its taco with it (the ingredients were already swapped for the taco).
	public void Toss(bool served) {
		// Leave the Player so it keeps moving where it is while the Player moves on.
		transform.SetParent(null, true);
		foreach (var col in GetComponentsInChildren<Collider2D>()) col.enabled = false;

		var cam = Camera.main;
		float dx = cam != null ? LeftEdge(cam) - OffscreenMargin - WorldBounds().max.x : -10f;
		Tween.Cancel(transform, "pos");
		Tween.Move(transform, transform.position + Vector3.right * dx, ExitTime, Tween.Ease.InQuad, 0f, () => Destroy(gameObject));
	}
}
