using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Run on the open Game scene, in this order:
//  1. "Kitchen Background -> World Sprite": the Kitchen Image on the Screen Space - Camera canvas is glued to the screen
//     (cropped in a small Game view, never zooms with the camera). This adds a world-space SpriteRenderer instead, sized to
//     the PSD artboard (1 px = 0.0091762 units) and aligned so the PSD prep table lands on the scene's actual Table.
//  2. "Fit Camera Shots": writes the Kitchen (whole kitchen) and Gameplay (table, trays, player) shots into the
//     CameraDirector from the bounds of the objects as they are now, instead of hard-coded numbers.
// Both have a preview that only logs. Apply is undoable and marks the scene dirty (it does not save it).
public static class FitCameraAndBackground {
	const float Px = 2f * 4.955149f / 1080f;      // world units per PSD pixel
	const float Aspect = 16f / 9f;
	static readonly Vector2 PsdTableCenterPx = new Vector2(1149.5f, 743.5f);   // prep-table layer centre on the 1920x1080 artboard
	const string BackgroundName = "Kitchen Background";

	[MenuItem("Tools/PSD Layout/Kitchen Background -> World Sprite (preview)")] static void BgPreview() { Background(false); }
	[MenuItem("Tools/PSD Layout/Kitchen Background -> World Sprite (apply)")] static void BgApply() { Background(true); }
	[MenuItem("Tools/PSD Layout/Fit Camera Shots (preview)")] static void ShotsPreview() { Shots(false); }
	[MenuItem("Tools/PSD Layout/Fit Camera Shots (apply)")] static void ShotsApply() { Shots(true); }

	// ---- background ----

	static void Background(bool apply) {
		var log = new StringBuilder(apply ? "[PsdLayout] Kitchen background\n" : "[PsdLayout] Kitchen background (preview)\n");
		var root = Find("Game Elements");
		var table = Find("Game Elements/Table");
		var oldKitchen = Find("Game Elements/Canvas/Kitchen");
		if (root == null || table == null) { Debug.LogError("[PsdLayout] Need 'Game Elements' and 'Game Elements/Table' in the open scene."); return; }

		Sprite sprite = null;
		if (oldKitchen != null && oldKitchen.TryGetComponent<Image>(out var img)) sprite = img.sprite;
		var existing = root.transform.Find(BackgroundName);
		if (sprite == null && existing != null && existing.TryGetComponent<SpriteRenderer>(out var er)) sprite = er.sprite;
		if (sprite == null) sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Redesign/Kitchen.png");
		if (sprite == null) { Debug.LogError("[PsdLayout] No Kitchen sprite found."); return; }

		var tableBounds = OwnBounds(table.transform);   // the table art only, not the trays on it
		if (tableBounds.size == Vector3.zero) { Debug.LogError("[PsdLayout] Table has no renderer/rect to measure."); return; }
		var tc = tableBounds.center;
		// Artboard centre = table centre minus the table's PSD offset from the artboard centre (y flips: PSD y is down).
		var center = new Vector3(tc.x - (PsdTableCenterPx.x - 960f) * Px, tc.y + (PsdTableCenterPx.y - 540f) * Px, 0f);
		float expectedTableW = 1019f * Px;
		log.AppendLine("  Table world bounds centre " + (Vector2)tc + " size " + (Vector2)tableBounds.size + " (PSD expects width " + expectedTableW.ToString("F2") + ")");
		log.AppendLine("  Background centre -> " + (Vector2)center + ", size " + (1920f * Px).ToString("F2") + " x " + (1080f * Px).ToString("F2") + " units");
		if (existing != null) log.AppendLine("  (updating existing '" + BackgroundName + "')");
		if (oldKitchen != null) log.AppendLine("  old Canvas/Kitchen Image will be deactivated");
		if (!apply) { Debug.Log(log.ToString()); return; }

		GameObject go;
		if (existing != null) go = existing.gameObject;
		else {
			go = new GameObject(BackgroundName);
			Undo.RegisterCreatedObjectUndo(go, "Kitchen background");
			go.transform.SetParent(root.transform, false);
		}
		var sr = go.GetComponent<SpriteRenderer>();
		if (sr == null) sr = Undo.AddComponent<SpriteRenderer>(go);
		Undo.RecordObject(sr, "Kitchen background"); Undo.RecordObject(go.transform, "Kitchen background");
		sr.sprite = sprite;

		// Behind everything in the room: same sorting layer as the table, one order below the lowest sprite under Game Elements.
		var tableSr = table.GetComponent<SpriteRenderer>();
		if (tableSr != null) sr.sortingLayerID = tableSr.sortingLayerID;
		int lowest = 0;
		foreach (var r in root.GetComponentsInChildren<SpriteRenderer>(true))
			if (r != sr && r.sortingLayerID == sr.sortingLayerID) lowest = Mathf.Min(lowest, r.sortingOrder);
		sr.sortingOrder = lowest - 1;

		go.transform.position = new Vector3(center.x, center.y, tableBounds.center.z + 1f);
		var parentScale = go.transform.parent != null ? go.transform.parent.lossyScale : Vector3.one;
		go.transform.localScale = new Vector3(Px * 100f / parentScale.x, Px * 100f / parentScale.y, 1f);
		go.transform.rotation = Quaternion.identity;

		if (oldKitchen != null && oldKitchen.activeSelf) {
			Undo.RecordObject(oldKitchen, "Kitchen background");
			oldKitchen.SetActive(false);
		}
		EditorUtility.SetDirty(go);
		MarkDirty();
		Debug.Log(log.ToString());
	}

