using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

// A short message that fades in, holds, and fades out in place (no scaling or movement).
public class HomeToast : MonoBehaviour
{
    public GameObject root;
    public Text messageText;
    public float seconds = 2.5f;
    public float fadeIn = 0.15f;
    public float fadeOut = 0.2f;
    public const float RevealSeconds = 0.8f;

    Coroutine _routine;
    CanvasGroup _group;
    bool _hoverShown;

    void Awake()
    {
        if (!root) return;
        // The scene's Animator used to scale the toast in; the fade replaces it.
        AnimUtil.DisableAnimator(root);
        _group = root.GetComponent<CanvasGroup>();
        if (!_group) _group = root.AddComponent<CanvasGroup>();
        _group.interactable = false;
        _group.blocksRaycasts = false;
        _group.alpha = 0f;
        root.transform.localScale = AnimUtil.RestScale(root.transform);

        // Sorts above ScreenFade's black cover, so a toast shown on scene entry reads over the hidden scene.
        var sorting = root.GetComponent<Canvas>();
        if (!sorting) sorting = root.AddComponent<Canvas>();
        sorting.overrideSorting = true;
        sorting.sortingLayerID = ScreenFade.TopSortingLayerId;
        sorting.sortingOrder = ScreenFade.AboveCoverOrder;

        root.SetActive(false);
    }

    public bool IsShowing { get { return _routine != null; } }

    // The canvas the toast is drawn by, for ScreenFade.Black to match.
    public Canvas RootCanvas
    {
        get
        {
            if (!root) return null;
            var canvas = root.GetComponentInParent<Canvas>(true);
            return canvas ? canvas.rootCanvas : null;
        }
    }

    public void Show(string msg)
    {
        _hoverShown = false;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(ShowRoutine(msg, seconds));
    }

    // Moving off the object: fades out the hover toast now. Any other toast (e.g. the scene-entry one) is left alone.
    public void HideHover()
    {
        if (!_hoverShown || _routine == null) return;
        _hoverShown = false;
        StopCoroutine(_routine);
        _routine = StartCoroutine(HideRoutine());
    }

    IEnumerator HideRoutine()
    {
        // A hover that replaced the scene-entry toast must still let the black cover lift.
        var reveal = ScreenFade.FadeIn(RevealSeconds);
        if (reveal != null) yield return reveal;
        if (_group) yield return Tween.Alpha(_group, 0f, fadeOut);
        if (root) root.SetActive(false);
        _routine = null;
    }

    // Hovering an object: replaces whatever toast is up with this key's message, unless it is already the one showing.
    public void ShowHover(string key)
    {
        string text = ToastData.Pick(key);
        if (text == null) return;
        if (IsShowing && messageText && messageText.text == text) return;
        Show(text);
        _hoverShown = true;
    }

    // Yield on this to wait until the toast has fully faded out.
    public IEnumerator ShowAndWait(string msg, float? hold = null)
    {
        if (_routine != null) StopCoroutine(_routine);
        yield return ShowRoutine(msg, hold ?? seconds);
    }

    // Data-driven: shows the best-matching toast for this trigger in Resources/toasts.txt, or nothing. Returns whether one was shown.
    public bool ShowKey(string key, Dictionary<string, float> extra = null)
    {
        string text = ToastData.Pick(key, extra);
        if (text == null) return false;
        Show(text);
        return true;
    }

    // Like ShowAndWait, for a key. Check Has first: this yields nothing when no toast matches.
    public IEnumerator ShowKeyAndWait(string key, Dictionary<string, float> extra = null)
    {
        string text = ToastData.Pick(key, extra);
        if (text != null) yield return ShowAndWait(text);
    }

    IEnumerator ShowRoutine(string msg, float hold)
    {
        if (messageText) messageText.text = msg;
        if (root)
        {
            root.SetActive(true);
            if (_group) { _group.alpha = 0f; yield return Tween.Alpha(_group, 1f, fadeIn); }
        }
        yield return new WaitForSecondsRealtime(hold);
        // A toast shown over the black scene-entry cover stays up while the scene fades in, then fades out.
        var reveal = ScreenFade.FadeIn(RevealSeconds);
        if (reveal != null) yield return reveal;
        if (_group) yield return Tween.Alpha(_group, 0f, fadeOut);
        if (root) root.SetActive(false);
        _routine = null;
    }
}

// Reads Resources/toasts.txt, shaped like boss dialogue.txt but with a "text" per variant:
//   { "trigger": [ { "when": {"day": "==1", "place": "==0"}, "once": true, "text": "..." } ] }
// Conditions use the DialogueContext stats plus whatever the caller passes (place: 0 bedroom, 1 living room, 2 door, 3 work).
// A trigger with no matching variant shows nothing. {stat} tokens in the text are filled from the same stats.
public static class ToastData
{
    const string PlayedPref = "toasts played";

    public static string Pick(string key, Dictionary<string, float> extra = null)
    {
        var file = Resources.Load<TextAsset>("toasts");
        if (file == null) return null;
        JSONNode variants = JSON.Parse(file.ToString())?[key];
        if (variants == null || variants.Count <= 0) return null;

        var context = DialogueContext.Build();
        if (extra != null) foreach (var kv in extra) context[kv.Key] = kv.Value;

        int i = DialogueSelector.Pick(key, variants, context, v => HasPlayed(key, v));
        if (i < 0) return null;
        if (variants[i]["once"].Value == "true") MarkPlayed(key, i);

        return TokenFormatter.Apply(variants[i]["text"].Value, context);
    }

    // One pref (not one per toast) so a new career can forget them all.
    public static void ResetPlayed() { PlayerPrefs.DeleteKey(PlayedPref); }
    static string Token(string key, int v) { return "|" + key + ":" + v + "|"; }
    static bool HasPlayed(string key, int v) { return PlayerPrefs.GetString(PlayedPref, "").Contains(Token(key, v)); }
    static void MarkPlayed(string key, int v) { PlayerPrefs.SetString(PlayedPref, PlayerPrefs.GetString(PlayedPref, "") + Token(key, v)); }
}
