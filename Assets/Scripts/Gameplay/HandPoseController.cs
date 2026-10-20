using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Drives the Player's hands. Sprites swap while the mapped button is pressed: Confirm (A) grabs,
/// HotSauce (right shoulder) squeezes the sauce, Swipe (left shoulder) wipes the nose.
/// Hands sway gently on Y at rest, the grab lunges forward and back, and the nose wipe rises, swipes right and returns.
/// Hand positions are composed each frame as rest position + action offset + sway, so animations never drift.
/// </summary>
public class HandPoseController : MonoBehaviour
{
    public enum ScaleMode
    {
        [Tooltip("Same scale as the idle sprite - right when the art was exported at the same scale.")]
        KeepIdleScale,
        [Tooltip("Scale so the pose has the same world height as the idle sprite.")]
        FitHeight
    }

    [Serializable]
    public class Pose
    {
        [Tooltip("Hand whose sprite is swapped.")]
        public SpriteRenderer hand;
        public Sprite sprite;
        public ScaleMode scaleMode = ScaleMode.KeepIdleScale;
        [FormerlySerializedAs("sizeMultiplier")]
        [Tooltip("Multiplies the resulting scale (tune if this pose looks bigger/smaller than the idle sprite).")]
        public float scaleMultiplier = 1f;
        [Tooltip("Local position nudge while this pose is showing, to compensate for a different sprite pivot/canvas.")]
        public Vector2 localOffset;
    }

    [Header("Poses")]
    public Pose grab;
    public Pose squeezeSauce;
    public Pose wipeNose;
    [Tooltip("Minimum time a pose stays up so a quick tap is still visible.")]
    public float holdTime = 0.2f;

    [Header("Sway (idle)")]
    public float swayAmplitude = 0.03f;
    public float swaySpeed = 0.4f;

    [Header("Grab lunge")]
    [Tooltip("Local offset the grabbing hand lunges to.")]
    public Vector2 lungeOffset = new Vector2(0.35f, 0f);
    public float lungeOut = 0.07f;
    public float lungeBack = 0.12f;

    [Header("Nose wipe")]
    [Tooltip("Local offset the hand rises to before swiping.")]
    public Vector2 wipeRise = new Vector2(0f, 0.4f);
    [Tooltip("Local offset the hand swipes to (to the right of the rise).")]
    public Vector2 wipeSwipe = new Vector2(0.9f, 0.4f);
    public float wipeRiseTime = 0.08f;
    public float wipeSwipeTime = 0.12f;
    public float wipeReturnTime = 0.15f;

    class HandState
    {
        public SpriteRenderer renderer;
        public Transform t;
        public Sprite idleSprite;
        public Vector3 idleScale;
        public Vector3 restPosition;
        public float idleHeight;
        public float phase;
        public Pose current;
        public float until;
        public bool sequence;
        public Vector2 offset;
    }

    readonly Dictionary<SpriteRenderer, HandState> _hands = new Dictionary<SpriteRenderer, HandState>();

    void OnEnable()
    {
        _hands.Clear();
        Register(grab);
        Register(squeezeSauce);
        Register(wipeNose);
    }

    void Register(Pose pose)
    {
        if (pose == null || !pose.hand || _hands.ContainsKey(pose.hand)) return;
        var r = pose.hand;
        _hands[r] = new HandState
        {
            renderer = r,
            t = r.transform,
            idleSprite = r.sprite,
            idleScale = r.transform.localScale,
            restPosition = r.transform.localPosition,
            idleHeight = r.sprite ? r.sprite.bounds.size.y * r.transform.localScale.y : 0f,
            phase = _hands.Count * 1.7f
        };
    }

    void Update()
    {
        var input = GameInput.Instance;
        if (input != null)
        {
            bool blocked = ModalFocus.HasFocus ||
                           (GameplayGate.Instance != null && !GameplayGate.Instance.InteractionEnabled);
            if (!blocked)
            {
                if (input.ConfirmPressed)  Grab();
                if (input.HotSaucePressed) Squeeze();
                if (input.SwipePressed)    Wipe();
            }
        }

        float time = Time.unscaledTime;
        foreach (var s in _hands.Values)
        {
            if (s.current != null && !s.sequence && time >= s.until) Restore(s);
            if (!s.t) continue;
            float sway = Mathf.Sin((time * swaySpeed + s.phase) * Mathf.PI * 2f) * swayAmplitude;
            Vector2 pose = s.current != null ? s.current.localOffset : Vector2.zero;
            s.t.localPosition = s.restPosition + new Vector3(s.offset.x + pose.x, s.offset.y + pose.y + sway, 0f);
        }
    }

    void OnDisable()
    {
        foreach (var s in _hands.Values)
        {
            if (s.t) Tween.CancelAll(s.t);
            s.offset = Vector2.zero;
            Restore(s);
            if (s.t) s.t.localPosition = s.restPosition;
        }
    }

    // ---- Actions ----

    void Grab()
    {
        var s = Show(grab, Mathf.Max(holdTime, lungeOut + lungeBack));
        if (s == null) return;
        MoveOffset(s, lungeOffset, lungeOut, Tween.Ease.OutQuad,
            () => MoveOffset(s, Vector2.zero, lungeBack, Tween.Ease.InQuad, null));
    }

    void Squeeze() { Show(squeezeSauce, holdTime); }

    void Wipe()
    {
        var s = Show(wipeNose, holdTime);
        if (s == null) return;
        s.sequence = true;
        MoveOffset(s, wipeRise, wipeRiseTime, Tween.Ease.OutQuad, () =>
            MoveOffset(s, wipeSwipe, wipeSwipeTime, Tween.Ease.OutQuad, () =>
                MoveOffset(s, Vector2.zero, wipeReturnTime, Tween.Ease.OutQuad, () => Restore(s))));
    }

    // ---- Helpers ----

    HandState Show(Pose pose, float hold)
    {
        if (pose == null || !pose.hand || !pose.sprite || !_hands.TryGetValue(pose.hand, out var s)) return null;
        s.current = pose;
        s.sequence = false;
        s.until = Time.unscaledTime + hold;

        s.renderer.sprite = pose.sprite;
        float k = pose.scaleMultiplier;
        if (pose.scaleMode == ScaleMode.FitHeight && s.idleHeight > 0f && pose.sprite.bounds.size.y > 0f)
            k *= s.idleHeight / pose.sprite.bounds.size.y;
        else
            k *= s.idleScale.x;
        s.t.localScale = new Vector3(k, k, s.idleScale.z);
        return s;
    }

    // Tweens the hand's action offset (added on top of the rest position every frame). A new move replaces the old one.
    static void MoveOffset(HandState s, Vector2 to, float duration, Tween.Ease ease, Action done)
    {
        Vector2 from = s.offset;
        Tween.Run(s.t, "handOffset", duration, v => s.offset = Vector2.LerpUnclamped(from, to, v), ease, 0f, done);
    }

    static void Restore(HandState s)
    {
        s.current = null;
        s.sequence = false;
        if (!s.renderer) return;
        s.renderer.sprite = s.idleSprite;
        s.t.localScale = s.idleScale;
    }
}
