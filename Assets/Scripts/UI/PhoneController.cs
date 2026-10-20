using System.Collections.Generic;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Drives the Phone UI: Home -> Bills / Mail -> (View Message). Back (UI/Cancel, or any Header button)
/// steps one level up and finally closes the phone. Gamepad navigation between the visible buttons is
/// provided by the ModalFocus on the same object; this script only decides which button gets selection.
/// </summary>
public class PhoneController : MonoBehaviour
{
    public enum Screen { Home, Bills, Mail, MailView }

    [Header("Screens")]
    public GameObject homeScreen;
    public GameObject billsScreen;
    public GameObject mailScreen;
    public GameObject messagesList;     // Mail/Messages
    public GameObject viewMessage;      // Mail/View Message

    [Header("Selection targets")]
    public Selectable homeFirst;        // Bills icon on the home screen
    public Selectable viewMessageBack;  // View Message header button
    public Selectable billsBack;        // Bills header button (found by name if unset)
    public Selectable mailBack;         // Mail header button (found by name if unset)
    public Selectable homeMail;         // Mail icon on the home screen (found by name if unset)

    [Header("Bills")]
    public Transform billsContainer;
    public BillScript billPrefab;

    [Header("Mail")]
    public Transform messagesContainer;
    public PhoneMessageRow messagePrefab;
    public TMP_Text viewInfo;
    public TMP_Text viewBody;

    public Screen Current { get; private set; } = Screen.Home;

    /// <summary>True while the phone is on screen; the world router ignores clicks so they can't hit what's behind it.</summary>
    public static bool IsOpen { get; private set; }
    static float LastClosed = -10f;

    /// <summary>Open, or closed within the last moment, so the click that closed it can't fall through to the room.</summary>
    public static bool BlocksWorld => IsOpen || (Time.unscaledTime >= LastClosed && Time.unscaledTime - LastClosed < 0.35f);

    readonly List<BillScript> _bills = new List<BillScript>();
    readonly List<PhoneMessageRow> _rows = new List<PhoneMessageRow>();
    JSONNode _mail;
    int _openedMessage = -1;
    Transform _rootCanvas;
    Vector3 _savedRootScale;
    bool _scaledRoot;
    Canvas _canvas;
    int _savedLayerId, _savedOrder;
    bool _sortingChanged;

    // Input: d-pad/keys navigate (selection mode); stick/mouse drive a cursor (pointer mode).
    InputAction _move;
    InputActionAsset _moveAsset;
    InputActionReference _moveRef, _savedMove;
    InputSystemUIInputModule _module;
    ModalFocus _modal;
    bool _pointerMode;
    bool _pressedOutside;
    Vector2 _lastPointer;
    Selectable _hoverSel;
    const float StickThreshold = 0.2f;

    void OnEnable()
    {
        // The Overlay Canvas hides itself with a zero scale; make it visible while the phone is open.
        var canvas = GetComponentInParent<Canvas>();
        _rootCanvas = canvas != null ? canvas.rootCanvas.transform : null;

        // The apartment's Cellphone / Bowl Front sprites sit on the Foreground sorting layer, which beats the
        // canvas's Default layer regardless of order. Lift the canvas above them while the phone is open.
        _canvas = canvas != null ? canvas.rootCanvas : null;
        if (_canvas != null && !_sortingChanged)
        {
            _savedLayerId = _canvas.sortingLayerID;
            _savedOrder = _canvas.sortingOrder;
            _canvas.sortingLayerName = "Foreground";
            _canvas.sortingOrder = 200;
            _sortingChanged = true;
        }
        if (_rootCanvas != null && _rootCanvas.localScale == Vector3.zero)
        {
            _savedRootScale = _rootCanvas.localScale;
            _rootCanvas.localScale = Vector3.one;
            _scaledRoot = true;
        }
        _openedMessage = -1;
        IsOpen = true;
        BillsSignals.BillPaid += OnBillPaid;
        _modal = GetComponent<ModalFocus>();
        if (_modal != null) _modal.manualNavigation = true; // this script owns the navigation chain
        BeginInput();
        Show(Screen.Home);
    }

    void OnDisable()
    {
        IsOpen = false;
        LastClosed = Time.unscaledTime;
        BillsSignals.BillPaid -= OnBillPaid;
        EndInput();
        if (_sortingChanged && _canvas != null)
        {
            _canvas.sortingLayerID = _savedLayerId;
            _canvas.sortingOrder = _savedOrder;
        }
        _sortingChanged = false;
        if (_scaledRoot && _rootCanvas != null) _rootCanvas.localScale = _savedRootScale;
        _scaledRoot = false;
    }