	// ---- shots ----

	static void Shots(bool apply) {
		var log = new StringBuilder(apply ? "[PsdLayout] Fit camera shots\n" : "[PsdLayout] Fit camera shots (preview)\n");
		var cam = Camera.main;
		if (cam == null) { Debug.LogError("[PsdLayout] No Main Camera."); return; }

		var bgT = Find("Game Elements/" + BackgroundName);
		if (bgT == null) { Debug.LogError("[PsdLayout] Run 'Kitchen Background -> World Sprite' first."); return; }
		var bg = WorldBounds(bgT.transform);

		// Whole kitchen.
		float kitchenOrtho = Mathf.Max(bg.extents.y, bg.extents.x / Aspect);
		var kitchenPos = (Vector2)bg.center;

		// Gameplay area = table, what sits on it, the player and the order queue anchor. Rocket Sauce sits well to the right of
		// the table and is left out so it does not zoom the shot out.
		var parts = new[] { "Game Elements/Table", "Game Elements/Table/Trays", "Game Elements/Player" };
		Bounds? area = null;
		foreach (var p in parts) {
			var t = Find(p);
			if (t == null) { log.AppendLine("  (missing " + p + ")"); continue; }
			var b = VisibleBounds(t.transform);
			if (b.size == Vector3.zero) continue;
			log.AppendLine("  part " + p + " -> " + (Vector2)b.min + " .. " + (Vector2)b.max);
			if (area == null) area = b; else { var a = area.Value; a.Encapsulate(b); area = a; }
		}
		var queue = Find("Game Elements/Order Queue");
		if (queue != null && area != null) { var a = area.Value; a.Encapsulate(queue.transform.position); area = a; }
		if (area == null) { Debug.LogError("[PsdLayout] Found nothing to frame for the Gameplay shot."); return; }

		var ab = area.Value;
		float gameplayOrtho = Mathf.Max(ab.extents.y, ab.extents.x / Aspect) * 1.02f;
		gameplayOrtho = Mathf.Clamp(gameplayOrtho, 1.5f, kitchenOrtho);
		var gp = (Vector2)ab.center;
		// Keep the view inside the background where it fits.
		float halfH = gameplayOrtho, halfW = gameplayOrtho * Aspect;
		if (halfW * 2f <= bg.size.x) gp.x = Mathf.Clamp(gp.x, bg.min.x + halfW, bg.max.x - halfW);
		if (halfH * 2f <= bg.size.y) gp.y = Mathf.Clamp(gp.y, bg.min.y + halfH, bg.max.y - halfH);

		log.AppendLine("  Kitchen  -> pos " + kitchenPos + " ortho " + kitchenOrtho.ToString("F3"));
		log.AppendLine("  Gameplay -> pos " + gp + " ortho " + gameplayOrtho.ToString("F3") + "  (area " + (Vector2)ab.min + " .. " + (Vector2)ab.max + ")");
		if (!apply) { Debug.Log(log.ToString()); return; }

		var dir = cam.GetComponent<CameraDirector>();
		if (dir == null) dir = Undo.AddComponent<CameraDirector>(cam.gameObject);
		Undo.RecordObject(dir, "Fit camera shots");
		SetShot(dir, CameraDirector.Kitchen, kitchenPos, kitchenOrtho);
		SetShot(dir, CameraDirector.Gameplay, gp, gameplayOrtho);
		EditorUtility.SetDirty(dir);

		Undo.RecordObject(cam, "Fit camera shots"); Undo.RecordObject(cam.transform, "Fit camera shots");
		cam.transform.position = new Vector3(gp.x, gp.y, cam.transform.position.z);
		cam.orthographicSize = gameplayOrtho;
		MarkDirty();
		Debug.Log(log.ToString());
	}

