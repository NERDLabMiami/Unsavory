using UnityEngine;

// Confirmation for each ingredient as it lands on the tray: a right one pops, flashes green and chimes;
// a wrong one flashes red, shakes and buzzes just before the tray is thrown out.
public static class IngredientFeedback {
	// How long a wrong ingredient is shown before the tray drops.
	public const float WrongHold = 0.6f;
	const float PlaceFade = 0.18f; // just over IngredientAnim's default fade-in

	static readonly Color Good = new Color(0.45f, 1f, 0.45f, 1f);
	static readonly Color Bad = new Color(1f, 0.35f, 0.35f, 1f);

	public static void Correct(GameObject item, MusicLibrary music) {
		if (music != null) music.select();
		if (item == null) return;
		Pop(item.transform);
		Flash(item, Good);
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
}
