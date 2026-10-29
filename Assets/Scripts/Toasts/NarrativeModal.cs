using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Unsavory.Toasts;

// The bed / door / pills confirmation window. Title, text and choices come from toasts.txt (modals.*);
// what a choice does lives in ModalActions, which calls the game's existing logic. Built in code so no scene edits are needed,
// and styled after the toast. Mouse, touch, keyboard and gamepad all go through ModalFocus and the EventSystem.
public class NarrativeModal : MonoBehaviour
{
    const string LockReason = "NARRATIVE_MODAL";
    const float CommitWindow = 1.5f;

    static NarrativeModal _instance;
    static float _committedAt = -100f;

    GameObject _root;
    Text _title, _body;
    RectTransform _buttons;
    ModalFocus _focus;
    Style _style;
    string _objectKey;
    HomeInteractable _source;
    bool _open;

    struct Style { public Font font; public int titleSize, bodySize, buttonSize; public Color text; public Sprite sprite; public Color panel; }

    public static bool IsOpen { get { return (_instance != null && _instance._open) || NarrativeModalPanel.AnyOpen; } }

    // A confirmed action that leaves the scene: ignore input until the load happens (or the window passes).
    public static bool IsCommitted { get { return Time.unscaledTime - _committedAt < CommitWindow; } }
    public static void Commit() { _committedAt = Time.unscaledTime; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics() { _instance = null; _committedAt = -100f; SceneManager.sceneLoaded += (s, m) => _committedAt = -100f; }

    // Returns false when there is no modal for this object (data missing or invalid), so the caller can fall back.
    public static bool Open(string objectKey, HomeInteractable source)
    {
        if (IsOpen || IsCommitted) return true;
        var view = NarrativeRuntime.PickModal(objectKey);
        if (view == null) return false;
        // The Home scene's own panel (Bed / Work / Drug Panel) when it has one; the code-built window otherwise.
        var panel = NarrativeModalPanel.Find(objectKey, source != null ? source.narrativePanel : null);
        if (panel != null) { panel.Show(objectKey, view, source); return true; }
        if (_instance == null) _instance = Create();
        _instance.Show(objectKey, view, source);
        return true;
    }

    static NarrativeModal Create()
    {
        var go = new GameObject("Narrative Modal");
        return go.AddComponent<NarrativeModal>();
    }

    void Awake()
    {
        _style = FindStyle();
        BuildUi();
    }

    // Borrow the toast's font and background so the window looks like the rest of the apartment UI.
    static Style FindStyle()
    {
        var style = new Style
        {
            titleSize = 34, bodySize = 26, buttonSize = 26,
            text = new Color(0.12f, 0.1f, 0.1f, 1f),
            panel = new Color(0.98f, 0.95f, 0.88f, 1f),
        };
        var toast = FindAnyObjectByType<HomeToast>(FindObjectsInactive.Include);
        if (toast != null)
        {
            if (toast.messageText != null)
            {
                style.font = toast.messageText.font;
                style.text = toast.messageText.color.a > 0.01f ? new Color(toast.messageText.color.r, toast.messageText.color.g, toast.messageText.color.b, 1f) : style.text;
            }
            var bg = toast.root != null ? toast.root.GetComponent<Image>() : null;
            if (bg != null) { style.sprite = bg.sprite; if (bg.color.a > 0.01f) style.panel = new Color(bg.color.r, bg.color.g, bg.color.b, 1f); }
        }
        if (style.font == null) style.font = Font.CreateDynamicFontFromOSFont("Arial", 32);
        return style;
    }

    void BuildUi()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 500;   // above the phone; ScreenFade (32000) and the toast stay on top
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        _root = new GameObject("Dim", typeof(RectTransform), typeof(Image));
        _root.transform.SetParent(transform, false);
        Stretch((RectTransform)_root.transform);
        var dim = _root.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.55f);
        dim.raycastTarget = true;   // swallows clicks behind the window

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        panel.transform.SetParent(_root.transform, false);
        var rt = (RectTransform)panel.transform;
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(900f, 0f);
        var image = panel.GetComponent<Image>();
        image.sprite = _style.sprite;
        image.type = _style.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = _style.panel;
        var layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(60, 60, 50, 50);
        layout.spacing = 28f;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        var fitter = panel.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        _title = MakeText("Title", panel.transform, _style.titleSize, FontStyle.Bold);
        _body = MakeText("Body", panel.transform, _style.bodySize, FontStyle.Normal);

