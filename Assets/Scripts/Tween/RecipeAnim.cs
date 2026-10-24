using UnityEngine;

// Order card feedback: time-running-out wobble, the pop-out when served, and the shake and drop when thrown away
// (was the Recipe controller's Order Time Running Out / Unmatched Order clips). Entrances are not animated here:
// new cards slide in from the right through OrderQueueLayout2D.
public class RecipeAnim : MonoBehaviour {
	SpriteRenderer _sprite;

	void Awake() {
		AnimUtil.DisableAnimator(gameObject);
		var t = transform.Find("Sprite");
		_sprite = t != null ? t.GetComponent<SpriteRenderer>() : null;
	}

	// Loops until the card is served, discarded or destroyed.
	public void Alert() {
		Tween.Run(this, "alert", 1f / 3f, v => {
			transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(v * Mathf.PI * 4f));
			if (_sprite != null) {
				var c = _sprite.color; c.a = Mathf.Lerp(1f, 0.5f, 1f - Mathf.Abs(2f * v - 1f)); _sprite.color = c;
			}
		}, Tween.Ease.Linear).Loop(0);
	}

	void StopAlert() {
		Tween.Cancel(this, "alert");
		transform.localRotation = Quaternion.identity;
		if (_sprite != null) { var c = _sprite.color; c.a = 1f; _sprite.color = c; }
	}

	// Total time the cash sequence needs before the card can be destroyed.
	public const float CashDuration = 1.5f;
	const float StackTime = 0.1f, CrumpleTime = 0.1f, DriftTime = 0.4f;
	const int TrailBurst = 8;
	const float TrailInterval = 0.03f, TrailSize = 0.2f, TrailMinLife = 0.6f, TrailMaxLife = 0.9f;

	// Served: the slip is swapped for cash (stack, crumple, explode) that then drifts to the left edge while fading.
	public void CashOut(Sprite stack, Sprite crumple, Sprite explode, Sprite single, Vector3 leftTarget) {
		StopAlert();
		float width = 1f;
		if (_sprite != null) width = _sprite.bounds.size.x;
		foreach (var s in GetComponentsInChildren<SpriteRenderer>(true)) {
			Tween.Cancel(s, "alpha");
			Tween.Alpha(s, 0f, 0.05f, Tween.Ease.Linear);
		}

		var go = new GameObject("Cash");
		go.transform.SetParent(transform, false);
		go.transform.position = _sprite != null ? _sprite.bounds.center : transform.position;
		var cash = go.AddComponent<SpriteRenderer>();
		cash.sortingLayerID = _sprite != null ? _sprite.sortingLayerID : 0;
		cash.sortingOrder = _sprite != null ? _sprite.sortingOrder + 50 : 50;

		// Cash art is 12+ units wide at import size, so size each frame relative to the slip's width.
		Vector3 lossy = transform.lossyScale;
		System.Func<Sprite, float, Vector3> fit = (sp, mult) => {
			float k = sp != null ? width * mult / sp.bounds.size.x : 1f;
			return new Vector3(k / Mathf.Max(lossy.x, 1e-4f), k / Mathf.Max(lossy.y, 1e-4f), 1f);
		};

		ShowFrame(cash, stack, fit(stack, 1.2f), 0.7f);
		Tween.Delay(StackTime, () => { if (cash != null) ShowFrame(cash, crumple, fit(crumple, 1.1f), 0.8f); });
		Tween.Delay(StackTime + CrumpleTime, () => {
			if (cash == null) return;
			ShowFrame(cash, explode, fit(explode, 1.8f), 0.6f);
			Tween.Move(transform, leftTarget, DriftTime, Tween.Ease.InQuad);
			Tween.Alpha(cash, 0f, DriftTime * 0.7f, Tween.Ease.Linear, DriftTime * 0.3f);
			EmitCashTrail(single, cash, width);
		});
	}

	// A burst of bills at the explosion, then a steady stream that follows the card as it drifts left.
	void EmitCashTrail(Sprite single, SpriteRenderer cash, float width) {
		if (single == null) return;
		for (int i = 0; i < TrailBurst; i++) SpawnBill(single, cash, width);
		float next = TrailInterval;
		Tween.Run(this, "cashTrail", DriftTime, v => {
			float t = v * DriftTime;
			while (t >= next) { next += TrailInterval; if (cash != null) SpawnBill(single, cash, width); }
		}, Tween.Ease.Linear);
	}

	// One bill left behind in world space (not parented, so it stays put while the card moves on): tumbles, falls, shrinks and fades.
	static void SpawnBill(Sprite single, SpriteRenderer cash, float width) {
		var go = new GameObject("CashBit");
		go.transform.position = cash.transform.position + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0f);
		go.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
		float k = width * TrailSize / single.bounds.size.x;
		go.transform.localScale = new Vector3(k, k, 1f);
		var sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = single;
		sr.sortingLayerID = cash.sortingLayerID;
		sr.sortingOrder = cash.sortingOrder - 1;

		float life = Random.Range(TrailMinLife, TrailMaxLife);
		Tween.Move(go.transform, go.transform.position + new Vector3(Random.Range(-0.4f, 0.4f), Random.Range(-1.6f, -0.8f), 0f), life, Tween.Ease.InQuad);
		Tween.Rotate(go.transform, new Vector3(0f, 0f, Random.Range(-360f, 360f)), life, Tween.Ease.Linear);
		Tween.Scale(go.transform, go.transform.localScale * 0.6f, life, Tween.Ease.Linear);
		Tween.Alpha(sr, 0f, life * 0.5f, Tween.Ease.Linear, life * 0.5f, () => { if (go != null) Object.Destroy(go); });
	}

	// Swaps the cash sprite and pops it in from a fraction of its size. A missing sprite skips the frame.
	static void ShowFrame(SpriteRenderer cash, Sprite sp, Vector3 scale, float from) {
		if (sp == null) return;
		cash.sprite = sp;
		var c = cash.color; c.a = 1f; cash.color = c;
		cash.transform.localScale = scale * from;
		Tween.Scale(cash.transform, scale, 0.15f, Tween.Ease.OutBack);
	}

	// Thrown away (wrong ingredient or out of time): a quick shake, then it drops and fades with everything on it.
	public void Unmatched() {
		StopAlert();
		Tween.Run(this, "shake", 0.1f, v => transform.localRotation = Quaternion.Euler(0f, 0f, 3f * Mathf.Sin(v * Mathf.PI * 4f) * (1f - v)),
			Tween.Ease.Linear, 0f, () => { if (this != null) transform.localRotation = Quaternion.identity; });
		Tween.Move(transform, transform.position + new Vector3(0f, -0.6f, 0f), 0.3f, Tween.Ease.InQuad, 0.1f);
		FadeAll(0f, 0.3f, 0.1f);
	}

	// Fades every sprite on the card, so the ingredient icons go with the card instead of being left behind.
	void FadeAll(float to, float time, float delay) {
		foreach (var s in GetComponentsInChildren<SpriteRenderer>(true)) {
			Tween.Cancel(s, "alpha");
			Tween.Alpha(s, to, time, Tween.Ease.Linear, delay);
		}
	}
}