    void Update()
    {
        var input = GameInput.Instance;
        if (input == null) return;

        if (input.CancelPressed) { Back(); return; }
        UpdateInputMode(input);
        if (this == null || !isActiveAndEnabled) return;
        UpdateClickOutside(input);
    }

    // ---- Input modes ----

    void BeginInput()
    {
        // The stick must drive the cursor, not the selection: while the phone is open the UI module only
        // sees d-pad / arrows / WASD for navigation.
        // An InputActionReference can only point at an action that lives in an asset, so build a tiny runtime one.
        _moveAsset = ScriptableObject.CreateInstance<InputActionAsset>();
        _moveAsset.name = "PhoneInput";
        _move = _moveAsset.AddActionMap("Phone").AddAction("Move", InputActionType.Value);
        _move.expectedControlType = "Vector2";
        _move.AddBinding("<Gamepad>/dpad");
        _move.AddCompositeBinding("2DVector")
             .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
             .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        _move.AddCompositeBinding("2DVector")
             .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
             .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        _move.Enable();

        _module = EventSystem.current != null ? EventSystem.current.currentInputModule as InputSystemUIInputModule : null;
        if (_module != null)
        {
            _savedMove = _module.move;
            _moveRef = InputActionReference.Create(_move);
            _module.move = _moveRef;
        }

        var input = GameInput.Instance;
        _lastPointer = input != null ? input.PointerPosition : Vector2.zero;
        _pressedOutside = false;
        SetPointerMode(false);
    }

    void EndInput()
    {
        if (_module != null) _module.move = _savedMove;
        _module = null;
        _savedMove = null;
        if (_moveRef != null) { Destroy(_moveRef); _moveRef = null; }
        if (_move != null) { _move.Disable(); _move = null; }
        if (_moveAsset != null) { Destroy(_moveAsset); _moveAsset = null; }

        SetPointerMode(false);
        _hoverSel = null;
    }

    void SetPointerMode(bool on)
    {
        _pointerMode = on;
        GameInput.ShowCursorInModal = on && GameInput.Instance != null && GameInput.Instance.UsingVirtualCursor;
        if (_modal != null) _modal.pointerMode = on;
    }

    void UpdateInputMode(GameInput input)
    {
        var es = EventSystem.current;

        if (_move != null && _move.WasPressedThisFrame())
        {
            // Back to d-pad: hide the cursor and make sure something is selected.
            SetPointerMode(false);
            if (es != null && (es.currentSelectedGameObject == null || !es.currentSelectedGameObject.activeInHierarchy))
                Select(CurrentDefaultTarget());
        }

        bool stickMoved = input.CursorStick.magnitude > StickThreshold;
        bool mouseMoved = !input.UsingVirtualCursor && input.PointerPosition != _lastPointer;
        _lastPointer = input.PointerPosition;
        if (stickMoved || mouseMoved) SetPointerMode(true);
        if (_pointerMode) GameInput.ShowCursorInModal = input.UsingVirtualCursor;

        // The stick cursor isn't seen by the EventSystem, so make hover select what's under it.
        if (_pointerMode && input.UsingVirtualCursor && es != null)
        {
            var under = input.SelectableUnderPointer();
            if (under != null && !under.transform.IsChildOf(transform)) under = null;
            if (under != _hoverSel)
            {
                _hoverSel = under;
                es.SetSelectedGameObject(under != null ? under.gameObject : null);
            }
        }
    }

    Selectable CurrentDefaultTarget()
    {
        switch (Current)
        {
            case Screen.Bills: return FirstPay();
            case Screen.Mail: return FirstSelectable(messagesContainer);
            case Screen.MailView: return viewMessageBack;
            default: return homeFirst;
        }
    }

    // Press and release both outside the phone's frame (mouse or stick cursor) hides the phone.
    void UpdateClickOutside(GameInput input)
    {
        if (input.ActivatePressed)
            _pressedOutside = _pointerMode && IsOutsidePhone(input.PointerPosition);

        if (input.ActivateReleased)
        {
            bool close = _pressedOutside && _pointerMode && IsOutsidePhone(input.PointerPosition);
            _pressedOutside = false;
            if (close) gameObject.SetActive(false);
        }
    }

