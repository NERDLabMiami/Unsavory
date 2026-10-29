using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using Unsavory.Toasts;

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
    bool _sequence;

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

    // True while a scene-entrance sequence plays; hover toasts stay out of its way until it has finished.
    public bool IsSequence { get { return _sequence; } }

    void OnDisable() { _sequence = false; }

    // The canvas the toast is drawn by, for ScreenFade.Black to match.
    public Canvas RootCanvas
    {
        get
        {
            if (!root) return null;
            // Not Canvas.rootCanvas: on the inactive toast's own nested canvas it reports itself (an overlay), not the scene's canvas.
            var canvases = root.GetComponentsInParent<Canvas>(true);
            return canvases.Length > 0 ? canvases[canvases.Length - 1] : null;
        }
    }

    public void Show(string msg)
    {
        _hoverShown = false;
        _sequence = false;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(SequenceRoutine(new List<string> { msg }, seconds));
    }

    // Scene entrance: each line fades in, holds and fades out in turn; the toast is gone after the last one.
    public void ShowSequence(IList<string> lines, Action onShown = null)
    {
        if (lines == null || lines.Count == 0) return;
        _hoverShown = false;
        if (_routine != null) StopCoroutine(_routine);
        _sequence = true;
        _routine = StartCoroutine(SequenceRoutine(lines, seconds, onShown));
    }

    // Moving off the object: fades out the hover toast now. Any other toast (e.g. the scene-entry one) is left alone.
    public void HideHover()
    {
        if (!_hoverShown || _routine == null) return;
        _hoverShown = false;
        StopCoroutine(_routine);
        // Deactivating the apartment (e.g. leaving for work) disables the pointer router, which hides the hover; coroutines can't start then.
        if (!isActiveAndEnabled) { _routine = null; if (_group) _group.alpha = 0f; if (root) root.SetActive(false); return; }
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
    public void ShowHover(string objectKey)
    {
        // The scene-entry toast is a caption over the black cover; a pointer already resting on an object must not replace it.
        if (ScreenFade.IsCovered || _sequence) return;
        string text = NarrativeRuntime.PickHover(objectKey);
        if (text == null) return;
        if (IsShowing && messageText && messageText.text == text) return;
        Show(text);
        _hoverShown = true;
    }

    // Yield on this to wait until the toast has fully faded out.
    public IEnumerator ShowAndWait(string msg, float? hold = null)
    {
        if (_routine != null) StopCoroutine(_routine);
        _sequence = false;
        yield return SequenceRoutine(new List<string> { msg }, hold ?? seconds);
    }

    // Yield on this to wait until the last line has faded out.
    public IEnumerator ShowSequenceAndWait(IList<string> lines, Action onShown = null)
    {
        if (lines == null || lines.Count == 0) yield break;
        if (_routine != null) StopCoroutine(_routine);
        _sequence = true;
        yield return SequenceRoutine(lines, seconds, onShown);
    }

    // Scene entry: the world line then the state line (or one special), chosen from toasts.txt. Returns whether one was shown.
    public bool ShowEntry(string kind, Action<NarrativeContext> extra = null)
    {
        var message = NarrativeRuntime.PickEntry(kind, extra);
        if (message == null) return false;
        ShowSequence(message.Lines, message.MarkShown);
        return true;
    }

    // An outcome line (after_action) on the current screen.
    public bool ShowAfterAction(string action)
    {
        var message = NarrativeRuntime.PickAfter(action);
        if (message == null) return false;
        _hoverShown = false;
        _sequence = false;
        if (_routine != null) StopCoroutine(_routine);
        _routine = StartCoroutine(SequenceRoutine(message.Lines, seconds, message.MarkShown));
        return true;
    }

    void SetTextAlpha(float a)
    {
        if (!messageText) return;
        var c = messageText.color;
        c.a = a;
        messageText.color = c;
    }

    IEnumerator SequenceRoutine(IList<string> lines, float hold, Action onShown = null)
    {
        if (root) root.SetActive(true);
        if (onShown != null) onShown();
        // With several lines only the text fades between them; the background stays up and leaves with the last line.
        bool multi = lines.Count > 1;
        SetTextAlpha(1f);
        for (int i = 0; i < lines.Count; i++)
        {
            bool last = i == lines.Count - 1;
            if (messageText) messageText.text = lines[i];
            if (_group && (!multi || i == 0))
            {
                _group.alpha = 0f;
                // A scene-load hitch makes the first frame's delta huge, which would finish the fade in one step.
                if (i == 0) yield return null;
                yield return Tween.Alpha(_group, 1f, fadeIn);
            }
            else if (multi && messageText)
            {
                SetTextAlpha(0f);
                yield return Tween.Alpha(messageText, 1f, fadeIn);
            }
            yield return Tween.Wait(hold);
            // A toast shown over the black scene-entry cover stays up while the scene fades in, then fades out.
            if (last)
            {
                var reveal = ScreenFade.FadeIn(RevealSeconds);
                if (reveal != null) yield return reveal;
            }
            if (multi && !last)
            {
                if (messageText) yield return Tween.Alpha(messageText, 0f, fadeOut);
            }
            else if (_group) yield return Tween.Alpha(_group, 0f, fadeOut);
        }
        if (root) root.SetActive(false);
        _sequence = false;
        _routine = null;
    }
}

// Kept so a new career can clear the toast state; the data and rules live in NarrativeRuntime.
public static class ToastData
{
    public static void ResetPlayed() { NarrativeRuntime.ResetRun(); }
}
