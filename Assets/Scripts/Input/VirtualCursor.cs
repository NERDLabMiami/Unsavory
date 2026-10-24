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

    public Vector2 Position { get; private set; }
    public bool Active { get; private set; }

    Vector2 _lastPoint;
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
            _hand.rectTransform.sizeDelta = _sprites.size;
            _hand.rectTransform.pivot = _sprites.hotspot;
            _hand.enabled = false;
        }
    }

    public void Tick(Vector2 point, Vector2 stick, bool gamepadActivated, bool showReticle,
                     CursorState state = CursorState.Point, bool hideHand = false)
    {
        if (point != _lastPoint)
        {
            _lastPoint = point;
            Active = false;
        }

        if (stick.magnitude > StickThreshold || gamepadActivated)
            Active = true;

        if (Active)
        {
            Position += stick * (StickSpeed * Screen.height * Time.unscaledDeltaTime);
            Position = new Vector2(Mathf.Clamp(Position.x, 0, Screen.width),
                                   Mathf.Clamp(Position.y, 0, Screen.height));
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
            _hand.rectTransform.position = Position;
            return;
        }

        _reticle.enabled = show;
        if (Active) _reticle.rectTransform.position = Position;
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
