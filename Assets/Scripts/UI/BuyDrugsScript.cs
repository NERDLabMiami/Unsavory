using UnityEngine;
using UnityEngine.UI;

public class BuyDrugsScript : MonoBehaviour
{
    public int costOfDrugs = 5;
    public Text canvasTitle;

    [Tooltip("Effective health a pill adds to today's shift (NoseWipeScript); it wears off overnight.")]
    public float pillBoost = 3f;
    public int maxPillsPerDay = 3;

    [Tooltip("Shown after the 1st, 2nd, 3rd... pill of the day; the last entry repeats.")]
    public string[] doseFeelings = { "Feeling Slightly Better", "Feeling Better", "Feeling Much Better" };

    [Tooltip("Found under Content/Buttons by name when left empty.")]
    public Button buyButton, takeButton;

    Animator _animator;
    RectTransform _rect;
    Canvas _canvas;
    bool _closing;

    // Set by NarrativeModalPanel: toasts.txt decides the text and the buttons, so this script stays out of the way.
    [HideInInspector] public bool narrativeDriven;

    void Awake()
    {
        PillsService.Price = costOfDrugs;
        PillsService.PillBoost = pillBoost;
        PillsService.MaxPerDay = maxPillsPerDay;
        _animator = GetComponent<Animator>();
        _rect = (RectTransform)transform;
        _canvas = GetComponentInParent<Canvas>();
        if (!buyButton) buyButton = FindButton("Content/Buttons/Buy Button");
        if (!takeButton) takeButton = FindButton("Content/Buttons/Take Button");
    }

    private Button FindButton(string path)
    {
        Transform t = transform.Find(path);
        Button button = t ? t.GetComponent<Button>() : null;
        if (!button) Debug.LogWarning("[BuyDrugsScript] no button at " + path + "; assign it in the inspector.");
        return button;
    }

    void OnEnable()
    {
        _closing = false;
        PlayerState.EnsureDefaultsExist();
        if (narrativeDriven) return;
        PlayerSignals.MoneyChanged += OnStockChanged;
        PlayerSignals.PillsChanged += OnStockChanged;
        SetHealthMessage();
        RefreshButtons();
    }

    void OnDisable()
    {
        PlayerSignals.MoneyChanged -= OnStockChanged;
        PlayerSignals.PillsChanged -= OnStockChanged;
    }

    // B closes the panel; so does a click or tap (or stick-cursor press) anywhere outside it.
    void Update()
    {
        var input = GameInput.Instance;
        if (narrativeDriven || _closing || input == null) return;

        if (input.CancelPressed || (input.ActivatePressed && (input.ActivateFromPointer || input.UsingVirtualCursor) && !PointerInside(input.PointerPosition)))
            Close();
    }

    private bool PointerInside(Vector2 screenPoint)
    {
        Camera cam = null;
        if (_canvas && _canvas.rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay) cam = _canvas.rootCanvas.worldCamera;
        return RectTransformUtility.RectangleContainsScreenPoint(_rect, screenPoint, cam);
    }

    // Same path as the other panels' Close Button: the Pop Out clip deactivates the panel, which releases the gate lock.
    public void Close()
    {
        if (_closing) return;
        _closing = true;
        if (_animator) _animator.SetTrigger("Pop Out");
        else gameObject.SetActive(false);
    }

    private void OnStockChanged(float _) => RefreshButtons();
    private void OnStockChanged(int _) => RefreshButtons();

    // Buy needs the money, Take needs a pill in hand.
    private void RefreshButtons()
    {
        if (buyButton) buyButton.interactable = PlayerState.GetMoney() >= costOfDrugs;
        if (takeButton) takeButton.interactable = PlayerState.GetPills() > 0;
    }

    public void buy()
    {
        var result = PillsService.Buy();
        if (!canvasTitle) return;
        canvasTitle.text = result == PillsService.Result.Ok ? "Cold Medicine Purchased" : PillsService.FailureText(result);
    }

    public void take()
    {
        int takenToday = PlayerState.GetPillsTakenToday();
        var result = PillsService.Take();
        if (!canvasTitle) return;
        if (result != PillsService.Result.Ok) canvasTitle.text = PillsService.FailureText(result);
        else if (doseFeelings.Length > 0)
            canvasTitle.text = doseFeelings[Mathf.Min(takenToday, doseFeelings.Length - 1)] + " (" + (takenToday + 1) + "/" + maxPillsPerDay + " today)";
    }

    public void SetHealthMessage()
    {
        // Pills taken today count: the shift will be played at this health.
        float health = PlayerState.GetHealth() + PlayerState.GetPillBoost();
        if (!canvasTitle) return;

        if (health > 29)      canvasTitle.text = "Feeling Good!";
        else if (health > 25) canvasTitle.text = "Feeling Okay";
        else if (health > 20) canvasTitle.text = "Feeling Less Than Stellar";
        else if (health > 15) canvasTitle.text = "Feeling Nauseous";
        else if (health > 10) canvasTitle.text = "Feeling Awful";
        else if (health > 5)  canvasTitle.text = "Feeling Horrible";
        else                  canvasTitle.text = "Definitely Sick";
    }
}
