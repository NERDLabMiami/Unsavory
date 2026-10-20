
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Activates the IWorldInteractable under the pointer when Activate is released.
/// Used by both Home and Game scenes; pointer source comes from GameInput.
/// </summary>
public class WorldInputRouter2D : MonoBehaviour
{
    public Camera cam;
    public LayerMask mask;

    [Header("Game scene only")]
    public NoseWipeScript nose; // receives the Scrub axis every frame

    // A press that started while a modal/gate owned input must not fire on release,
    // e.g. pressing A on a Close button shouldn't click the world once the modal is gone.
    bool _armed;
    Vector2 _pressPos;
    IHoverable _hovered;

    void Update()
    {
        var input = GameInput.Instance;
        if (input == null) return;

        bool blocked = ModalFocus.HasFocus || PhoneController.BlocksWorld ||
                       (GameplayGate.Instance != null && !GameplayGate.Instance.InteractionEnabled);

        if (input.ActivatePressed)
        {
            // Grid mode: gamepad/keyboard confirm belongs to the ingredient grid, only a real pointer clicks the world.
            _armed = !blocked && !(GameInput.GridMode && !input.ActivateFromPointer);
            _pressPos = input.PointerPosition;
        }
        if (!cam) cam = Camera.main;

        // Hover feedback follows the pointer (mouse or stick cursor); a modal such as the Phone clears it.
        UpdateHover(input, blocked || GameInput.GridMode);
        if (blocked) return;

        if (nose) nose.SubmitScrub(input.Scrub, Time.deltaTime);

        if (!input.ActivateReleased) return;
        bool armed = _armed;
        _armed = false;
        if (!armed) return;

        // A drag (e.g. wiping the nose with the mouse) is not a click on whatever is under the release point.
        if (Vector2.Distance(_pressPos, input.PointerPosition) > Screen.height * 0.03f) return;

        // Stick cursor: it isn't a real pointer, so press any uGUI button under it (HUD pause/quit) before the world.
        if (input.UsingVirtualCursor && input.TryClickUI()) return;

        var target = InteractableUnderPointer(input);
        if (target == null) return;
        Debug.Log($"[Router] Interact {target.name} (HasFocus={ModalFocus.HasFocus}, phoneOpen={PhoneController.IsOpen}, virtualCursor={input.UsingVirtualCursor}, frame={Time.frameCount})");

        // One object can carry several interactables (the Tortilla Tray adds the tortilla AND unlocks the trays).
        foreach (var interactable in target.GetComponents<IWorldInteractable>())
            interactable.Interact();
    }

    void UpdateHover(GameInput input, bool suppress)
    {
        IHoverable next = null;
        if (!suppress && cam)
        {
            var target = InteractableUnderPointer(input);
            if (target != null) next = target.GetComponent<IHoverable>();
        }
        if (next == _hovered) return;
        _hovered?.SetHover(false);
        _hovered = next;
        _hovered?.SetHover(true);
    }

    void OnDisable()
    {
        _hovered?.SetHover(false);
        _hovered = null;
    }

    static readonly List<Collider2D> Overlaps = new List<Collider2D>();

    // Topmost interactable under the pointer (sorting layer, then order), so overlapping colliders
    // resolve the same way for hover and click.
    Component InteractableUnderPointer(GameInput input)
    {
        int layers = (mask.value == 0) ? ~0 : mask.value;
        var filter = new ContactFilter2D { useLayerMask = true, layerMask = layers, useTriggers = true };
        Overlaps.Clear();
        Physics2D.OverlapPoint(input.PointerWorld(cam), filter, Overlaps);

        Component best = null;
        long bestKey = long.MinValue;
        foreach (var col in Overlaps)
        {
            var target = col.GetComponentInParent<IWorldInteractable>() as Component;
            if (target == null) continue;

            var sr = col.GetComponentInParent<SpriteRenderer>();
            long key = sr != null ? ((long)SortingLayer.GetLayerValueFromID(sr.sortingLayerID) << 32) + sr.sortingOrder : 0;
            if (best == null || key > bestKey) { best = target; bestKey = key; }
        }
        return best;
    }
}
