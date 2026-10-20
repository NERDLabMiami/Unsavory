using System.Collections.Generic;
using UnityEngine;

// Shows or hides the gameplay area (trays, plate, nose and hands, rocket sauce, order queue) by switching its renderers,
// so the boss dialogue can play against the whole kitchen with only the empty prep table in view. Objects stay active,
// so their Awake/Start/coroutines are unaffected. CameraDirector calls this when it moves to the Dialogue or Gameplay shot.
// Hiding is instant; showing fades the sprites up (nothing scales or moves).
public static class KitchenStage {
	const string Root = "Game Elements";
	// Paths under Root. The Table itself (the empty prep table) stays visible; only what sits on it is hidden.
	static readonly string[] GameplayParts = { "Table/Trays", "Player", "Rocket Sauce", "Order Queue" };

	public const float RevealFade = 0.5f;

	static readonly List<Renderer> _hidden = new List<Renderer>();
	static readonly Dictionary<Renderer, float> _alpha = new Dictionary<Renderer, float>();

	static Transform _table;
	static Vector3 _tableRest;

	// Shifts the empty prep table by `offset` from where the scene placed it (zero = back to rest), over `duration`
	// seconds (0 = at once). Its children, the trays, go with it.
	public static void SetTableOffset(Vector2 offset, float duration) {
		var root = GameObject.Find(Root);
		var table = root != null ? root.transform.Find("Table") : null;
		if (table == null) return;
		if (table != _table) { _table = table; _tableRest = table.localPosition; }

		var to = _tableRest + new Vector3(offset.x, offset.y, 0f);
		if (duration <= 0f) {
			Tween.Cancel(table, "pos");
			table.localPosition = to;
		} else {
			Tween.MoveLocal(table, to, duration, Tween.Ease.InOutCubic);
		}
	}

	// Returns a tween to wait on while the gameplay area fades in (null when hiding, or nothing was hidden).
	public static Tween SetGameplayVisible(bool visible) {
		if (visible) {
			Tween fade = null;
			foreach (var r in _hidden) {
				if (r == null) continue;
				var sprite = r as SpriteRenderer;
				float to;
				if (sprite != null && _alpha.TryGetValue(r, out to)) {
					var c = sprite.color; c.a = 0f; sprite.color = c;
					r.enabled = true;
					fade = Tween.Alpha(sprite, to, RevealFade, Tween.Ease.Linear);
				} else {
					r.enabled = true;
				}
			}
			_hidden.Clear();
			_alpha.Clear();
			return fade;
		}

		var root = GameObject.Find(Root);
		if (root == null) return null;
		foreach (var path in GameplayParts) {
			var part = root.transform.Find(path);
			if (part == null) continue;
			foreach (var r in part.GetComponentsInChildren<Renderer>(true)) {
				if (!r.enabled) continue;
				var sprite = r as SpriteRenderer;
				if (sprite != null) {
					Tween.Cancel(sprite, "alpha");
					_alpha[r] = sprite.color.a;
				}
				r.enabled = false;
				_hidden.Add(r);
			}
		}
		return null;
	}
}
