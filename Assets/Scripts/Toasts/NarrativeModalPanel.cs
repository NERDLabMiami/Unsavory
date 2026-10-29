using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unsavory.Toasts;

// Drives one of the Home scene's modal windows (Bed Panel, Work Panel, Drug Panel) from toasts.txt.
// The panel keeps its own look and pop-in/out animation; this fills it in:
//   Content/Title (optional) and Content/Text  <- the rule's title and text
//   Content/Buttons/*                           <- one button per choice (extra buttons are cloned, spare ones hidden)
// Added to the panel at runtime the first time the door, bed or pills are used, so no scene edits are needed.
public class NarrativeModalPanel : MonoBehaviour
{
    const string PopOutTrigger = "Pop Out";
    const float InputDelay = 0.15f;

    // Panels currently active (open, or still popping out), so a second press can't reopen one.
    static int _active;
    public static bool AnyOpen { get { return _active > 0; } }

    Animator _animator;
    RectTransform _rect;
    Canvas _canvas;
    ModalFocus _focus;
    Text _title, _body;
    RectTransform _buttonsRoot;
    readonly List<Button> _buttons = new List<Button>();
    string _key;
    HomeInteractable _source;
    bool _bound, _busy;
    float _openedAt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { _active = 0; }

    // The panel for "bed" / "door" / "pills": the one assigned on the interactable, else found by name. Null if the scene has none.
    public static NarrativeModalPanel Find(string objectKey, GameObject assigned)
    {
        GameObject go = assigned != null ? assigned : FindByName(objectKey);
        if (go == null) return null;
        var panel = go.GetComponent<NarrativeModalPanel>();
        if (panel == null) panel = go.AddComponent<NarrativeModalPanel>();
        return panel.Bind() ? panel : null;
    }

    static GameObject FindByName(string objectKey)
    {
        string[] hints;
        switch (objectKey)
        {
            case "bed": hints = new[] { "bed" }; break;
            case "door": hints = new[] { "work", "door" }; break;
            case "pills": hints = new[] { "drug", "pill" }; break;
            default: return null;
        }
        foreach (var focus in FindObjectsByType<ModalFocus>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            string name = focus.name.ToLowerInvariant();
            foreach (var hint in hints)
                if (name.Contains(hint)) return focus.gameObject;
        }
        return null;
    }

    bool Bind()
    {
        if (_bound) return true;
        Transform content = transform.Find("Content");
        Transform bodyTransform = content != null ? content.Find("Text") : null;
        Transform titleTransform = content != null ? content.Find("Title") : null;
        Transform buttonsTransform = content != null ? content.Find("Buttons") : null;
        _body = bodyTransform != null ? bodyTransform.GetComponent<Text>() : null;
        _title = titleTransform != null ? titleTransform.GetComponent<Text>() : null;
        _buttonsRoot = buttonsTransform as RectTransform;
        if (_buttonsRoot != null)
            foreach (Transform child in _buttonsRoot)
            {
                var b = child.GetComponent<Button>();
                if (b != null) _buttons.Add(b);
            }
        if (_body == null || _buttons.Count == 0)
        {
            Debug.LogWarning("[Narrative] " + name + " needs Content/Text and at least one button under Content/Buttons; using the built-in window instead.");
            Destroy(this);
            return false;
        }

        // The panels were sized for one short line at 42-51pt; the rule text is longer. Let it shrink to fit the panel's own rect.
        FitText(_body, 18);
        if (_title != null) FitText(_title, 18);

        _animator = GetComponent<Animator>();
        _rect = (RectTransform)transform;
        _canvas = GetComponentInParent<Canvas>();
        _focus = GetComponent<ModalFocus>();

        // The Drug Panel's own script would rewrite the text and the Buy/Take buttons; the data decides instead.
        var drugs = GetComponent<BuyDrugsScript>();
        if (drugs != null) drugs.narrativeDriven = true;

        _bound = true;
        return true;
    }

    static void FitText(Text text, int minSize)
    {
        text.resizeTextForBestFit = true;
        text.resizeTextMaxSize = Mathf.Max(minSize, Mathf.RoundToInt(text.fontSize * 0.7f));   // start smaller than the scene's placeholder size
        text.resizeTextMinSize = minSize;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
    }

