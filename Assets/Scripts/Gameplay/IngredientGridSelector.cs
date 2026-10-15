using UnityEngine;

/// <summary>
/// Gamepad/keyboard ingredient selection for the Game scene. The stick or d-pad moves a highlight over a
/// grid of trays, Confirm adds the highlighted ingredient, and the shoulder buttons apply hot sauce / wipe the nose.
/// Mouse and touch still tap trays directly through WorldInputRouter2D.
/// </summary>
public class IngredientGridSelector : MonoBehaviour
{
    [System.Serializable]
    public class Row
    {
        public AddIngredientScript[] trays;
    }

    [Tooltip("Top row first: chicken, beans, cheese, sour cream. Then: tomatoes, lettuce.")]
    public Row[] rows;
    [Tooltip("Moved next to the selected tray (the finger/hand icon).")]
    public Transform highlight;
    public enum HandSide { LeftOfTray, RightOfTray }
    [Tooltip("Which side of the tray the hand sits on. The edge of the hand nearest the tray reaches into it by Tip Overlap.")]
    public HandSide handSide = HandSide.RightOfTray;
    [Tooltip("How far the fingertip reaches into the tray, in world units.")]
    public float tipOverlap = 0.15f;
    public PlateScript plate;
    public NoseWipeScript nose;

    int _row, _col;
    bool _blockedLastFrame = true;

    void OnEnable()  { GameInput.GridMode = true; }
    void OnDisable() { GameInput.GridMode = false; }

    void Start() { MoveHighlight(); }

    void Update()
    {
        var input = GameInput.Instance;
        if (input == null) return;

        bool blocked = ModalFocus.HasFocus ||
                       (GameplayGate.Instance != null && !GameplayGate.Instance.InteractionEnabled);
        // A press that closed a modal (or arrived while one was up) must not also pick an ingredient.
        bool wasBlocked = _blockedLastFrame;
        _blockedLastFrame = blocked;
        if (blocked || wasBlocked) return;

        var step = input.NavigateStep;
        if (step != Vector2Int.zero) Move(step);
        // During the tutorial's "move the stick" prompt the hand can move but nothing can be added yet.
        if (plate && plate.tutorialMode && plate.tutor && plate.tutor.IntroPlaying) return;

        if (input.ConfirmPressed) SelectedTray()?.Interact();
        if (input.HotSaucePressed && plate) plate.ApplyHotSauce();
        if (input.SwipePressed && nose) nose.SwipeFinger();
    }

    AddIngredientScript SelectedTray()
    {
        if (rows == null || _row >= rows.Length) return null;
        var trays = rows[_row].trays;
        return (trays != null && _col < trays.Length) ? trays[_col] : null;
    }

    void Move(Vector2Int step)
    {
        if (rows == null || rows.Length == 0) return;

        if (step.y != 0)
        {
            // Screen-up is a lower row index.
            int row = Mathf.Clamp(_row - step.y, 0, rows.Length - 1);
            int len = rows[row].trays?.Length ?? 0;
            if (len == 0) return;
            _row = row;
            _col = Mathf.Min(_col, len - 1);
        }
        else
        {
            int len = rows[_row].trays?.Length ?? 0;
            _col = Mathf.Clamp(_col + step.x, 0, Mathf.Max(len - 1, 0));
        }
        MoveHighlight();
    }

    void MoveHighlight()
    {
        var tray = SelectedTray();
        if (!highlight || !tray) return;

        var trayRenderer = tray.GetComponentInChildren<SpriteRenderer>();
        var handRenderer = highlight.GetComponentInChildren<SpriteRenderer>();
        if (!trayRenderer || !handRenderer)
        {
            highlight.position = tray.transform.position;
            return;
        }

        // Put the fingertip at the tray's near edge, reaching in by tipOverlap, vertically centred on the tray.
        Bounds trayBounds = trayRenderer.bounds;
        Bounds handBounds = handRenderer.bounds;
        float handHalfWidth = handBounds.extents.x;
        float targetCenterX = handSide == HandSide.LeftOfTray
            ? trayBounds.min.x + tipOverlap - handHalfWidth
            : trayBounds.max.x - tipOverlap + handHalfWidth;
        Vector3 targetCenter = new Vector3(targetCenterX, trayBounds.center.y, highlight.position.z);

        // The hand's pivot is not necessarily its centre, so move by the difference.
        highlight.position += targetCenter - new Vector3(handBounds.center.x, handBounds.center.y, highlight.position.z);
    }
}
