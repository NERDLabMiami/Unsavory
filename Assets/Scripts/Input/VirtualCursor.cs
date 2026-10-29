using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pointer that follows the mouse/touch, or is driven by the gamepad stick. Switches
/// source on actual input from either, and shows a hand (or fallback ring) only for the stick cursor.
/// </summary>
public class VirtualCursor
{
    // Fraction of screen height crossed per second at full stick deflection.
    const float StickSpeed = 0.85f;
    const float StickThreshold = 0.2f;
    // How fast the cursor eases toward a MoveTo target (higher = snappier).
    const float TargetSpeed = 18f;

    // Hand size/offset are authored for a 900px-tall window and scale with the real height.
    static float UiScale => Screen.height / 900f;

    public Vector2 Position { get; private set; }
    public bool Active { get; private set; }

    Vector2 _lastPoint;
    Vector2 _target;
    bool _hasTarget;
    readonly RawImage _reticle; // fallback ring when no HandCursorSprites asset exists
    readonly Image _hand;
    readonly HandCursorSprites _sprites;

    public VirtualCursor(Transform parent)
    {
        Position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        var canvasGo = new GameObject("CursorCanvas");
        canvasGo.transform.SetParent(parent, false);
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        var go = new GameObject("Reticle");
        go.transform.SetParent(canvasGo.transform, false);
        _reticle = go.AddComponent<RawImage>();
        _reticle.texture = MakeRing(48);
        _reticle.raycastTarget = false;
        _reticle.rectTransform.sizeDelta = new Vector2(48, 48);
        _reticle.enabled = false;

        _sprites = Resources.Load<HandCursorSprites>("HandCursorSprites");
        if (_sprites != null)
        {
            var handGo = new GameObject("Hand");
            handGo.transform.SetParent(canvasGo.transform, false);
            _hand = handGo.AddComponent<Image>();
            _hand.raycastTarget = false;
            _hand.preserveAspect = true;
            _hand.rectTransform.sizeDelta = _sprites.size * UiScale;
            _hand.rectTransform.pivot = _sprites.pointHotspot;
            _hand.enabled = false;
        }
    }

    /// <summary>
    /// Glides the stick cursor to a screen position and makes it the active pointer. The target is held (and can be
    /// re-set every frame to follow a moving control) until the stick or mouse moves, which cancels it at once.
    /// </summary>
    public void MoveTo(Vector2 screenPos, bool instant = false)
    {
        _target = ClampToScreen(screenPos);
        _hasTarget = true;
        Active = true;
        if (instant) Position = _target;
    }

    public void CancelTarget() => _hasTarget = false;

    public void Tick(Vector2 point, Vector2 stick, bool gamepadActivated, bool showReticle,
                     CursorState state = CursorState.Point, bool hideHand = false)
    {
        if (point != _lastPoint)
        {
            _lastPoint = point;
            Active = false;
            _hasTarget = false;
        }

        if (stick.magnitude > StickThreshold || gamepadActivated)
        {
            Active = true;
            if (stick.magnitude > StickThreshold) _hasTarget = false; // the player broke away from the snap
        }

        if (Active && _hasTarget)
        {
            Position = Vector2.Distance(Position, _target) < 0.5f
                ? _target
                : Vector2.Lerp(Position, _target, 1f - Mathf.Exp(-TargetSpeed * Time.unscaledDeltaTime));
        }
        else if (Active)
        {
            Position += stick * (StickSpeed * Screen.height * Time.unscaledDeltaTime);
            Position = ClampToScreen(Position);
        }
        else
        {
            Position = point; // keep in sync so switching to the stick doesn't jump
        }

        bool show = Active && showReticle;
        if (_hand != null)
        {
            _reticle.enabled = false;
            _hand.enabled = show && !hideHand;
            if (!_hand.enabled) return;
            var sprite = _sprites.For(state);
            if (_hand.sprite != sprite) _hand.sprite = sprite;
            _hand.rectTransform.pivot = _sprites.HotspotFor(state);
            _hand.rectTransform.sizeDelta = _sprites.size * UiScale;
            _hand.rectTransform.position = Position + _sprites.imageOffset * UiScale;
            return;
        }

        _reticle.enabled = show;
        if (Active) _reticle.rectTransform.position = Position;
    }

    // The pointer may travel until the drawn hand's centre reaches each window edge (so half the sprite can
    // hang off-screen), plus the asset's extra overscan, which keeps partly off-screen objects reachable.
    Vector2 ClampToScreen(Vector2 p)
    {
        Vector2 lo = Vector2.zero, hi = new Vector2(Screen.width, Screen.height);
        if (_sprites != null)
        {
            // Where the sprite centre sits relative to the pointer.
            var c = (_sprites.imageOffset + Vector2.Scale(new Vector2(0.5f, 0.5f) - _sprites.pointHotspot, _sprites.size)) * UiScale;
            lo += new Vector2(Mathf.Min(0, -c.x), Mathf.Min(0, -c.y)) - Vector2.one * _sprites.overscan;
            hi += new Vector2(Mathf.Max(0, -c.x), Mathf.Max(0, -c.y)) + Vector2.one * _sprites.overscan;
        }
        return new Vector2(Mathf.Clamp(p.x, lo.x, hi.x), Mathf.Clamp(p.y, lo.y, hi.y));
    }

    static Texture2D MakeRing(int size)
    {
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear };
        float c = (size - 1) * 0.5f, outer = size * 0.45f, inner = size * 0.32f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                float a = Mathf.Clamp01(outer - d) * Mathf.Clamp01(d - inner);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
        tex.Apply();
        return tex;
    }
}
