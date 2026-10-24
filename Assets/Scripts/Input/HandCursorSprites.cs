using UnityEngine;

public enum CursorState { Point, Hover, Tap, Grab }

/// <summary>
/// Sprites for the stick-driven hand cursor. Loaded from Resources/HandCursorSprites by VirtualCursor.
/// </summary>
[CreateAssetMenu(menuName = "Unsavory/Hand Cursor Sprites", fileName = "HandCursorSprites")]
public class HandCursorSprites : ScriptableObject
{
    public Sprite point;
    public Sprite hover;
    public Sprite tap;
    public Sprite grab;

    [Tooltip("Cursor image size in screen pixels.")]
    public Vector2 size = new Vector2(96, 96);
    [Tooltip("Normalized (0-1) point inside the sprite that sits on the pointer position, e.g. the fingertip.")]
    public Vector2 hotspot = new Vector2(0.5f, 0.5f);

    public Sprite For(CursorState state)
    {
        Sprite s = state switch
        {
            CursorState.Hover => hover,
            CursorState.Tap => tap,
            CursorState.Grab => grab,
            _ => point,
        };
        return s != null ? s : point;
    }
}
