using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Builds Assets/Scenes/Credits.unity (black screen, scrolling credits text, CreditsController) and adds it to the build settings.
/// The credits text lives in Resources/credits.txt. Restyle the scene by hand afterwards; running this again asks before overwriting it.
/// </summary>
static class BuildCreditsScene
{
    const string ScenePath = "Assets/Scenes/Credits.unity";

    [MenuItem("Tools/Create Credits Scene")]
    static void Create()
    {
        if (System.IO.File.Exists(ScenePath) &&
            !EditorUtility.DisplayDialog("Credits scene exists", "Replace " + ScenePath + "?", "Replace", "Cancel"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraGO = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraGO.tag = "MainCamera";
        var cam = cameraGO.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.orthographic = true;

        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        var scrollerGO = new GameObject("Scroller", typeof(RectTransform), typeof(ContentSizeFitter), typeof(VerticalLayoutGroup));
        scrollerGO.transform.SetParent(canvasGO.transform, false);
        var scroller = (RectTransform)scrollerGO.transform;
        scroller.anchorMin = scroller.anchorMax = new Vector2(0.5f, 0f);   // bottom centre
        scroller.pivot = new Vector2(0.5f, 1f);                            // top edge at the bottom of the screen: starts off screen
        scroller.sizeDelta = new Vector2(1100f, 0f);
        scroller.anchoredPosition = Vector2.zero;
        scrollerGO.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var layout = scrollerGO.GetComponent<VerticalLayoutGroup>();
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = layout.childForceExpandHeight = false;

        var textGO = new GameObject("Credits Text", typeof(RectTransform), typeof(TextMeshProUGUI));
        textGO.transform.SetParent(scrollerGO.transform, false);
        var text = textGO.GetComponent<TextMeshProUGUI>();
        text.alignment = TextAlignmentOptions.Top;
        text.fontSize = 56;
        text.color = Color.white;
        text.richText = true;
        text.raycastTarget = false;
        text.text = "(credits.txt)";

        var controllerGO = new GameObject("Credits Controller", typeof(CreditsController));
        var controller = controllerGO.GetComponent<CreditsController>();
        controller.text = text;
        controller.scroller = scroller;
        controller.fade = canvasGO.GetComponent<CanvasGroup>();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AddToBuildSettings();
        Debug.Log("Created " + ScenePath);
    }

    static void AddToBuildSettings()
    {
        var scenes = EditorBuildSettings.scenes.ToList();
        if (scenes.Any(s => s.path == ScenePath)) return;
        scenes.Add(new EditorBuildSettingsScene(ScenePath, true));
        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
