using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Put on any UI panel with buttons. While the panel is actually visible (panels here are often
/// hidden by animation, not deactivated) the topmost one owns gamepad focus: its first button is
/// selected, Cancel presses its close button, and the world router is blocked (HasFocus).
/// </summary>
public class ModalFocus : MonoBehaviour
{
    [Tooltip("Optional. Defaults to the first visible button in the panel.")]
    public Selectable firstSelected;
    [Tooltip("Cancel (B / Esc) presses the panel's Close/No button.")]
    public bool allowCancel;
    [Tooltip("The panel sets Selectable.navigation itself; ModalFocus keeps focus/cancel but doesn't rebuild navigation.")]
    public bool manualNavigation;
    [Tooltip("A pointer (mouse / stick cursor) is driving selection: don't snap it back into the panel.")]
    public bool pointerMode;

    static readonly string[] CancelNames = { "Close Button", "Close", "No Button", "No" };
    static readonly List<ModalFocus> All = new List<ModalFocus>();
    static ModalFocus _top;
    static bool _owned;
    static readonly Vector3[] Corners = new Vector3[4];

    // Buttons animate (alpha pulses, slide in/out), so a modal stays "visible" for a short
    // grace period after its last visible frame instead of dropping focus on a single dip.
    const float HideGrace = 0.3f;
    float _visibleSince = -1f;
    float _lastSeen = -1f;

    /// <summary>True while a visible modal owns input; world interaction should be ignored.</summary>
    public static bool HasFocus => _top != null;

    void OnEnable() { All.Add(this); }

    void OnDisable()
    {
        if (_navOwner == this) RestoreNavigation();
        All.Remove(this);
        _visibleSince = -1f;
        _lastSeen = -1f;
        if (_top == this) _top = null; // Tick clears the stale selection
    }

    /// <summary>Called once per frame by GameInput.</summary>
    public static void Tick(bool cancelPressed)
    {
        var es = EventSystem.current;
        if (es == null) { _top = null; _owned = false; return; }

        ModalFocus best = null;
        foreach (var m in All)
        {
            if (m.FirstVisible() != null) m._lastSeen = Time.unscaledTime;
            // A switched-off canvas is hidden at once; the grace period is only for animation dips.
            var canvas = m.GetComponentInParent<Canvas>();
            bool canvasOff = canvas != null && !canvas.rootCanvas.enabled;
            bool visible = !canvasOff && m._lastSeen >= 0f && Time.unscaledTime - m._lastSeen <= HideGrace;
            if (!visible) { m._visibleSince = -1f; m._lastSeen = -1f; continue; }
            if (m._visibleSince < 0f) m._visibleSince = Time.unscaledTime;
            if (best == null || m._visibleSince >= best._visibleSince) best = m;
        }

        if (best != _top)
        {
            _top = best;
            Debug.Log($"[ModalFocus] top = {(_top ? _top.name : "none")} (visible: {DescribeVisible()})");
            if (_top != null) es.SetSelectedGameObject(_top.Pick()?.gameObject);
            else if (_owned) es.SetSelectedGameObject(null);
            _owned = _top != null;
        }
        else if (_top != null)
        {
            var cur = es.currentSelectedGameObject;
            var sel = cur != null ? cur.GetComponent<Selectable>() : null;
            if (!_top.pointerMode && (sel == null || !sel.isActiveAndEnabled || !sel.transform.IsChildOf(_top.transform)))
            {
                if (cur != null && Time.unscaledTime - _lastLeaveLog > 1f)
                {
                    _lastLeaveLog = Time.unscaledTime;
                    Debug.Log($"[ModalFocus] selection left {_top.name} -> {cur.name}, snapping back");
                }
                es.SetSelectedGameObject(_top.Pick()?.gameObject);
            }
        }
        else if (_owned)
        {
            es.SetSelectedGameObject(null);
            _owned = false;
        }

        // Keep stick/d-pad moves inside the top modal. Automatic navigation searches every Selectable in the
        // scene, so it would otherwise jump to buttons of panels that are only hidden by animation.
        if (_top != _navOwner) RestoreNavigation();
        if (_top != null && !_top.manualNavigation) _top.ApplyLocalNavigation();

        if (cancelPressed && _top != null && _top.allowCancel)
            _top.PressCancel();
    }

    static string DescribeVisible()
    {
        var sb = new System.Text.StringBuilder();
        foreach (var m in All)
            if (m._visibleSince >= 0f) sb.Append(m.name).Append('@').Append(m._visibleSince.ToString("0.0")).Append(' ');
        return sb.ToString();
    }

    static float _lastLeaveLog;
    static ModalFocus _navOwner;
    static readonly Dictionary<Selectable, Navigation> _savedNav = new Dictionary<Selectable, Navigation>();
    static readonly List<Selectable> _visibleSelectables = new List<Selectable>();
    int _navSignature;

    static void RestoreNavigation()
    {
        foreach (var kv in _savedNav)
            if (kv.Key != null) kv.Key.navigation = kv.Value;
        _savedNav.Clear();
        _navOwner = null;
    }

    void ApplyLocalNavigation()
    {
        _visibleSelectables.Clear();
        int signature = 17;
        foreach (var sel in GetComponentsInChildren<Selectable>(false))
        {
            if (!IsVisible(sel)) continue;
            _visibleSelectables.Add(sel);
            signature = unchecked(signature * 31 + sel.GetHashCode());
        }
        if (_navOwner == this && signature == _navSignature) return;

        RestoreNavigation();
        _navOwner = this;
        _navSignature = signature;

        foreach (var sel in _visibleSelectables)
        {
            _savedNav[sel] = sel.navigation;
            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnLeft = Neighbour(sel, Vector2.left),
                selectOnRight = Neighbour(sel, Vector2.right),
                selectOnUp = Neighbour(sel, Vector2.up),
                selectOnDown = Neighbour(sel, Vector2.down),
            };
            sel.navigation = nav;
        }
    }

    // Nearest visible sibling in a direction: mostly along the direction, little sideways drift.
    static Selectable Neighbour(Selectable from, Vector2 dir)
    {
        Selectable best = null;
        float bestScore = float.MaxValue;
        Vector2 origin = from.transform.position;
        foreach (var other in _visibleSelectables)
        {
            if (other == from) continue;
            Vector2 delta = (Vector2)other.transform.position - origin;
            float along = Vector2.Dot(delta, dir);
            if (along <= 0.001f) continue;
            float lateral = Mathf.Abs(delta.x * dir.y - delta.y * dir.x);
            float score = along + 2f * lateral;
            if (score < bestScore) { bestScore = score; best = other; }
        }
        return best;
    }

    Selectable FirstVisible()
    {
        foreach (var s in GetComponentsInChildren<Selectable>(false))
            if (IsVisible(s)) return s;
        return null;
    }

    Selectable Pick()
    {
        if (firstSelected != null && firstSelected.transform.IsChildOf(transform) && IsVisible(firstSelected))
            return firstSelected;
        return FirstVisible();
    }

    void PressCancel()
    {
        foreach (var s in GetComponentsInChildren<Button>(false))
        {
            if (!s.isActiveAndEnabled || !s.IsInteractable()) continue;
            foreach (var n in CancelNames)
                if (s.name == n) { s.onClick.Invoke(); return; }
        }
    }

    /// <summary>Why IsVisible would reject this selectable (null if it passes). For diagnostics.</summary>
    public static string WhyHidden(Selectable s)
    {
        if (!s.isActiveAndEnabled) return "inactive";
        if (!s.IsInteractable()) return "not interactable";
        var rt = s.transform as RectTransform;
        if (rt == null) return "no RectTransform";
        if (rt.lossyScale.x < 0.01f || rt.lossyScale.y < 0.01f) return "scale " + rt.lossyScale.x.ToString("0.0000");
        return IsVisible(s) ? null : "off-screen or alpha 0";
    }

    static bool IsVisible(Selectable s)
    {
        if (!s.isActiveAndEnabled || !s.IsInteractable()) return false;
        // Panels often hide themselves by switching their Canvas component off; the buttons are still active but unseen.
        var rootCanvas = s.GetComponentInParent<Canvas>();
        if (rootCanvas != null && !rootCanvas.rootCanvas.enabled) return false;

        var rt = s.transform as RectTransform;
        if (rt == null) return false;
        if (rt.lossyScale.x < 0.01f || rt.lossyScale.y < 0.01f) return false;

        float alpha = 1f;
        foreach (var cg in s.GetComponentsInParent<CanvasGroup>())
        {
            alpha *= cg.alpha;
            if (cg.ignoreParentGroups) break;
        }
        if (alpha < 0.01f) return false;

        var canvas = s.GetComponentInParent<Canvas>();
        Camera cam = null;
        if (canvas != null)
        {
            canvas = canvas.rootCanvas;
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = canvas.worldCamera;
        }

        rt.GetWorldCorners(Corners);
        Vector2 center = Vector2.zero;
        for (int i = 0; i < 4; i++) center += RectTransformUtility.WorldToScreenPoint(cam, Corners[i]);
        center *= 0.25f;
        return center.x >= 0 && center.x <= Screen.width && center.y >= 0 && center.y <= Screen.height;
    }
}
