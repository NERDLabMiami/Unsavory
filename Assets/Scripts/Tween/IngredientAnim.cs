using System.Collections.Generic;
using UnityEngine;

// Ingredient on the plate: fades in when placed, fades out when its order is thrown away.
// Replaces the per-ingredient Animator controllers (Place X / Bad X Match clips). Values are copied from those clips
// and keyed by the controller name the prefab still carries.
public class IngredientAnim : MonoBehaviour {
	struct BadData {
		public Vector3 offset; public float duration;
		public BadData(float x, float y, float duration) { offset = new Vector3(x, y, 0f); this.duration = duration; }
	}

	const float DefaultPlaceDuration = 1f / 6f;
	static readonly BadData DefaultBad = new BadData(0f, -0.19f, 0.25f);

	// How long each ingredient takes to fade in once it is on the plate (from the old Place X clips).
	static readonly Dictionary<string, float> PlaceDuration = new Dictionary<string, float> {
		{ "Rocket Sauce", 0.5f },
	};

	static readonly Dictionary<string, BadData> Bad = new Dictionary<string, BadData> {
		{ "Beans", new BadData(0f, -0.19f, 0.25f) },
		{ "Cheese", new BadData(0f, -0.18f, 0.25f) },
		{ "Chicken", new BadData(-0.289f, 0.13f, 0.25f) },
		{ "Rice", new BadData(0f, 0.11f, 0.25f) },
		{ "Sour Cream", new BadData(-0.3f, -0.19f, 0.25f) },
		{ "Tortilla", new BadData(0f, -0.06f, 0.25f) },
		{ "Lettuce", new BadData(0f, -0.19f, 1f / 6f) },
		{ "Rocket Sauce", new BadData(0f, -0.33f, 1f / 12f) },
	};

	string _kind = "";
	SpriteRenderer _sprite;

	void Awake() {
		var anim = GetComponent<Animator>();
		if (anim != null) {
			if (anim.runtimeAnimatorController != null) _kind = anim.runtimeAnimatorController.name;
			anim.enabled = false;
		}
		_sprite = GetComponent<SpriteRenderer>();
	}

	// Call right after the ingredient has been parented to the plate (the clips animated local position).
	public static IngredientAnim PlaceIn(GameObject ingredient) {
		var a = AnimUtil.Get<IngredientAnim>(ingredient);
		a.StartPlace();
		return a;
	}

	// The ingredient appears in place on the plate: it only fades in (no slide or scale).
	void StartPlace() {
		float duration;
		if (!PlaceDuration.TryGetValue(_kind, out duration)) duration = DefaultPlaceDuration;

		// Parenting kept the prefab's world scale; relative to the plate it is one.
		transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
		if (_sprite != null) {
			SetAlpha(0f);
			Tween.Alpha(_sprite, 1f, duration, Tween.Ease.Linear);
		}
	}

	// The order was thrown away: drift and fade, then remove.
	public void Mismatch() {
		BadData d;
		if (!Bad.TryGetValue(_kind, out d)) d = DefaultBad;

		Tween.CancelAll(transform);
		Tween.MoveLocal(transform, transform.localPosition + d.offset, d.duration, Tween.Ease.Linear);
		if (_sprite != null) Tween.Alpha(_sprite, 0f, d.duration, Tween.Ease.Linear, 0f, () => Destroy(gameObject));
		else Tween.Delay(d.duration, () => Destroy(gameObject));
	}

	void SetAlpha(float a) {
		var c = _sprite.color; c.a = a; _sprite.color = c;
	}
}