	static void SetShot(CameraDirector dir, string name, Vector2 pos, float ortho) {
		CameraDirector.Shot s = null;
		foreach (var x in dir.shots) if (x.name == name) { s = x; break; }
		if (s == null) { s = new CameraDirector.Shot { name = name }; dir.shots.Add(s); }
		s.position = pos; s.orthoSize = ortho;
	}

	// ---- helpers ----

	// Bounds of just this object's own sprite or rect (children ignored).
	static Bounds OwnBounds(Transform t) {
		var sr = t.GetComponent<SpriteRenderer>();
		if (sr != null && sr.sprite != null) return sr.bounds;
		var rt = t as RectTransform;
		if (rt == null) return new Bounds(t.position, Vector3.zero);
		var c = new Vector3[4];
		rt.GetWorldCorners(c);
		var b = new Bounds(c[0], Vector3.zero);
		for (int i = 1; i < 4; i++) b.Encapsulate(c[i]);
		return b;
	}

	// Bounds of the sprites that are actually drawn under t: active, enabled, non-transparent. Unlike WorldBounds this
	// ignores inactive objects and UI rect corners, which would otherwise make the Gameplay shot too wide.
	static Bounds VisibleBounds(Transform t) {
		bool any = false;
		var b = new Bounds();
		foreach (var sr in t.GetComponentsInChildren<SpriteRenderer>(false)) {
			if (!sr.enabled || sr.sprite == null || sr.color.a <= 0.01f) continue;
			if (!any) { b = sr.bounds; any = true; } else b.Encapsulate(sr.bounds);
		}
		return any ? b : new Bounds(t.position, Vector3.zero);
	}

	// Union of every renderer's bounds under t, plus RectTransform corners (UI-style objects such as the Table).
	static Bounds WorldBounds(Transform t) {
		bool any = false;
		var b = new Bounds();
		void Add(Bounds x) { if (!any) { b = x; any = true; } else b.Encapsulate(x); }
		foreach (var r in t.GetComponentsInChildren<Renderer>(true)) {
			if (r is SpriteRenderer sr && sr.sprite == null) continue;
			Add(r.bounds);
		}
		if (!any) {
			foreach (var rt in t.GetComponentsInChildren<RectTransform>(true)) {
				var c = new Vector3[4];
				rt.GetWorldCorners(c);
				var rb = new Bounds(c[0], Vector3.zero);
				for (int i = 1; i < 4; i++) rb.Encapsulate(c[i]);
				if (rb.size.sqrMagnitude > 0f) Add(rb);
			}
		}
		return any ? b : new Bounds(t.position, Vector3.zero);
	}

	static GameObject Find(string path) {
		var parts = path.Split('/');
		Transform cur = null;
		foreach (var r in SceneManager.GetActiveScene().GetRootGameObjects())
			if (r.name == parts[0]) { cur = r.transform; break; }
		for (int i = 1; cur != null && i < parts.Length; i++) cur = cur.Find(parts[i]);
		return cur != null ? cur.gameObject : null;
	}

	static void MarkDirty() { UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene()); }
}