    bool IsOutsidePhone(Vector2 screenPoint)
    {
        var rect = transform as RectTransform;
        if (rect == null) return false;
        var canvas = GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null && canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.rootCanvas.worldCamera;
        return !RectTransformUtility.RectangleContainsScreenPoint(rect, screenPoint, cam);
    }

    // ---- Button hooks (wired in the scene) ----

    public void OpenBills() => Show(Screen.Bills);
    public void OpenMail() => Show(Screen.Mail);

    public void Back()
    {
        switch (Current)
        {
            case Screen.MailView: Show(Screen.Mail); break;
            case Screen.Bills:
            case Screen.Mail: Show(Screen.Home); break;
            default: gameObject.SetActive(false); break;
        }
    }

    // ---- Screens ----

    public void Show(Screen screen)
    {
        var previous = Current;
        Current = screen;

        if (homeScreen) homeScreen.SetActive(screen == Screen.Home);
        if (billsScreen) billsScreen.SetActive(screen == Screen.Bills);
        if (mailScreen) mailScreen.SetActive(screen == Screen.Mail || screen == Screen.MailView);
        if (messagesList) messagesList.SetActive(screen == Screen.Mail);
        if (viewMessage) viewMessage.SetActive(screen == Screen.MailView);

        Selectable target = null;
        switch (screen)
        {
            case Screen.Home:
                LinkHome();
                target = homeFirst;
                break;
            case Screen.Bills:
                BuildBills();
                LinkBills();
                target = FirstPay();
                StartCoroutine(LogBillsNextFrame());
                break;
            case Screen.Mail:
                if (previous != Screen.MailView) BuildMail();
                LinkMail();
                var openedRow = RowFor(_openedMessage);
                target = openedRow != null ? openedRow.button : FirstSelectable(messagesContainer);
                break;
            case Screen.MailView:
                target = viewMessageBack;
                break;
        }
        Select(target);
    }

    // ---- Navigation (set explicitly; the Phone's ModalFocus has manualNavigation on) ----

    Selectable FindHeader(GameObject screen, ref Selectable cached)
    {
        if (cached == null && screen != null)
        {
            var t = screen.transform.Find("Header");
            if (t != null) cached = t.GetComponent<Selectable>();
        }
        return cached;
    }

    static void SetNav(Selectable s, Selectable up, Selectable down, Selectable left = null, Selectable right = null)
    {
        if (s == null) return;
        s.navigation = new Navigation
        {
            mode = Navigation.Mode.Explicit,
            selectOnUp = up, selectOnDown = down, selectOnLeft = left, selectOnRight = right
        };
    }

    // Vertical chain top -> bottom; each item's missing neighbour is itself so selection never drops out.
    static void Chain(List<Selectable> items)
    {
        for (int i = 0; i < items.Count; i++)
            SetNav(items[i], i > 0 ? items[i - 1] : items[i], i < items.Count - 1 ? items[i + 1] : items[i]);
    }

    void LinkHome()
    {
        if (homeMail == null && homeScreen != null)
        {
            var t = homeScreen.transform.Find("Mail");
            if (t != null) homeMail = t.GetComponent<Selectable>();
        }
        SetNav(homeMail, homeMail, homeMail, homeMail, homeFirst);
        SetNav(homeFirst, homeFirst, homeFirst, homeMail, homeFirst);
    }

    void LinkBills()
    {
        var chain = new List<Selectable>();
        var back = FindHeader(billsScreen, ref billsBack);
        if (back != null) chain.Add(back);
        foreach (var b in _bills)
            if (b != null && b.payButton != null && b.payButton.IsInteractable()) chain.Add(b.payButton);
        Chain(chain);
    }

    void LinkMail()
    {
        var chain = new List<Selectable>();
        var back = FindHeader(mailScreen, ref mailBack);
        if (back != null) chain.Add(back);
        foreach (var r in _rows)
            if (r != null && r.button != null) chain.Add(r.button);
        Chain(chain);
        SetNav(viewMessageBack, viewMessageBack, viewMessageBack);
    }

    Selectable FirstPay()
    {
        foreach (var b in _bills)
            if (b != null && b.payButton != null && b.payButton.IsInteractable()) return b.payButton;
        return FindHeader(billsScreen, ref billsBack);
    }