        var buttons = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup));
        buttons.transform.SetParent(panel.transform, false);
        var bl = buttons.GetComponent<VerticalLayoutGroup>();
        bl.spacing = 16f;
        bl.childControlWidth = bl.childControlHeight = true;
        bl.childForceExpandWidth = true;
        bl.childForceExpandHeight = false;
        _buttons = (RectTransform)buttons.transform;

        _focus = panel.AddComponent<ModalFocus>();
        _focus.allowCancel = true;   // B / Esc presses the choice named "No Button"

        _root.SetActive(false);
    }

    Text MakeText(string name, Transform parent, int size, FontStyle fontStyle)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = _style.font;
        t.fontSize = size;
        t.fontStyle = fontStyle;
        t.color = _style.text;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    void Show(string objectKey, ModalView view, HomeInteractable source)
    {
        _objectKey = objectKey;
        _source = source;
        _title.text = view.Title;
        _body.text = view.Text;

        for (int i = _buttons.childCount - 1; i >= 0; i--)
        {
            var old = _buttons.GetChild(i).gameObject;
            old.SetActive(false);   // Destroy waits for the end of the frame; the old buttons must not stay focusable
            Destroy(old);
        }
        Button first = null;
        bool hasCancel = false;
        foreach (var choice in view.Choices)
        {
            bool cancel = choice.Action == NarrativeActions.Cancel && !hasCancel;
            hasCancel |= cancel;
            var button = MakeButton(choice, cancel);
            if (first == null && choice.Enabled) first = button;
        }
        _focus.firstSelected = first;

        _open = true;
        _root.SetActive(true);
        if (GameplayGate.Instance != null) GameplayGate.Instance.Lock(LockReason);
    }

    Button MakeButton(ModalChoiceView choice, bool cancel)
    {
        // ModalFocus presses a button named "No Button" on Cancel.
        var go = new GameObject(cancel ? "No Button" : "Choice " + choice.Action, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(_buttons, false);
        go.GetComponent<LayoutElement>().minHeight = 64f;

        var image = go.GetComponent<Image>();
        image.sprite = _style.sprite;
        image.type = _style.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = Color.white;

        var button = go.GetComponent<Button>();
        button.targetGraphic = image;
        button.interactable = choice.Enabled;
        var colors = button.colors;
        colors.normalColor = new Color(0.93f, 0.88f, 0.76f, 1f);
        colors.highlightedColor = new Color(1f, 0.93f, 0.6f, 1f);
        colors.selectedColor = new Color(1f, 0.86f, 0.35f, 1f);
        colors.pressedColor = new Color(0.85f, 0.7f, 0.25f, 1f);
        colors.disabledColor = new Color(0.62f, 0.6f, 0.58f, 0.7f);
        button.colors = colors;

        var label = MakeText("Label", go.transform, _style.buttonSize, FontStyle.Normal);
        Stretch((RectTransform)label.transform);
        label.text = choice.Label;
        if (!choice.Enabled) label.color = new Color(_style.text.r, _style.text.g, _style.text.b, 0.45f);

        string action = choice.Action;
        button.onClick.AddListener(() => Choose(action));
        return button;
    }

    // The window closes first, so a second press can't fire the action twice.
    void Choose(string action)
    {
        if (!_open) return;
        if (action != NarrativeActions.Cancel && !NarrativeRuntime.IsActionEnabled(_objectKey, action))
        {
            Debug.LogWarning("[Narrative] '" + action + "' is no longer available for " + _objectKey + "; ignoring it.");
            return;
        }
        var source = _source;
        string key = _objectKey;
        Close();
        ModalActions.Execute(action, key, source);
    }

    void Update()
    {
        if (!_open) return;
        var input = GameInput.Instance;
        // The stick cursor stays usable on the buttons, like the phone.
        GameInput.ShowCursorInModal = input != null && input.UsingVirtualCursor;
    }

    void Close()
    {
        if (!_open) return;
        _open = false;
        _root.SetActive(false);
        GameInput.ShowCursorInModal = false;
        if (GameplayGate.Instance != null) GameplayGate.Instance.Unlock(LockReason);
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    void OnDisable()
    {
        if (_open) Close();
    }

    static void Stretch(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
    }
}

// Maps each action identifier in toasts.txt to the game's existing logic. The window has already closed and
// NarrativeRuntime.IsActionEnabled has rechecked availability against the current state.
public static class ModalActions
{
    public static void Execute(string action, string objectKey, HomeInteractable source)
    {
        var toast = Object.FindAnyObjectByType<HomeToast>();
        switch (action)
        {
            case NarrativeActions.Cancel:
                return;

            case NarrativeActions.SkipShift:
                // The existing missed-work path: sick day, health recovery, one reload into the next morning.
                NarrativeModal.Commit();
                if (source != null) source.ExecuteAction();
                return;

            case NarrativeActions.DriveToWork:
                // Driving with the insurance overdue ends in a crash that wrecks the car; the commute still happens, by transit.
                NarrativeModal.Commit();
                CommuteState.Drive();
                if (source != null) source.ExecuteAction();
                return;

            case NarrativeActions.TransitToWork:
                // The ordinary commute and shift; lateness is whatever the game computes (Level reads CommuteState.LateEffect).
                NarrativeModal.Commit();
                if (source != null) source.ExecuteAction();
                return;

            case NarrativeActions.BuyPills:
                Report(toast, PillsService.Buy(), "buy_pills");
                return;

            case NarrativeActions.TakePill:
                Report(toast, PillsService.Take(), "take_pill");
                return;

            case NarrativeActions.OpenPhone:
                foreach (var h in Object.FindObjectsByType<HomeInteractable>(FindObjectsSortMode.None))
                    if (h.action == HomeInteractable.HomeAction.OpenBills) { h.OpenPhone(); break; }
                return;

            default:
                Debug.LogWarning("[Narrative] unknown modal action '" + action + "'");
                return;
        }
    }

    // A completed action gets its after_action line; a refused one gets the ordinary reason instead.
    static void Report(HomeToast toast, PillsService.Result result, string afterAction)
    {
        if (toast == null) return;
        if (result == PillsService.Result.Ok)
        {
            var music = Object.FindAnyObjectByType<MusicLibrary>();
            if (music != null) { if (afterAction == "take_pill") music.popPill(); else music.spendMoney(); }
            toast.ShowAfterAction(afterAction);
        }
        else toast.Show(PillsService.FailureText(result));
    }
}
