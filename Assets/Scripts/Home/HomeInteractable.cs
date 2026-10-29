using UnityEngine;

public class HomeInteractable : MonoBehaviour, IWorldInteractable, IHoverable
{
    public enum HomeAction
    {
        GoToWork,
        StayHome,
        OpenBills,
        OpenPills,
        OpenLetter,
        FruitBowl      // hover text only
    }

    public HomeAction action;

    [Header("Targets")]
    public WorkScript workScript;          // short-term: keep your existing stayHome/gotoWork logic
    public GameObject billsModalRoot;      // set active to open
    public GameObject pillsModalRoot;      // set active to open
    public GameObject letterModalRoot;
    [Header("Confirm UI")]
    public ConfirmPanel confirmPanel; // assign in inspector
    [Tooltip("The scene panel (Bed Panel / Work Panel / Drug Panel) that shows this object's toasts.txt modal. Found by name when empty.")]
    public GameObject narrativePanel;

    // ---- Hover: a warm glow copy of each sprite, slightly larger and drawn just behind it ----
    static readonly Color GlowColor = new Color(1f, 0.88f, 0.35f, 0.9f);
    readonly System.Collections.Generic.List<SpriteRenderer> _glows = new System.Collections.Generic.List<SpriteRenderer>();
    bool _glowBuilt;

    [Header("Hover toast")]
    public string hoverToastKey;           // toasts.txt hover object (phone, bed, door, pills, fruit_bowl); blank uses the action's default
    HomeToast _toast;

    string HoverKey()
    {
        if (!string.IsNullOrEmpty(hoverToastKey)) return hoverToastKey;
        switch (action)
        {
            case HomeAction.GoToWork: return "door";
            case HomeAction.StayHome: return "bed";
            case HomeAction.OpenBills: return "phone";
            case HomeAction.OpenPills: return "pills";
            case HomeAction.FruitBowl: return "fruit_bowl";
            default: return null;
        }
    }

    // ---- Lights off: tint matches the "Lights Off" clip on Background/Foreground ----
    static readonly Color DimTint = new Color(0.4724f, 0.4724f, 0.4724f, 1f);
    const float DimDuration = 0.1667f;
    static readonly System.Collections.Generic.List<HomeInteractable> _all = new System.Collections.Generic.List<HomeInteractable>();
    readonly System.Collections.Generic.Dictionary<SpriteRenderer, Color> _baseColors = new System.Collections.Generic.Dictionary<SpriteRenderer, Color>();

    public static void ApplyDim(bool dim)
    {
        foreach (var h in _all) h.SetDimmed(dim);
    }

    void OnEnable()
    {
        _all.Add(this);
        if (Electricity.LightsOff) SetDimmed(true);
    }

    void OnDisable()
    {
        _all.Remove(this);
    }

    void SetDimmed(bool dim)
    {
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr == null || _glows.Contains(sr) || sr.name == "Hover Glow") continue;
            if (!_baseColors.TryGetValue(sr, out var orig))
            {
                orig = sr.color;
                _baseColors[sr] = orig;
            }
            var to = orig;
            if (dim) to = new Color(orig.r * DimTint.r, orig.g * DimTint.g, orig.b * DimTint.b, orig.a);
            Tween.Tint(sr, to, DimDuration, Tween.Ease.Linear);
        }
    }

    void Start()
    {
        if (action == HomeAction.OpenBills) AnimUtil.Get<PhoneNotifyAnim>(gameObject);
        if (action == HomeAction.OpenPills) PillsService.SyncFromScene();
    }

    public void SetHover(bool hovered)
    {
        if (hovered && GameplayGate.Instance != null && !GameplayGate.Instance.InteractionEnabled) return;
        if (hovered)
        {
            string key = HoverKey();
            if (key != null)
            {
                if (_toast == null) _toast = FindObjectOfType<HomeToast>();
                if (_toast != null) _toast.ShowHover(key);
            }
        }
        else if (_toast != null) _toast.HideHover();
        if (!_glowBuilt) BuildGlows();
        foreach (var g in _glows)
        {
            if (g == null) continue;
            var to = GlowColor;
            if (Electricity.LightsOff) { to.r *= DimTint.r; to.g *= DimTint.g; to.b *= DimTint.b; }
            if (!hovered) to.a = 0f;
            Tween.Tint(g, to, 0.1f, Tween.Ease.Linear);
        }
    }

    void BuildGlows()
    {
        _glowBuilt = true;
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
        {
            if (sr.sprite == null) continue;
            var go = new GameObject("Hover Glow");
            go.transform.SetParent(sr.transform, false);
            go.transform.localScale = Vector3.one * 1.05f;
            var glow = go.AddComponent<SpriteRenderer>();
            glow.sprite = sr.sprite;
            glow.flipX = sr.flipX;
            glow.flipY = sr.flipY;
            glow.sortingLayerID = sr.sortingLayerID;
            glow.sortingOrder = sr.sortingOrder - 1;
            glow.color = new Color(GlowColor.r, GlowColor.g, GlowColor.b, 0f);
            _glows.Add(glow);
        }
    }

    public void Interact()
    {
        if (GameplayGate.Instance != null && !GameplayGate.Instance.InteractionEnabled)
            return;

        switch (action)
        {
            case HomeAction.GoToWork:
                // Once the run is over the door only leads to the credits.
                if (CareerFlowManager.Instance.RunEnded) { ExecuteAction(); break; }
                OpenModal("door");
                break;

            case HomeAction.StayHome:
                if (CareerFlowManager.Instance.RunEnded) break;   // nothing left to skip
                OpenModal("bed");
                break;

            case HomeAction.OpenBills:
                OpenPhone();
                break;

            case HomeAction.OpenPills:
                if (!NarrativeModal.Open("pills", this) && pillsModalRoot) pillsModalRoot.SetActive(true);
                break;

            case HomeAction.OpenLetter:
                if (letterModalRoot) letterModalRoot.SetActive(true);
                break;
        }
    }

    // The narrative window for the bed / door. Without valid data the old behavior (confirm panel, else act at once) remains.
    void OpenModal(string objectKey)
    {
        if (NarrativeModal.Open(objectKey, this)) return;
        if (confirmPanel != null) confirmPanel.Open(this);
        else ExecuteAction();
    }

    public void OpenPhone()
    {
        var notify = GetComponent<PhoneNotifyAnim>();
        if (notify != null) notify.Stop();
        if (billsModalRoot) billsModalRoot.SetActive(true);
    }

    public void ExecuteAction()
    {
        switch (action)
        {
            case HomeAction.GoToWork:
                if (workScript != null) workScript.gotoWork();
                else CareerFlowManager.Instance.BeginShift();
                break;

            case HomeAction.StayHome:
                if (workScript != null) workScript.stayHome();
                else CareerFlowManager.Instance.LoadHome();
                break;
        }
    }

}