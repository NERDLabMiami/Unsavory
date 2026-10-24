using UnityEngine;

public class HomeInteractable : MonoBehaviour, IWorldInteractable, IHoverable
{
    public enum HomeAction
    {
        GoToWork,
        StayHome,
        OpenBills,
        OpenPills,
        OpenLetter
    }

    public HomeAction action;

    [Header("Targets")]
    public WorkScript workScript;          // short-term: keep your existing stayHome/gotoWork logic
    public GameObject billsModalRoot;      // set active to open
    public GameObject pillsModalRoot;      // set active to open
    public GameObject letterModalRoot;
    [Header("Confirm UI")]
    public ConfirmPanel confirmPanel; // assign in inspector

    // ---- Hover: a warm glow copy of each sprite, slightly larger and drawn just behind it ----
    static readonly Color GlowColor = new Color(1f, 0.88f, 0.35f, 0.9f);
    readonly System.Collections.Generic.List<SpriteRenderer> _glows = new System.Collections.Generic.List<SpriteRenderer>();
    bool _glowBuilt;

    [Header("Hover toast")]
    public string hoverToastKey;           // Resources/toasts.txt key; blank uses the action's default
    HomeToast _toast;

    string HoverKey()
    {
        if (!string.IsNullOrEmpty(hoverToastKey)) return hoverToastKey;
        switch (action)
        {
            case HomeAction.GoToWork: return "hover_door";
            case HomeAction.StayHome: return "hover_bed";
            case HomeAction.OpenBills: return "hover_phone";
            case HomeAction.OpenPills: return "hover_pills";
            default: return null;
        }
    }

    void Start()
    {
        if (action == HomeAction.OpenBills) AnimUtil.Get<PhoneNotifyAnim>(gameObject);
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
            case HomeAction.StayHome:
                if (confirmPanel != null)
                    confirmPanel.Open(this);     // modal confirm
                else
                    ExecuteAction();             // fallback
                break;

            case HomeAction.OpenBills:
                var notify = GetComponent<PhoneNotifyAnim>();
                if (notify != null) notify.Stop();
                if (billsModalRoot) billsModalRoot.SetActive(true);
                break;

            case HomeAction.OpenPills:
                if (pillsModalRoot) pillsModalRoot.SetActive(true);
                break;

            case HomeAction.OpenLetter:
                if (letterModalRoot) letterModalRoot.SetActive(true);
                break;
        }
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