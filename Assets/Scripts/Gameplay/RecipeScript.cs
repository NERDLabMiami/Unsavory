using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// An order card. The slip is built at runtime from `ingredients`: one row per ingredient (icon circle) that the
// plate grays out as the player builds the order, or briefly shows in red when a wrong ingredient is added.
public class RecipeScript : MonoBehaviour {
	[Tooltip("What this card shows. Set by Init at spawn; the fields below are only a fallback for cards placed directly in a scene.")]
	public RecipeData data;
	public List<GameObject> ingredients = new List<GameObject>();
	public GameObject completed;
	[Tooltip("Blank slip drawn behind the rows. Hangs from the card origin (top-centre) regardless of the sprite's pivot.")]
	public Sprite slipBackground;
	[Tooltip("Scale of the slip. Its 9-slice borders keep their proportions at this scale; the paper's height grows with the row count.")]
	[SerializeField] float slipScale = 0.2602041f;

	// Layout, in the card's local space.
	const float RowPitch = 0.95f;
	const float IconSize = 0.8f;
	const float TopMargin = 0.85f;   // slip top to the first row's centre (leaves room for the hole)
	const float BottomMargin = 0.7f; // last row's centre to the slip bottom
	const int MinRows = 2;           // short orders still get a slip that looks like paper

	static readonly Color CircleColor = new Color(0.33f, 0.76f, 0.58f, 1f);
	static readonly Color RocketSlip = new Color(1f, 0.6f, 0.55f, 1f);
	static readonly Color Bad =new Color(0.85f, 0.2f, 0.2f, 1f);
	static readonly Color DoneIcon = new Color(0.5f, 0.5f, 0.5f, 0.35f);
	static readonly Color DoneCircle = new Color(CircleColor.r, CircleColor.g, CircleColor.b, 0.35f);

	class Row {
		public GameObject ingredient;
		public Transform root;
		public SpriteRenderer circle, icon;
		public bool temporary;
	}

	readonly List<Row> rows = new List<Row>();
	SpriteRenderer slip;
	Transform rowsRoot;

	bool built;

	// Cards spawned from code call Init(data) right after Instantiate; a card already in a scene builds itself here.
	void Awake() {
		if (!built && (data != null || ingredients.Count > 0)) BuildVisual();
	}

	public void Init(RecipeData d) {
		data = d;
		if (built) ClearVisual();
		BuildVisual();
	}

	void ClearVisual() {
		if (rowsRoot != null) Destroy(rowsRoot.gameObject);
		rows.Clear();
		built = false;
	}

	// The tortilla is placed automatically, so it gets no row. Rocket sauce gets none either: the slip's red tint says it is needed.
	public static bool HasRow(GameObject ingredient) {
		return ingredient != null && ingredient.name != "Tortilla" && ingredient.name != "Rocket Sauce";
	}

	bool IsRocket() {
		if (data != null && data.rocket) return true;
		foreach (var ing in ingredients) if (ing != null && ing.name == "Rocket Sauce") return true;
		return false;
	}

	void BuildVisual() {
		if (data != null) {
			ingredients = new List<GameObject>(data.ingredients);
			completed = data.completed;
		}
		built = true;
		var slipT = transform.Find("Sprite");
		if (slipT == null) {
			slipT = new GameObject("Sprite").transform;
			slipT.SetParent(transform, false);
			slipT.gameObject.AddComponent<SpriteRenderer>();
		}
		slip = slipT.GetComponent<SpriteRenderer>();
		if (slipBackground != null) slip.sprite = slipBackground;
		// Orders that need rocket sauce are printed on red paper.
		slip.color = IsRocket() ? RocketSlip : Color.white;
		// Sliced draw mode stretches only the middle, so the hole and the torn bottom keep their shape at any height.
		slip.drawMode = SpriteDrawMode.Sliced;
		slip.tileMode = SpriteTileMode.Continuous;

		rowsRoot = new GameObject("Rows").transform;
		rowsRoot.SetParent(transform, false);

		foreach (var ing in ingredients) {
			if (HasRow(ing)) AddRow(ing, false);
		}
		Layout();
	}

	Row AddRow(GameObject ingredient, bool wrong) {
		var row = new Row { ingredient = ingredient };
		row.root = new GameObject("Row " + ingredient.name).transform;
		row.root.SetParent(rowsRoot, false);

		Sprite iconSprite = null;
		var src = ingredient.GetComponentInChildren<SpriteRenderer>();
		if (src != null) iconSprite = src.sprite;

		row.circle = NewSprite("Circle", row.root, RecipeMarks.Circle(), 1, CircleColor);
		row.circle.transform.localScale = Vector3.one * (IconSize / RecipeMarks.CircleUnits);

		if (iconSprite != null) {
			var icon = row.icon = NewSprite("Icon", row.root, iconSprite, 2, Color.white);
			var size = iconSprite.bounds.size;
			float s = (IconSize * 0.8f) / Mathf.Max(size.x, size.y, 0.0001f);
			icon.transform.localScale = Vector3.one * s;
			// Sprite bounds are pivot-relative; offset so the artwork, not the pivot, sits in the circle's centre.
			icon.transform.localPosition = -iconSprite.bounds.center * s;
		}

		rows.Add(row);
		if (wrong) {
			row.temporary = true;
			Tint(row, Bad, Bad);
			row.root.localScale = Vector3.one * 0.6f;
			Tween.Scale(row.root, Vector3.one, 0.18f, Tween.Ease.OutBack);
		}
		return row;
	}

