using UnityEditor;
using UnityEngine;

// One-time setup for the boss expression art: moves Assets/Sprites/Boss to Assets/Resources/Boss (so CharacterDialog can
// Resources.Load "Boss/boss-<face>-<view>") and imports each PNG as a single sprite (they arrive auto-sliced into fragments).
public static class BossSpriteSetup
{
    const string From = "Assets/Sprites/Boss";
    const string To = "Assets/Resources/Boss";

    [MenuItem("Tools/Boss/Set Up Expression Sprites")]
    static void Run()
    {
        if (AssetDatabase.IsValidFolder(From))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Resources")) AssetDatabase.CreateFolder("Assets", "Resources");
            string err = AssetDatabase.MoveAsset(From, To);
            if (!string.IsNullOrEmpty(err)) { Debug.LogError("[BossSpriteSetup] " + err); return; }
        }

        int fixedCount = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { To }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritesheet = new SpriteMetaData[0];
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SaveAndReimport();
            fixedCount++;
        }
        Debug.Log($"[BossSpriteSetup] {fixedCount} boss sprites set to Single in {To}.");
    }
}
