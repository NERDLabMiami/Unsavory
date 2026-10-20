using UnityEditor;
using UnityEngine;

// Points every order prefab's RecipeScript at the blank slip, so the card is built from its ingredient list at runtime.
public static class AssignRecipeSlip {
	const string SlipPath = "Assets/Sprites/order-blank-template.png";

	[MenuItem("Unsavory/Assign Blank Slip To Recipes")]
	static void Run() {
		var slip = AssetDatabase.LoadAssetAtPath<Sprite>(SlipPath);
		if (slip == null) { Debug.LogError("Blank slip sprite not found at " + SlipPath); return; }

		int count = 0;
		foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Recipes", "Assets/Prefabs" })) {
			var path = AssetDatabase.GUIDToAssetPath(guid);
			var root = PrefabUtility.LoadPrefabContents(path);
			try {
				var rs = root.GetComponent<RecipeScript>();
				if (rs == null) continue;
				rs.slipBackground = slip;
				PrefabUtility.SaveAsPrefabAsset(root, path);
				count++;
			} finally {
				PrefabUtility.UnloadPrefabContents(root);
			}
		}
		Debug.Log("Assigned blank slip to " + count + " recipe prefabs.");
	}
}