	SpriteRenderer NewSprite(string name, Transform parent, Sprite sprite, int order, Color color) {
		var go = new GameObject(name);
		go.transform.SetParent(parent, false);
		var sr = go.AddComponent<SpriteRenderer>();
		sr.sprite = sprite;
		sr.color = color;
		sr.sortingLayerID = slip.sortingLayerID;
		sr.sortingOrder = slip.sortingOrder + order;
		return sr;
	}

	// Hangs the slip from the card origin (its top-centre, whatever the sprite's pivot) and stacks the rows below it.
	void Layout() {
		for (int i = 0; i < rows.Count; i++) {
			rows[i].root.localPosition = new Vector3(0f, -TopMargin - i * RowPitch, 0f);
		}
		if (slip != null && slip.sprite != null) {
			var sprite = slip.sprite;
			float height = TopMargin + (Mathf.Max(rows.Count, MinRows) - 1) * RowPitch + BottomMargin;
			var size = new Vector2(sprite.rect.width / sprite.pixelsPerUnit, height / slipScale);
			slip.size = size;
			slip.transform.localScale = new Vector3(slipScale, slipScale, 1f);
			// Place the slip's top-centre at the card origin, whatever the sprite's pivot.
			var pivot = new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height);
			var topCentre = new Vector2((0.5f - pivot.x) * size.x, (1f - pivot.y) * size.y);
			slip.transform.localPosition = new Vector3(-topCentre.x, -topCentre.y, 0f) * slipScale;
		}
	}

	Row Find(GameObject ingredient) {
		foreach (var r in rows) if (r.ingredient == ingredient) return r;
		return null;
	}

	// The player added an ingredient this order needs: gray the row out.
	public void MarkCorrect(GameObject ingredient) {
		var row = Find(ingredient);
		if (row == null) return;
		Tween.Tint(row.circle, DoneCircle, 0.18f);
		if (row.icon != null) Tween.Tint(row.icon, DoneIcon, 0.18f);
	}

	// The player added an ingredient this order doesn't need: show it on the slip in red, briefly.
	public void MarkWrong(GameObject ingredient) {
		if (ingredient == null) return;
		var row = Find(ingredient);
		if (row == null) {
			AddRow(ingredient, true);
			Layout();
			Tween.Delay(IngredientFeedback.WrongHold, RemoveTemporaryRows);
		} else {
			Tint(row, Bad, Bad);
		}
	}

	static void Tint(Row row, Color circle, Color icon) {
		row.circle.color = circle;
		if (row.icon != null) row.icon.color = icon;
	}

	void RemoveTemporaryRows() {
		if (this == null) return;
		for (int i = rows.Count - 1; i >= 0; i--) {
			if (!rows[i].temporary) continue;
			Destroy(rows[i].root.gameObject);
			rows.RemoveAt(i);
		}
		Layout();
	}

	void setAlert() {
		gameObject.GetComponent<AudioSource>().Play();
	}

}

// Procedurally drawn sprites for the order slip, so the card needs no art beyond the blank slip.
static class RecipeMarks {
	const int Size = 64;
	public const float PPU = 64f;
	public const float CircleUnits = Size / PPU;
	static Sprite _circle;

	static Texture2D Blank() {
		var tex = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
		var clear = new Color(1f, 1f, 1f, 0f);
		var px = new Color[Size * Size];
		for (int i = 0; i < px.Length; i++) px[i] = clear;
		tex.SetPixels(px);
		return tex;
	}

	static Sprite Finish(Texture2D tex) {
		tex.Apply();
		return Sprite.Create(tex, new Rect(0, 0, Size, Size), new Vector2(0.5f, 0.5f), PPU);
	}

	// Filled disc with a dark ring, antialiased on the edge.
	public static Sprite Circle() {
		if (_circle != null) return _circle;
		var tex = Blank();
		float c = (Size - 1) * 0.5f, r = Size * 0.5f - 1f;
		for (int y = 0; y < Size; y++) for (int x = 0; x < Size; x++) {
			float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
			float a = Mathf.Clamp01(r - d + 0.5f);
			float ring = Mathf.Clamp01((d - (r - 4f)) + 0.5f);
			var col = Color.Lerp(Color.white, new Color(0.35f, 0.45f, 0.45f, 1f), ring);
			tex.SetPixel(x, y, new Color(col.r, col.g, col.b, a));
		}
		return _circle = Finish(tex);
	}
}
