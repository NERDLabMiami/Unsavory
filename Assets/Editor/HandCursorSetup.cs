using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates Resources/HandCursorSprites.asset with placeholder hand sprites if it doesn't exist yet.
/// Replace the sprites on the asset with the final point/hover/tap/grab art.
/// </summary>
[InitializeOnLoad]
static class HandCursorSetup
{
    const string AssetPath = "Assets/Resources/HandCursorSprites.asset";

    static HandCursorSetup() => EditorApplication.delayCall += Create;

    [MenuItem("Tools/Create Hand Cursor Asset")]
    static void Create()
    {
        if (AssetDatabase.LoadAssetAtPath<HandCursorSprites>(AssetPath) != null) return;

        var asset = ScriptableObject.CreateInstance<HandCursorSprites>();
        asset.point = FirstSprite("Assets/Sprites/hand-searching.png");
        asset.hover = FirstSprite("Assets/Sprites/hand-arm-search.png");
        asset.tap = FirstSprite("Assets/Sprites/hand-grabbing.png");
        asset.grab = FirstSprite("Assets/Sprites/hand-arm-grab.png");
        AssetDatabase.CreateAsset(asset, AssetPath);
        AssetDatabase.SaveAssets();
    }

    static Sprite FirstSprite(string path)
    {
        foreach (var o in AssetDatabase.LoadAllAssetsAtPath(path))
            if (o is Sprite s) return s;
        return null;
    }
}