    // After a payment that row's Pay stops being selectable: rebuild the chain and move on to the next row.
    void OnBillPaid(int id)
    {
        if (Current != Screen.Bills) return;
        int paidIndex = _bills.FindIndex(b => b != null && b.id == id);
        LinkBills();

        Selectable next = null;
        for (int i = paidIndex + 1; i < _bills.Count && next == null; i++)
            if (_bills[i] != null && _bills[i].payButton != null && _bills[i].payButton.IsInteractable()) next = _bills[i].payButton;
        for (int i = paidIndex - 1; i >= 0 && next == null; i--)
            if (_bills[i] != null && _bills[i].payButton != null && _bills[i].payButton.IsInteractable()) next = _bills[i].payButton;
        Select(next != null ? next : FindHeader(billsScreen, ref billsBack));
    }

    System.Collections.IEnumerator LogBillsNextFrame()
    {
        yield return null; // layout has run
        var sb = new System.Text.StringBuilder($"[Phone] {_bills.Count} bill rows:");
        foreach (var b in _bills)
        {
            if (b == null || b.payButton == null) continue;
            sb.Append($"\n  {b.title} paid={b.isPaid()} interactable={b.payButton.IsInteractable()} hidden={ModalFocus.WhyHidden(b.payButton) ?? "no"} y={b.payButton.transform.position.y:0.00}");
        }
        Debug.Log(sb.ToString());
    }

    static void Select(Selectable s)
    {
        var es = EventSystem.current;
        if (es != null) es.SetSelectedGameObject(s != null ? s.gameObject : null);
    }

    static Selectable FirstSelectable(Transform root)
    {
        if (root == null) return null;
        foreach (var s in root.GetComponentsInChildren<Selectable>(false))
            if (s.IsInteractable()) return s;
        // Fall back to a disabled-but-present control so back/nav still has an anchor.
        return root.GetComponentInChildren<Selectable>(false);
    }

    static void Clear(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            var child = root.GetChild(i);
            child.SetParent(null);
            Destroy(child.gameObject);
        }
    }

    // ---- Bills ----

    void BuildBills()
    {
        if (billsContainer == null || billPrefab == null) return;
        var content = PhoneScrollList.Prepare(billsContainer);
        Clear(content);
        _bills.Clear();

        var text = Resources.Load<TextAsset>("bills");
        var bills = text != null ? JSON.Parse(text.ToString())?["bills"] : null;
        if (bills == null) return;

        for (int i = 0; i < bills.Count; i++)
        {
            var bill = Instantiate(billPrefab, content);
            bill.name = "Bill " + bills[i]["title"];
            bill.Init(i);
            PhoneScrollList.Register(bill, bill.payButton);
            _bills.Add(bill);
        }
    }

    // ---- Mail ----

    JSONNode MailData()
    {
        if (_mail != null) return _mail;
        var text = Resources.Load<TextAsset>("mail");
        _mail = text != null ? JSON.Parse(text.ToString())["mail"] : null;
        return _mail;
    }

    void BuildMail()
    {
        if (messagesContainer == null || messagePrefab == null) return;
        var content = PhoneScrollList.Prepare(messagesContainer);
        Clear(content);
        _rows.Clear();

        var mail = MailData();
        if (mail == null) return;

        int day = PlayerPrefs.GetInt("current level", 0);
        // Newest first.
        for (int i = mail.Count - 1; i >= 0; i--)
        {
            if (mail[i]["unlockDay"].AsInt > day) continue;
            var row = Instantiate(messagePrefab, content);
            row.name = "Message " + i;
            row.Setup(i, mail[i]["sender"], mail[i]["subject"], PlayerPrefs.GetInt("mail" + i, 0) == 1, OpenMessage);
            PhoneScrollList.Register(row, row.button);
            _rows.Add(row);
        }
    }

    PhoneMessageRow RowFor(int index)
    {
        foreach (var r in _rows)
            if (r != null && r.Index == index) return r;
        return null;
    }

    void OpenMessage(int index)
    {
        var mail = MailData();
        if (mail == null || index < 0 || index >= mail.Count) return;

        _openedMessage = index;
        PlayerPrefs.SetInt("mail" + index, 1);

        if (viewInfo) viewInfo.text = "From: " + mail[index]["sender"] + "  |  Day " + mail[index]["unlockDay"].AsInt;
        if (viewBody) viewBody.text = "<b>" + mail[index]["subject"] + "</b>\n\n" + mail[index]["body"];

        var row = RowFor(index);
        if (row != null) row.Setup(index, mail[index]["sender"], mail[index]["subject"], true, OpenMessage);

        Show(Screen.MailView);
    }
}
