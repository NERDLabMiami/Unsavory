using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// An order card. The slip is built at runtime from `ingredients`: one row per ingredient (icon circle) that the
// plate checks off as the player builds the order, or X's out when a wrong ingredient is added.
public class RecipeScript : MonoBehaviour {
	public List<GameObject> ingredients = new List<GameObject>();
	public GameObject completed;
	[Tooltip("Blank slip drawn behind the rows. Hangs from the card origin (top-centre) regardless of the sprite's pivot.")]
	public Sprite slipBackground;
	[Tooltip("Uniform scale of the slip; keeps its aspect ratio.")]
	[SerializeField] float slipScale = 0.2602041f;

	// Layout, in the card's local space.
	const float RowPitch = 0.95f;
	const float IconSize = 0.8f;
	const float TopMargin = 0.85f;   // slip top to the first row's centre (leaves room for the hole)

	static readonly Color CircleColor = new Color(0.33f, 0.76f, 0.58f, 1f);
	static readonly Color Good = new Color(0.25f, 0.7f, 0.25f, 1f);
	static readonly Color Bad = new Color(0.85f, 0.2f, 0.2f, 1f);

	class Row {
		public GameObject ingredient;
		public Transform root;
		public SpriteRenderer circle, mark;
	}

	readonly List<Row> rows = new List<Row>();
	SpriteRenderer slip;
	Transform rowsRoot;

	void Awake() {
		BuildVisual();
	}

	// Tortilla and hot sauce are placed automatically, so they get no row.
	public static bool HasRow(GameObject ingredient) {
		return ingredient != null && ingredient.name != "Tortilla" && ingredient.name != "Rocket Sauce";
	}

	void BuildVisual() {
		var slipT = transform.Find("Sprite");
		if (slipT == null) {
			slipT = new GameObject("Sprite").transform;
			slipT.SetParent(transform, false);
			slipT.gameObject.AddComponent<SpriteRenderer>();
		}
		slip = slipT.GetComponent<SpriteRenderer>();
		if (slipBackground != null) slip.sprite = slipBackground;

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
			var icon = NewSprite("Icon", row.root, iconSprite, 2, Color.white);
			var size = iconSprite.bounds.size;
			float s = (IconSize * 0.8f) / Mathf.Max(size.x, size.y, 0.0001f);
			icon.transform.localScale = Vector3.one * s;
			// Sprite bounds are pivot-relative; offset so the artwork, not the pivot, sits in the circle's centre.
			icon.transform.localPosition = -iconSprite.bounds.center * s;
		}

		rows.Add(row);
		if (wrong) SetMark(row, false);
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
			var b = slip.sprite.bounds;
			slip.transform.localScale = new Vector3(slipScale, slipScale, 1f);
			slip.transform.localPosition = new Vector3(-b.center.x, -b.max.y, 0f) * slipScale;
		}
	}

	Row Find(GameObject ingredient) {
		foreach (var r in rows) if (r.ingredient == ingredient) return r;
		return null;
	}

	// The player added an ingredient this order needs.
	public void MarkCorrect(GameObject ingredient) {
		var row = Find(ingredient);
		if (row != null) SetMark(row, true);
	}

	// The player added an ingredient this order doesn't need: show it on the slip with an X.
	public void MarkWrong(GameObject ingredient) {
		if (ingredient == null) return;
		var row = Find(ingredient);
		if (row == null) {
			row = AddRow(ingredient, true);
			Layout();
		} else {
			SetMark(row, false);
		}
	}

	void SetMark(Row row, bool correct) {
		if (row.mark == null) row.mark = NewSprite("Mark", row.root, null, 4, Color.white);
		row.mark.sprite = correct ? RecipeMarks.Check() : RecipeMarks.Cross();
		row.mark.color = correct ? Good : Bad;
		row.mark.transform.localPosition = Vector3.zero;
		float rest = IconSize / RecipeMarks.CircleUnits * 0.9f;
		row.mark.transform.localScale = Vector3.one * (rest * 0.4f);
		Tween.Scale(row.mark.transform, Vector3.one * rest, 0.18f, Tween.Ease.OutBack);
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
	static Sprite _circle, _check, _cross;

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

	public static Sprite Check() {
		if (_check != null) return _check;
		var tex = Blank();
		Line(tex, 12, 34, 26, 16);
		Line(tex, 26, 16, 54, 52);
		return _check = Finish(tex);
	}

	public static Sprite Cross() {
		if (_cross != null) return _cross;
		var tex = Blank();
		Line(tex, 12, 12, 52, 52);
		Line(tex, 12, 52, 52, 12);
		return _cross = Finish(tex);
	}

	static void Line(Texture2D tex, int x0, int y0, int x1, int y1) {
		int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0)) * 2;
		for (int i = 0; i <= steps; i++) {
			float f = i / (float)steps;
			int cx = Mathf.RoundToInt(Mathf.Lerp(x0, x1, f)), cy = Mathf.RoundToInt(Mathf.Lerp(y0, y1, f));
			for (int dx = -2; dx <= 2; dx++) for (int dy = -2; dy <= 2; dy++) {
				int px = cx + dx, py = cy + dy;
				if (px >= 0 && px < Size && py >= 0 && py < Size) tex.SetPixel(px, py, Color.white);
			}
		}
	}
}
