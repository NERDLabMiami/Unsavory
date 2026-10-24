using System.IO;
using UnityEditor;
using UnityEngine;

// One-time migration: turns each order prefab in Resources/Recipes into a RecipeData asset, prepares the single
// Recipe prefab, and sets the slip's 9-slice borders. Safe to run again. Old prefabs are left for you to delete.
public static class ConvertRecipesToData {
	const string RecipeDir = "Assets/Resources/Recipes";
	const string DataDir = RecipeDir + "/Data";
	const string SinglePrefab = RecipeDir + "/Recipe.prefab";
	const string SlipPath = "Assets/Sprites/order-blank-template.png";

	[MenuItem("Unsavory/Convert Recipe Prefabs To Data")]
	static void Run() {
		if (!AssetDatabase.IsValidFolder(DataDir)) AssetDatabase.CreateFolder(RecipeDir, "Data");

		SetSlipBorder();

		int count = 0;
		foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { RecipeDir })) {
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (path == SinglePrefab) continue;
			var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
			var rs = go != null ? go.GetComponent<RecipeScript>() : null;
			if (rs == null || rs.ingredients.Count == 0) continue;

			string name = Path.GetFileNameWithoutExtension(path);
			string assetPath = DataDir + "/" + name + ".asset";
			var data = AssetDatabase.LoadAssetAtPath<RecipeData>(assetPath);
			if (data == null) {
				data = ScriptableObject.CreateInstance<RecipeData>();
				AssetDatabase.CreateAsset(data, assetPath);
			}
			data.ingredients = new System.Collections.Generic.List<GameObject>(rs.ingredients);
			data.completed = rs.completed;
			data.rocket = name.EndsWith("-R");
			EditorUtility.SetDirty(data);
			count++;
		}

		PrepareSinglePrefab();
		AssetDatabase.SaveAssets();
		Debug.Log("Converted " + count + " recipe prefabs to RecipeData in " + DataDir + ".");
	}

	// Top keeps the punch hole, bottom keeps the torn edge, sides keep the outline; only the middle stretches.
	static void SetSlipBorder() {
		var importer = AssetImporter.GetAtPath(SlipPath) as TextureImporter;
		if (importer == null) { Debug.LogError("Slip texture not found at " + SlipPath); return; }
		var border = new Vector4(40f, 80f, 40f, 110f); // left, bottom, right, top
		var sheet = importer.spritesheet;
		if (sheet.Length > 0) {
			sheet[0].border = border;
			importer.spritesheet = sheet;
		} else {
			importer.spriteBorder = border;
		}
		importer.SaveAndReimport();
	}

	// The card's background object must be named "Sprite" (RecipeScript and RecipeAnim look it up).
	static void PrepareSinglePrefab() {
		var slip = AssetDatabase.LoadAssetAtPath<Sprite>(SlipPath);
		var root = PrefabUtility.LoadPrefabContents(SinglePrefab);
		try {
			var rs = root.GetComponent<RecipeScript>();
			if (rs == null) { Debug.LogError("Recipe.prefab has no RecipeScript."); return; }
			rs.ingredients.Clear();
			rs.completed = null;
			rs.slipBackground = slip;
			var paper = root.transform.Find("Paper");
			if (paper != null && root.transform.Find("Sprite") == null) paper.name = "Sprite";
			root.transform.localScale = Vector3.one * 0.6f;
			PrefabUtility.SaveAsPrefabAsset(root, SinglePrefab);
		} finally {
			PrefabUtility.UnloadPrefabContents(root);
		}
	}
}