    static void FitLabel(TMP_Text label)
    {
        label.enableAutoSizing = true;
        label.fontSizeMax = label.fontSize * 0.8f;
        label.fontSizeMin = Mathf.Min(14f, label.fontSizeMax);
    }

    public void Show(string objectKey, ModalView view, HomeInteractable source)
    {
        if (gameObject.activeSelf) return;   // still popping out from the last time
        _key = objectKey;
        _source = source;
        _busy = false;

        string title = view.Title ?? "";
        string text = view.Text ?? "";
        if (_title != null) { _title.text = title; _body.text = text; }
        else _body.text = (title.Length > 0 ? "<b>" + title + "</b>\n" : "") + text;

        Button first = null;
        for (int i = 0; i < view.Choices.Count; i++)
        {
            var choice = view.Choices[i];
            Button button = ButtonAt(i);
            button.gameObject.SetActive(true);
            button.interactable = choice.Enabled;
            SetLabel(button, choice.Label);
            // Replacing the event drops the copied persistent calls (take/buy/sounds) the scene buttons still carry.
            button.onClick = new Button.ButtonClickedEvent();
            string action = choice.Action;
            button.onClick.AddListener(() => Choose(action));
            if (first == null && choice.Enabled) first = button;
        }
        for (int i = view.Choices.Count; i < _buttons.Count; i++) _buttons[i].gameObject.SetActive(false);

        if (_buttonsRoot != null) LayoutRebuilder.ForceRebuildLayoutImmediate(_buttonsRoot);
        if (_focus != null) _focus.firstSelected = first;

        gameObject.SetActive(true);
    }

    // Extra choices copy the last button, so they keep its look and place in the layout.
    Button ButtonAt(int index)
    {
        while (index >= _buttons.Count)
        {
            var template = _buttons[_buttons.Count - 1];
            var clone = Instantiate(template.gameObject, template.transform.parent);
            clone.name = template.name + " " + (_buttons.Count + 1);
            _buttons.Add(clone.GetComponent<Button>());
        }
        return _buttons[index];
    }

    static void SetLabel(Button button, string label)
    {
        var tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            if (!tmp.enableAutoSizing) FitLabel(tmp);
            tmp.text = label;
            return;
        }
        var legacy = button.GetComponentInChildren<Text>(true);
        if (legacy != null) legacy.text = label;
    }

    void OnEnable()
    {
        _active++;
        _busy = false;
        _openedAt = Time.unscaledTime;
    }

    void OnDisable()
    {
        _active = Mathf.Max(0, _active - 1);
        GameInput.ShowCursorInModal = false;
    }

    // B / Esc, or a click or tap outside the panel, is the cancel choice.
    void Update()
    {
        var input = GameInput.Instance;
        if (_busy || input == null) return;
        // The stick cursor stays usable on the buttons, like the phone.
        GameInput.ShowCursorInModal = input.UsingVirtualCursor;
        if (Time.unscaledTime - _openedAt < InputDelay) return;

        bool outside = input.ActivatePressed && (input.ActivateFromPointer || input.UsingVirtualCursor) && !PointerInside(input.PointerPosition);
        if (input.CancelPressed || outside) Choose(NarrativeActions.Cancel);
    }

    bool PointerInside(Vector2 screenPoint)
    {
        Camera cam = null;
        if (_canvas && _canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = _canvas.rootCanvas.worldCamera;
        return RectTransformUtility.RectangleContainsScreenPoint(_rect, screenPoint, cam);
    }

    // The panel starts closing first, so a second press can't fire the action twice.
    void Choose(string action)
    {
        if (_busy) return;
        if (action != NarrativeActions.Cancel && !NarrativeRuntime.IsActionEnabled(_key, action))
        {
            Debug.LogWarning("[Narrative] '" + action + "' is no longer available for " + _key + "; ignoring it.");
            return;
        }
        _busy = true;
        string key = _key;
        var source = _source;
        GameInput.ShowCursorInModal = false;
        Close();
        ModalActions.Execute(action, key, source);
    }

    // The Standard Panel animation plays its pop-out clip, which deactivates the panel (and releases the gate lock).
    void Close()
    {
        if (_animator != null && _animator.isActiveAndEnabled) _animator.SetTrigger(PopOutTrigger);
        else gameObject.SetActive(false);
    }
}
