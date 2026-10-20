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
    [Tooltip("Tip Overlap was tuned for trays this wide (world units); it is scaled by the selected tray's width relative to this, so smaller trays get a proportionally smaller reach. 0 = use Tip Overlap as is.")]
    public float referenceTrayWidth = 2.5f;
    [Tooltip("The Player object (grabbing hand, tray, sauce hand). Moved as a whole so the grabbing hand sits on the bin. Defaults to the highlight's parent.")]
    public Transform playerRoot;
    [Tooltip("The hand that holds the sauce. Defaults to the Player's 'Right Hand' child.")]
    public Transform sauceHand;
    public float moveTime = 0.12f;
    public PlateScript plate;
    public NoseWipeScript nose;

    /// <summary>Raised when the highlight lands on a different tray (not when a press is stopped by an edge).</summary>
    public event System.Action SelectionChanged;

    int _row, _col;
    bool _blockedLastFrame = true;

    void OnEnable()  { GameInput.GridMode = true; }
    void OnDisable() { GameInput.GridMode = false; }

    void Start() { MoveHighlight(true); }

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
        int oldRow = _row, oldCol = _col;

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
        if (_row != oldRow || _col != oldCol) SelectionChanged?.Invoke();
    }

    // The Player object holds the grabbing hand (highlight), the tray and the sauce hand; it moves as one.
    Transform PlayerRoot()
    {
        if (playerRoot) return playerRoot;
        return highlight && highlight.parent ? highlight.parent : highlight;
    }

    SpriteRenderer SauceHandRenderer(Transform root)
    {
        if (!sauceHand) sauceHand = root.Find("Right Hand");
        return sauceHand ? sauceHand.GetComponentInChildren<SpriteRenderer>() : null;
    }

    void MoveHighlight(bool snap = false)
    {
        var tray = SelectedTray();
        if (!highlight || !tray) return;

        var trayRenderer = tray.GetComponentInChildren<SpriteRenderer>();
        var handRenderer = highlight.GetComponentInChildren<SpriteRenderer>();
        var root = PlayerRoot();
        if (!trayRenderer || !handRenderer)
        {
            root.position += tray.transform.position - highlight.position;
            return;
        }

        Bounds trayBounds = trayRenderer.bounds;
        Bounds handBounds = handRenderer.bounds;
        Vector3 delta;
        if (root == highlight)
        {
            // Lone hand: put the fingertip at the tray's near edge, reaching in by tipOverlap, vertically centred on the tray.
            float overlap = referenceTrayWidth > 0f ? tipOverlap * (trayBounds.size.x / referenceTrayWidth) : tipOverlap;
            float targetCenterX = handSide == HandSide.LeftOfTray
                ? trayBounds.min.x + overlap - handBounds.extents.x
                : trayBounds.max.x - overlap + handBounds.extents.x;
            delta = new Vector3(targetCenterX - handBounds.center.x, trayBounds.center.y - handBounds.center.y, 0f);
        }
        else
        {
            // Whole Player: the span from the grabbing hand to the sauce hand is centred on the bin, so the grabbing hand
            // reaches in from the left. Height stays where the Player is placed in the scene.
            Bounds span = handBounds;
            var sauceRenderer = SauceHandRenderer(root);
            if (sauceRenderer) span.Encapsulate(sauceRenderer.bounds);
            delta = new Vector3(trayBounds.center.x - span.center.x, 0f, 0f);
        }

        if (snap) root.position += delta;
        else Tween.Move(root, root.position + delta, moveTime, Tween.Ease.OutQuad);
    }
}
