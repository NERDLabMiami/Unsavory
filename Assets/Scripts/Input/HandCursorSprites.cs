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
    [Tooltip("Normalized (0-1, from bottom-left) point inside each sprite that sits on the pointer position, i.e. the fingertip. The poses differ, so each has its own.")]
    public Vector2 pointHotspot = new Vector2(0.84f, 0.84f);
    public Vector2 hoverHotspot = new Vector2(0.82f, 0.68f);
    public Vector2 tapHotspot = new Vector2(0.76f, 0.78f);
    public Vector2 grabHotspot = new Vector2(0.78f, 0.68f);
    [Tooltip("Screen-pixel shift of the drawn hand; the pointer position is unchanged. +y moves the hand up.")]
    public Vector2 imageOffset;
    [Tooltip("Screen pixels the stick cursor may travel past each window edge, to reach partly off-screen objects.")]
    public float overscan = 100f;

    public Vector2 HotspotFor(CursorState state) => state switch
    {
        CursorState.Hover => hoverHotspot,
        CursorState.Tap => tapHotspot,
        CursorState.Grab => grabHotspot,
        _ => pointHotspot,
    };

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
