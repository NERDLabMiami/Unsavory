using UnityEngine;

public class FruitBowl : MonoBehaviour {
	public GameObject[] fruit;

	void OnEnable() {
		PlayerSignals.FruitChanged += SetCount;
		SetCount(PlayerPrefs.GetInt("fruit", 5));
	}

	void OnDisable() {
		PlayerSignals.FruitChanged -= SetCount;
	}

	void Start() {
		EnsureHover();
	}

	// The bowl has no collider or interactable in the scene. Give it a hover-only one (toasts.txt hover.fruit_bowl) that fits
	// the fruit and the bowl sprites beside it, so the pointer, stick cursor and gamepad can reach it like the other objects.
	void EnsureHover() {
		if (GetComponent<HomeInteractable>() != null) return;
		bool any = false;
		Bounds bounds = new Bounds(transform.position, Vector3.zero);
		foreach (var r in GetComponentsInChildren<SpriteRenderer>(true)) {
			if (!r.sprite) continue;
			if (any) bounds.Encapsulate(r.bounds); else { bounds = r.bounds; any = true; }
		}
		if (transform.parent) {
			foreach (Transform sibling in transform.parent) {
				if (sibling == transform || sibling.name.IndexOf("Bowl", System.StringComparison.OrdinalIgnoreCase) < 0) continue;
				foreach (var r in sibling.GetComponentsInChildren<SpriteRenderer>(true)) {
					if (!r.sprite) continue;
					if (any) bounds.Encapsulate(r.bounds); else { bounds = r.bounds; any = true; }
				}
			}
		}
		if (!any) return;

		var box = GetComponent<BoxCollider2D>();
		if (!box) box = gameObject.AddComponent<BoxCollider2D>();
		box.isTrigger = true;
		Vector3 scale = transform.lossyScale;
		box.offset = transform.InverseTransformPoint(bounds.center);
		box.size = new Vector2(bounds.size.x / Mathf.Max(0.0001f, Mathf.Abs(scale.x)), bounds.size.y / Mathf.Max(0.0001f, Mathf.Abs(scale.y)));
		var hover = gameObject.AddComponent<HomeInteractable>();
		hover.action = HomeInteractable.HomeAction.FruitBowl;
	}

	/// <summary>Shows the first n pieces of fruit (0..fruit.Length).</summary>
	public void SetCount(int n) {
		for (int i = 0; i < fruit.Length; i++) {
			if (fruit[i]) fruit[i].SetActive(i < n);
		}
	}

	public void setFruit(int dayOfWeek) {
		switch(dayOfWeek) {
		case 3:
			fruit[fruit.Length-3].SetActive(false);
			goto case 2;
		case 2:
			fruit[fruit.Length-2].SetActive(false);
			goto case 1;
		case 1:
			fruit[fruit.Length-1].SetActive(false);
			break;
		case 4:
			for (int i = 0; i < fruit.Length; i++) {
				fruit[i].SetActive(false);
			}
			break;
		case 5:
			break;
		}
	}

}
