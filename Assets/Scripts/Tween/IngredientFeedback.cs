using UnityEngine;

// Confirmation for each ingredient as it lands on the tray: a right one pops, flashes green, shows a check and chimes;
// a wrong one flashes red, shakes and buzzes just before the tray is thrown out.
public static class IngredientFeedback {
	// How long a wrong ingredient is shown before the tray drops.
	public const float WrongHold = 0.6f;
	const float PlaceFade = 0.18f; // just over IngredientAnim's default fade-in

	static readonly Color Good = new Color(0.45f, 1f, 0.45f, 1f);
	static readonly Color Bad = new Color(1f, 0.35f, 0.35f, 1f);
	static Sprite _check;

	public static void Correct(GameObject item, MusicLibrary music) {
		if (music != null) music.select();
		if (item == null) return;
		Pop(item.transform);
		Flash(item, Good);
		SpawnCheck(item);
	}

	public static void Wrong(GameObject item, MusicLibrary music) {
		if (music != null) music.misplated();
		if (item == null) return;
		Flash(item, Bad);
		Shake(item.transform);
	}

	static void Pop(Transform t) {
		Vector3 rest = t.localScale;
		Tween.Scale(t, rest * 1.15f, 0.1f, Tween.Ease.OutQuad, 0f,
			() => { if (t != null) Tween.Scale(t, rest, 0.1f, Tween.Ease.OutQuad); });
	}

	// Waits for the ingredient's own fade-in to finish so the two don't fight over the sprite's alpha.
	static void Flash(GameObject item, Color tint) {
		Tween.Delay(PlaceFade, () => {
			if (item == null) return;
			foreach (var s in item.GetComponentsInChildren<SpriteRenderer>()) {
				var sprite = s;
				Color rest = sprite.color;
				Color flash = new Color(tint.r, tint.g, tint.b, rest.a);
				Tween.Tint(sprite, flash, 0.1f, Tween.Ease.Linear, 0f,
					() => { if (sprite != null) Tween.Tint(sprite, rest, 0.25f, Tween.Ease.Linear); });
			}
		});
	}

	static void Shake(Transform t) {
		Vector3 rest = t.localPosition;
		Vector3 left = rest + new Vector3(-0.06f, 0f, 0f), right = rest + new Vector3(0.06f, 0f, 0f);
		Tween.MoveLocal(t, left, 0.05f, Tween.Ease.Linear, 0f, () => {
			if (t == null) return;
			Tween.MoveLocal(t, right, 0.08f, Tween.Ease.Linear, 0f, () => {
				if (t != null) Tween.MoveLocal(t, rest, 0.05f, Tween.Ease.Linear);
			});
		});
	}

	// A small green check above the ingredient that floats up and fades. Drawn once as a texture, so it needs no art.
	static void SpawnCheck(GameObject item) {
		var anchor = item.GetComponentInChildren<SpriteRenderer>();
		if (anchor == null) return;
		var go = new GameObject("Check");
		var sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = CheckSprite();
		sr.color = Good;
		sr.sortingLayerID = anchor.sortingLayerID;
		sr.sortingOrder = anchor.sortingOrder + 500;
		go.transform.position = anchor.bounds.center + new Vector3(0f, anchor.bounds.extents.y * 0.5f, 0f);
		Tween.Move(go.transform, go.transform.position + new Vector3(0f, 0.5f, 0f), 0.6f, Tween.Ease.OutQuad);
		Tween.Alpha(sr, 0f, 0.6f, Tween.Ease.InQuad, 0.15f, () => Object.Destroy(go));
	}

	static Sprite CheckSprite() {
		if (_check != null) return _check;
		const int size = 32;
		var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
		var clear = new Color(1f, 1f, 1f, 0f);
		for (int y = 0; y < size; y++) for (int x = 0; x < size; x++) tex.SetPixel(x, y, clear);
		// Short stroke down-right, then long stroke up-right.
		DrawLine(tex, 6, 16, 13, 8);
		DrawLine(tex, 13, 8, 26, 25);
		tex.Apply();
		_check = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 32f);
		return _check;
	}

	static void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1) {
		int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)) * 2;
		for (int i = 0; i <= steps; i++) {
			float f = i / (float)steps;
			int cx = Mathf.RoundToInt(Mathf.Lerp(x0, x1, f)), cy = Mathf.RoundToInt(Mathf.Lerp(y0, y1, f));
			for (int dx = -1; dx <= 1; dx++) for (int dy = -1; dy <= 1; dy++)
				tex.SetPixel(cx + dx, cy + dy, Color.white);
		}
	}
}
