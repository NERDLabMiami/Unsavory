using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

// Read-only: writes the active scene's hierarchy (path, kind, world/rect geometry, sprite) to scene_dump.json next to
// this script so it can be compared against layout.json (exported from the PSD). Does not modify the scene.
public static class SceneDump {
	[MenuItem("Tools/PSD Layout/Dump Scene Hierarchy")]
	static void Dump() {
		var sb = new StringBuilder();
		sb.Append("{\"scene\":\"").Append(SceneManager.GetActiveScene().name).Append("\",\"objects\":[\n");
		var first = true;
		foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
			Walk(root.transform, "", sb, ref first);
		sb.Append("\n]}");
		const string path = "Assets/Editor/PsdLayout/scene_dump.json";
		File.WriteAllText(path, sb.ToString());
		AssetDatabase.Refresh();
		Debug.Log("[PsdLayout] Wrote " + path);
	}

	static void Walk(Transform t, string parent, StringBuilder sb, ref bool first) {
		var p = parent.Length == 0 ? t.name : parent + "/" + t.name;
		if (!first) sb.Append(",\n");
		first = false;
		sb.Append("{\"path\":").Append(Q(p)).Append(",\"active\":").Append(t.gameObject.activeSelf ? "true" : "false")
		  .Append(",\"sibling\":").Append(t.GetSiblingIndex());

		var rt = t as RectTransform;
		var sr = t.GetComponent<SpriteRenderer>();
		var cv = t.GetComponent<Canvas>();
		if (cv != null) sb.Append(",\"canvas\":\"").Append(cv.renderMode).Append("\"");
		if (rt != null) {
			sb.Append(",\"kind\":\"ui\",\"anchorMin\":").Append(V(rt.anchorMin)).Append(",\"anchorMax\":").Append(V(rt.anchorMax))
			  .Append(",\"pivot\":").Append(V(rt.pivot)).Append(",\"anchoredPos\":").Append(V(rt.anchoredPosition))
			  .Append(",\"sizeDelta\":").Append(V(rt.sizeDelta)).Append(",\"scale\":").Append(V(rt.localScale));
			var c = new Vector3[4];
			rt.GetWorldCorners(c);
			sb.Append(",\"worldMin\":").Append(V(c[0])).Append(",\"worldMax\":").Append(V(c[2]));
		} else {
			sb.Append(",\"kind\":\"world\",\"pos\":").Append(V(t.position)).Append(",\"scale\":").Append(V(t.localScale));
		}
		if (sr != null && sr.sprite != null) {
			var b = sr.bounds;
			sb.Append(",\"sprite\":").Append(Q(sr.sprite.name)).Append(",\"sortLayer\":").Append(Q(sr.sortingLayerName))
			  .Append(",\"order\":").Append(sr.sortingOrder).Append(",\"boundsMin\":").Append(V(b.min)).Append(",\"boundsMax\":").Append(V(b.max));
		}
		var img = t.GetComponent<UnityEngine.UI.Image>();
		if (img != null && img.sprite != null) sb.Append(",\"image\":").Append(Q(img.sprite.name));
		var cam = t.GetComponent<Camera>();
		if (cam != null) sb.Append(",\"ortho\":").Append(cam.orthographicSize.ToString("R"));
		sb.Append("}");
		foreach (Transform c in t) Walk(c, p, sb, ref first);
	}

	static string Q(string s) => "\"" + s.Replace("\\", "\\\\").Replace("\"", "\\\"") + "\"";
	static string V(Vector2 v) => "[" + v.x.ToString("R") + "," + v.y.ToString("R") + "]";
	static string V(Vector3 v) => "[" + v.x.ToString("R") + "," + v.y.ToString("R") + "," + v.z.ToString("R") + "]";
}
