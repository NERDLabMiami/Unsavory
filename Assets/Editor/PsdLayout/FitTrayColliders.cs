using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Makes each BoxCollider2D that sits on a SpriteRenderer's GameObject match that sprite's bounds. The redesign swapped
// tray sprites and resized them, but the colliders kept their old sizes, so clicks hit the wrong area.
public static class FitTrayColliders {
	[MenuItem("Tools/PSD Layout/Fit Tray Colliders (preview)")] static void Preview() { Run(false); }
	[MenuItem("Tools/PSD Layout/Fit Tray Colliders (apply)")] static void Apply() { Run(true); }

	static void Run(bool apply) {
		int n = 0;
		var log = new System.Text.StringBuilder(apply ? "[PsdLayout] Fit colliders\n" : "[PsdLayout] Fit colliders (preview)\n");
		foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects()) {
			foreach (var box in root.GetComponentsInChildren<BoxCollider2D>(true)) {
				var sr = box.GetComponent<SpriteRenderer>();
				if (sr == null || sr.sprite == null) continue;
				if (!IsTray(box.transform)) continue;
				var b = sr.sprite.bounds;
				log.AppendLine("  " + Path(box.transform) + "  size " + box.size.ToString("F2") + " -> " + ((Vector2)b.size).ToString("F2"));
				if (apply) {
					Undo.RecordObject(box, "Fit tray collider");
					box.size = b.size;
					box.offset = b.center;
					EditorUtility.SetDirty(box);
				}
				n++;
			}
		}
		log.AppendLine("  " + n + " collider(s)");
		if (apply && n > 0) UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
		Debug.Log(log.ToString());
	}

	static bool IsTray(Transform t) {
		for (; t != null; t = t.parent) if (t.name == "Trays") return true;
		return false;
	}

	static string Path(Transform t) {
		var p = t.name;
		for (t = t.parent; t != null; t = t.parent) p = t.name + "/" + p;
		return p;
	}
}
