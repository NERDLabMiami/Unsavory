using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Single entry point for gameplay input. Wraps the "Game" map of the project-wide
/// actions asset (InputSystem.actions) so scripts never poll devices directly.
/// Created automatically before the first scene loads.
/// </summary>
[DefaultExecutionOrder(-100)]
public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    InputAction _point, _cursor, _activate, _scrub, _prevRoom, _nextRoom, _pause, _cancel;
    InputAction _navigate, _confirm, _hotSauce, _swipe;
    Vector2Int _navHeld;
    VirtualCursor _virtualCursor;
    Vector2 _lastPointer;
    float _dragScrub;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;
        var go = new GameObject("GameInput");
        DontDestroyOnLoad(go);
        go.AddComponent<GameInput>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        var map = InputSystem.actions.FindActionMap("Game", true);
        map.Enable();
        _point     = map.FindAction("Point", true);
        _cursor    = map.FindAction("Cursor", true);
        _activate  = map.FindAction("Activate", true);
        _scrub     = map.FindAction("Scrub", true);
        _prevRoom  = map.FindAction("PreviousRoom", true);
        _nextRoom  = map.FindAction("NextRoom", true);
        _pause     = map.FindAction("Pause", true);
        _navigate  = map.FindAction("Navigate", true);
        _confirm   = map.FindAction("Confirm", true);
        _hotSauce  = map.FindAction("HotSauce", true);
        _swipe     = map.FindAction("Swipe", true);
        _cancel    = InputSystem.actions.FindAction("UI/Cancel", true);

        _virtualCursor = new VirtualCursor(transform);
    }

    void Update()
    {
        UpdateDragScrub();
        UpdateNavigate();
        ModalFocus.Tick(_cancel.WasPressedThisFrame());
        // With the ingredient grid up, the stick navigates the grid instead of driving the cursor.
        _virtualCursor.Tick(_point.ReadValue<Vector2>(), GridMode ? Vector2.zero : _cursor.ReadValue<Vector2>(),
                            !GridMode && _activate.WasPressedThisFrame() && _activate.activeControl?.device is Gamepad,
                            !ModalFocus.HasFocus || ShowCursorInModal, CurrentCursorState(),
                            HandHiddenInScene);
    }

    /// <summary>Set by WorldInputRouter2D: the pointer is over a world interactable.</summary>
    public static bool PointerOverInteractable;
    /// <summary>Set by WorldInputRouter2D when a click (non-drag release) fires.</summary>
    public static float LastTapTime = -10f;
    const float TapFlashSeconds = 0.12f;

    // The hand cursor isn't wanted in gameplay; other scenes show it for the stick cursor.
    static bool HandHiddenInScene => SceneManager.GetActiveScene().name == "Game";

    CursorState CurrentCursorState()
    {
        if (_activate.IsPressed()) return CursorState.Grab;
        if (Time.unscaledTime - LastTapTime < TapFlashSeconds) return CursorState.Tap;
        if (PointerOverInteractable || SelectableUnderPointer() != null) return CursorState.Hover;
        return CursorState.Point;
    }

    // Stick/d-pad/arrows become one step per push, so holding a direction doesn't skip across the grid.
    void UpdateNavigate()
    {
        var v = _navigate.ReadValue<Vector2>();
        Vector2Int dir = Vector2Int.zero;
        if (v.magnitude > 0.5f)
            dir = Mathf.Abs(v.x) >= Mathf.Abs(v.y)
                ? new Vector2Int(v.x > 0 ? 1 : -1, 0)
                : new Vector2Int(0, v.y > 0 ? 1 : -1);
        NavigateStep = dir != Vector2Int.zero && dir != _navHeld ? dir : Vector2Int.zero;
        _navHeld = dir;
    }

    void UpdateDragScrub()
    {
        var p = _point.ReadValue<Vector2>();
        float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
        // Full deflection = dragging half a screen width per second.
        _dragScrub = (!_virtualCursor.Active && _activate.IsPressed())
            ? Mathf.Clamp((p.x - _lastPointer.x) / dt / (Screen.width * 0.5f), -1f, 1f)
            : 0f;
        _lastPointer = p;
    }

    /// <summary>Pointer position in screen pixels (mouse/touch, or the stick-driven cursor).</summary>
    public Vector2 PointerPosition => _virtualCursor.Position;
    /// <summary>True while the stick-driven cursor is the active pointer.</summary>
    public bool UsingVirtualCursor => _virtualCursor.Active;

    public bool ActivatePressed  => _activate.WasPressedThisFrame();
    public bool ActivateReleased => _activate.WasReleasedThisFrame();
    public bool ActivateHeld     => _activate.IsPressed();

    /// <summary>Nose scrub axis, -1..1: A/D or right stick, or a horizontal mouse/touch drag while Activate is held.</summary>
    public float Scrub
    {
        get
        {
            float axis = _scrub.ReadValue<float>();
            return axis != 0f ? axis : _dragScrub;
        }
    }

    /// <summary>A modal (the Phone) wants the stick cursor reticle visible even though it owns focus.</summary>
    public static bool ShowCursorInModal;

    /// <summary>Raw stick value that drives the virtual cursor.</summary>
    public Vector2 CursorStick => _cursor.ReadValue<Vector2>();

    /// <summary>True while the Game scene's ingredient grid owns the stick/d-pad and gamepad confirm.</summary>
    public static bool GridMode;

    /// <summary>This frame's grid step (x right, y up), or zero. One step per push.</summary>
    public Vector2Int NavigateStep { get; private set; }
    /// <summary>The direction currently held on the stick/d-pad/arrows, or zero. Unlike NavigateStep this is not a one-frame edge.</summary>
    public Vector2Int NavigateHeld => _navHeld;
    public bool ConfirmPressed   => _confirm.WasPressedThisFrame();
    public bool HotSaucePressed  => _hotSauce.WasPressedThisFrame();
    public bool SwipePressed     => _swipe.WasPressedThisFrame();

    /// <summary>True when the Activate press came from a mouse/touch pointer rather than a button.</summary>
    public bool ActivateFromPointer => _activate.activeControl?.device is Pointer;

    public bool PreviousRoomPressed => _prevRoom.WasPressedThisFrame();
    public bool NextRoomPressed     => _nextRoom.WasPressedThisFrame();
    public bool PausePressed        => _pause.WasPressedThisFrame();
    public bool CancelPressed       => _cancel.WasPressedThisFrame();

    static readonly List<RaycastResult> _uiHits = new List<RaycastResult>();

    /// <summary>
    /// Clicks the uGUI button under the pointer. Mouse/touch clicks already reach uGUI through the EventSystem;
    /// the stick-driven cursor does not, so the router calls this for it (HUD pause/quit buttons, etc.).
    /// </summary>
    public bool TryClickUI()
    {
        var es = EventSystem.current;
        if (es == null) return false;

        var data = new PointerEventData(es) { position = PointerPosition, button = PointerEventData.InputButton.Left };
        _uiHits.Clear();
        es.RaycastAll(data, _uiHits);
        foreach (var hit in _uiHits)
        {
            var target = ExecuteEvents.GetEventHandler<IPointerClickHandler>(hit.gameObject);
            if (target == null) continue;
            data.pointerPress = target;
            data.pointerCurrentRaycast = hit;
            return ExecuteEvents.Execute(target, data, ExecuteEvents.pointerClickHandler);
        }
        return false;
    }

    /// <summary>Converts the current pointer position to a world point for a 2D hit test.</summary>
    public Vector2 PointerWorld(Camera cam)
    {
        var p = PointerPosition;
        // Distance to the z=0 plane, so this works for orthographic and perspective cameras.
        float z = cam.orthographic ? 0f : -cam.transform.position.z;
        return cam.ScreenToWorldPoint(new Vector3(p.x, p.y, z));
    }

    /// <summary>The topmost interactable uGUI Selectable under the pointer, or null.</summary>
    public Selectable SelectableUnderPointer()
    {
        var es = EventSystem.current;
        if (es == null) return null;

        var data = new PointerEventData(es) { position = PointerPosition };
        _uiHits.Clear();
        es.RaycastAll(data, _uiHits);
        foreach (var hit in _uiHits)
        {
            var sel = hit.gameObject.GetComponentInParent<Selectable>();
            if (sel != null && sel.IsInteractable()) return sel;
            if (hit.gameObject.GetComponentInParent<Graphic>() != null && sel == null) continue;
        }
        return null;
    }
}
