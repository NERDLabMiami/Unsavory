using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Applies the redesign PSD (exported to layout.json / layer_files.json + Assets/Sprites/Redesign/*.png) to the open
// Game scene. "Preview" only logs what would change; "Apply" does it with Undo. Only the active scene is touched, and
// only the objects listed in Map below. Overlay-canvas UI (Boss, HUD, Tutor) is intentionally not handled yet.
public static class PsdLayoutApply {
	const string Dir = "Assets/Editor/PsdLayout/";
	const string SpriteDir = "Assets/Sprites/Redesign/";

	// PSD artboard (1920x1080) -> world. The Kitchen background fills the camera at ortho 4.955149, centred on the camera.
	const float OrthoWide = 4.955149f;
	const float Px = 2f * OrthoWide / 1080f; // world units per PSD pixel
	static readonly Vector2 Center = new Vector2(9.57f, 0f);

	// Gameplay shot: fit the Gameplay Area (PSD px 640..1659 x 481..1006) with a little margin.
	const float GameplayOrtho = 2.68f;

	enum Kind { WorldSprite, WorldCanvasImage }
	struct Entry { public string file, scenePath; public Kind kind; public Entry(string f, string p, Kind k) { file = f; scenePath = p; kind = k; } }

	static readonly Entry[] Map = {
		new Entry("Kitchen",    "Game Elements/Canvas/Kitchen", Kind.WorldCanvasImage),
		new Entry("prep-table", "Game Elements/Canvas/Image",   Kind.WorldCanvasImage),
		new Entry("beans",      "Game Elements/Trays/Top/Bean Tray",       Kind.WorldSprite),
		new Entry("chicken",    "Game Elements/Trays/Top/Chicken Tray",    Kind.WorldSprite),
		new Entry("cheese",     "Game Elements/Trays/Top/Cheese Tray",     Kind.WorldSprite),
		new Entry("sour-cream", "Game Elements/Trays/Top/Sour Cream Tray", Kind.WorldSprite),
		new Entry("tomatoes",   "Game Elements/Trays/Middle/Tomato Tray",  Kind.WorldSprite),
		new Entry("lettuce",    "Game Elements/Trays/Middle/Lettuce Tray", Kind.WorldSprite),
	};

	[Serializable] class Item { public string file, layer; public int x0, y0, x1, y1; }
	[Serializable] class ItemList { public Item[] items; }

	[MenuItem("Tools/PSD Layout/Preview (no changes)")] static void Preview() { Run(false); }
	[MenuItem("Tools/PSD Layout/Apply to open scene")] static void Apply() { Run(true); }

	static void Run(bool apply) {
		var list = JsonUtility.FromJson<ItemList>(File.ReadAllText(Dir + "layer_files.json"));
		var byFile = new Dictionary<string, Item>();
		foreach (var it in list.items) byFile[it.file] = it;

		if (apply) ImportSprites(list);

		var log = new StringBuilder();
		log.AppendLine(apply ? "[PsdLayout] APPLY" : "[PsdLayout] PREVIEW (no changes)");
		Undo.IncrementCurrentGroup();
		int group = Undo.GetCurrentGroup();

		foreach (var e in Map) {
			Item it;
			if (!byFile.TryGetValue(e.file, out it)) { log.AppendLine("  MISSING layer file: " + e.file); continue; }
			var go = Find(e.scenePath);
			if (go == null) { log.AppendLine("  MISSING scene object: " + e.scenePath); continue; }

			var size = new Vector2(it.x1 - it.x0, it.y1 - it.y0);
			var centerPx = new Vector2((it.x0 + it.x1) * 0.5f, (it.y0 + it.y1) * 0.5f);
			var world = new Vector3(Center.x + (centerPx.x - 960f) * Px, -(centerPx.y - 540f) * Px, 0f);
			var sprite = apply ? AssetDatabase.LoadAssetAtPath<Sprite>(SpriteDir + e.file + ".png") : null;
			if (apply && sprite == null) { log.AppendLine("  NO SPRITE (import failed): " + e.file); continue; }

			var t = go.transform;
			if (e.kind == Kind.WorldSprite) {
				var sr = go.GetComponent<SpriteRenderer>();
				if (sr == null) { log.AppendLine("  NO SpriteRenderer: " + e.scenePath); continue; }
				var wp = new Vector3(world.x, world.y, t.position.z);
				log.AppendLine("  " + e.scenePath + "  pos " + t.position.ToString("F2") + " -> " + wp.ToString("F2")
					+ "  worldSize -> " + (size * Px).ToString("F2") + "  sprite " + (sr.sprite ? sr.sprite.name : "-") + " -> " + e.file);
				if (!apply) continue;
				Undo.RecordObject(t, "PSD layout"); Undo.RecordObject(sr, "PSD layout");
				sr.sprite = sprite;
				t.position = wp;
				// Sprites import at 100 PPU, so native world size = px/100; scale so world size = psd px * Px.
				SetWorldScale(t, new Vector3(Px * 100f, Px * 100f, 1f));
				var box = go.GetComponent<BoxCollider2D>();
				if (box != null) { Undo.RecordObject(box, "PSD layout"); box.offset = Vector2.zero; box.size = sprite.bounds.size; log.AppendLine("    resized BoxCollider2D"); }
				EditorUtility.SetDirty(go);
			} else {
				var rt = go.GetComponent<RectTransform>();
				var img = go.GetComponent<Image>();
				if (rt == null || img == null) { log.AppendLine("  NO RectTransform/Image: " + e.scenePath); continue; }
				var ls = rt.lossyScale;
				bool isBackground = e.file == "Kitchen";
				log.AppendLine("  " + e.scenePath + "  image " + (img.sprite ? img.sprite.name : "-") + " -> " + e.file
					+ (isBackground ? "  (geometry unchanged)" : "  centre -> " + world.ToString("F2") + " worldSize " + (size * Px).ToString("F2")));
				if (!apply) continue;
				Undo.RecordObject(rt, "PSD layout"); Undo.RecordObject(img, "PSD layout");
				img.sprite = sprite;
				if (!isBackground) {
					rt.position = new Vector3(world.x, world.y, rt.position.z);
					rt.sizeDelta = new Vector2(size.x * Px / ls.x, size.y * Px / ls.y);
				}
				EditorUtility.SetDirty(go);
			}
		}

		ApplyCamera(apply, log);

		if (apply) {
			Undo.CollapseUndoOperations(group);
			EditorSceneManager_MarkDirty();
		}
		Debug.Log(log.ToString());
	}

	static void ApplyCamera(bool apply, StringBuilder log) {
		var cam = Camera.main;
		if (cam == null) { log.AppendLine("  NO Main Camera"); return; }
		// Gameplay Area bbox centre (1149.5, 743.5) in PSD px
		var gp = new Vector2(Center.x + (1149.5f - 960f) * Px, -(743.5f - 540f) * Px);
		log.AppendLine("  CameraDirector shots: Gameplay -> pos " + gp.ToString("F2") + " ortho " + GameplayOrtho
			+ ";  Kitchen (wide) -> pos " + Center + " ortho " + OrthoWide.ToString("F3"));
		if (!apply) return;

		var dir = cam.GetComponent<CameraDirector>();
		if (dir == null) dir = Undo.AddComponent<CameraDirector>(cam.gameObject);
		Undo.RecordObject(dir, "PSD layout");
		SetShot(dir, CameraDirector.Gameplay, gp, GameplayOrtho);
		SetShot(dir, "Kitchen", Center, OrthoWide);
		EditorUtility.SetDirty(dir);

		Undo.RecordObject(cam, "PSD layout"); Undo.RecordObject(cam.transform, "PSD layout");
		cam.transform.position = new Vector3(gp.x, gp.y, cam.transform.position.z);
		cam.orthographicSize = GameplayOrtho;
	}

	static void SetShot(CameraDirector dir, string name, Vector2 pos, float ortho) {
		var s = dir.Find(name);
		if (s == null) { s = new CameraDirector.Shot { name = name }; dir.shots.Add(s); }
		s.position = pos; s.orthoSize = ortho;
	}

	static void ImportSprites(ItemList list) {
		AssetDatabase.Refresh();
		foreach (var it in list.items) {
			var path = SpriteDir + it.file + ".png";
			var imp = AssetImporter.GetAtPath(path) as TextureImporter;
			if (imp == null) continue;
			if (imp.textureType == TextureImporterType.Sprite && imp.spriteImportMode == SpriteImportMode.Single
				&& Mathf.Approximately(imp.spritePixelsPerUnit, 100f)) continue;
			imp.textureType = TextureImporterType.Sprite;
			imp.spriteImportMode = SpriteImportMode.Single;
			imp.spritePixelsPerUnit = 100f;
			imp.alphaIsTransparency = true;
			imp.mipmapEnabled = false;
			imp.SaveAndReimport();
		}
	}

	static void SetWorldScale(Transform t, Vector3 world) {
		var p = t.parent != null ? t.parent.lossyScale : Vector3.one;
		t.localScale = new Vector3(world.x / p.x, world.y / p.y, t.localScale.z);
	}

	static GameObject Find(string path) {
		var parts = path.Split('/');
		Transform cur = null;
		foreach (var r in SceneManager.GetActiveScene().GetRootGameObjects())
			if (r.name == parts[0]) { cur = r.transform; break; }
		for (int i = 1; cur != null && i < parts.Length; i++) cur = cur.Find(parts[i]);
		return cur != null ? cur.gameObject : null;
	}

	static void EditorSceneManager_MarkDirty() {
		UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
	}
}
