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

    void Update()
    {
        var input = GameInput.Instance;
        if (input == null) return;

        bool blocked = ModalFocus.HasFocus ||
                       (GameplayGate.Instance != null && !GameplayGate.Instance.InteractionEnabled);

        if (input.ActivatePressed)
        {
            // Grid mode: gamepad/keyboard confirm belongs to the ingredient grid, only a real pointer clicks the world.
            _armed = !blocked && !(GameInput.GridMode && !input.ActivateFromPointer);
            _pressPos = input.PointerPosition;
        }
        if (blocked) return;

        if (!cam) cam = Camera.main;

        if (nose) nose.SubmitScrub(input.Scrub, Time.deltaTime);

        if (!input.ActivateReleased) return;
        bool armed = _armed;
        _armed = false;
        if (!armed) return;

        // A drag (e.g. wiping the nose with the mouse) is not a click on whatever is under the release point.
        if (Vector2.Distance(_pressPos, input.PointerPosition) > Screen.height * 0.03f) return;

        // Stick cursor: it isn't a real pointer, so press any uGUI button under it (HUD pause/quit) before the world.
        if (input.UsingVirtualCursor && input.TryClickUI()) return;

        int layers = (mask.value == 0) ? ~0 : mask.value;
        var hit = Physics2D.OverlapPoint(input.PointerWorld(cam), layers);
        if (!hit) return;

        var target = hit.GetComponentInParent<IWorldInteractable>() as Component;
        if (target == null) return;

        // One object can carry several interactables (the Tortilla Tray adds the tortilla AND unlocks the trays).
        foreach (var interactable in target.GetComponents<IWorldInteractable>())
            interactable.Interact();
    }
}
