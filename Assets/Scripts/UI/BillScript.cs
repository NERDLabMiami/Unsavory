using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;

public class BillScript : MonoBehaviour
{
    [Header("Bill Data")]
    public int id;
    public int amount;
    public string title;
    public string effect;
    public int dueDate = 0;
    public bool isBuyItem = false; // one-shot purchase (groceries): "Buy" instead of "Pay", never marked paid
    const int MaxFruit = 5;

    [Header("UI")]
    public TMP_Text titleUI;
    public TMP_Text amountUI;
    public TMP_Text dueUI;
    public Button payButton;
    public TMP_Text bankAccount; // "Account Balance: $0.00"

    private bool paid = false;
    private bool newlyDisabled = false;
    private int effectAmount = 0;

    void Start()
    {
        PlayerState.EnsureDefaultsExist();
        paid = (PlayerPrefs.GetInt("bill" + id, 0) == 1);
        Refresh();
    }

    /// <summary>Configures this bill from Resources/bills.txt (index = id) so a prefab instance can be spawned for any bill.</summary>
    public void Init(int billId)
    {
        id = billId;
        var bills = BillsState.Bills();
        var node = bills != null ? bills[billId] : null;
        if (node != null && node["title"] != null)
        {
            title = node["title"];
            amount = Mathf.RoundToInt(BillsState.GetAmount(billId));
            dueDate = node["due"].AsInt;
            effect = node["effect"];
            isBuyItem = node["type"].Value == "buy";
        }
        PlayerState.EnsureDefaultsExist();
        paid = (PlayerPrefs.GetInt("bill" + id, 0) == 1);
        Refresh();
    }

    void Awake()
    {
        // Selection feedback: the Pay button is what gets focus, so it reports back to the row.
        if (payButton && !payButton.GetComponent<BillSelectionRelay>())
            payButton.gameObject.AddComponent<BillSelectionRelay>().bill = this;

        // Rows without layout sizes collapse to zero height in a layout group and stack on top of each
        // other, which breaks directional navigation. Guarantee a height.
        var le = GetComponent<LayoutElement>();
        if (!le) le = gameObject.AddComponent<LayoutElement>();
        if (le.minHeight < 80f) le.minHeight = 80f;
        if (le.preferredHeight < 80f) le.preferredHeight = 80f;
    }

    void OnEnable()
    {
        PlayerSignals.MoneyChanged += OnMoneyChanged;
        Refresh();
    }

    void OnDisable()
    {
        PlayerSignals.MoneyChanged -= OnMoneyChanged;
    }

    private void OnMoneyChanged(float _)
    {
        if (!paid || isBuyItem) Refresh();
    }

    public void Refresh()
    {
        float money = PlayerState.GetMoney();
        int currentDay = PlayerPrefs.GetInt("current level", 0);

        if (titleUI)  titleUI.text = title;
        if (amountUI) amountUI.text = "$" + amount.ToString("0");

        // Dead phone: nothing can be paid or bought, the player just watches the effects pile on.
        if (BillsState.IsPhoneDead())
        {
            if (dueUI) dueUI.text = "PHONE DEAD";
            if (!paid) { SetButtonLabel(isBuyItem ? "Buy" : AmountLabel()); Disable(); }
            else SetPaidVisualState();
            return;
        }

        if (isBuyItem)
        {
            bool full = PlayerPrefs.GetInt("fruit", MaxFruit) >= MaxFruit;
            SetButtonLabel(full ? "Full" : "Buy");
            if (dueUI) dueUI.text = "";
            SetPayButtonEnabled(!full && money >= amount);
            return;
        }

        if (paid)
        {
            SetPaidVisualState();
            return;
        }

        // Due UI + effectAmount
        if (dueUI)
        {
            if (dueDate > currentDay)
            {
                dueUI.text = "Due in " + (dueDate - currentDay) + (dueDate - currentDay == 1 ? " day" : " days");
                effectAmount = 0;
            }
            else if (dueDate == currentDay)
            {
                dueUI.text = "Due Today";
                effectAmount = 0;
            }
            else if(!isBuyItem)
            {
                dueUI.text = "OVERDUE!";
                effectAmount = currentDay - dueDate; // days overdue
            }
        }

        SetButtonLabel(AmountLabel());
        SetPayButtonEnabled(money >= amount);
    }

    string AmountLabel() => "$" + amount.ToString("0");

    // An unaffordable bill keeps a selectable (but dimmed) Pay button so it can be focused;
    // pressing it shakes instead of paying (see payBill).
    private void SetPayButtonEnabled(bool affordable)
    {
        if (!payButton) return;

        payButton.interactable = true;

        if (!affordable)
            Disable();
        else
        {
            if (newlyDisabled) RestorePayButton();
            newlyDisabled = false;
        }
    }

    public bool isPaid() => paid;

    public void Disable()
    {
        if (!payButton) return;

        if (!newlyDisabled)
        {
            newlyDisabled = true;
            DimPayButton();
        }
    }

    void SetButtonLabel(string text)
    {
        if (!payButton) return;
        var label = payButton.GetComponentInChildren<TMP_Text>();
        if (label) label.text = text;
    }

    void BuyItem()
    {
        if (BillsState.IsPhoneDead()) { Disable(); ShakePayButton(); return; }
        float money = PlayerState.GetMoney();
        if (money < amount || PlayerPrefs.GetInt("fruit", MaxFruit) >= MaxFruit)
        {
            Disable();
            ShakePayButton();
            return;
        }

        money -= amount;
        PlayerState.SetMoney(money);
        PlayerPrefs.SetInt("fruit", MaxFruit);
        PlayerPrefs.Save();
        PlayerSignals.RaiseFruitChanged(MaxFruit);

        if (bankAccount)
            bankAccount.text = money.ToString("Account Balance: $0.00");

        Refresh();
    }

    public void payBill()
    {
        if (isBuyItem) { BuyItem(); return; }
        if (paid) return;

        if (BillsState.IsPhoneDead())
        {
            Disable();
            ShakePayButton();
            return;
        }

        float money = PlayerState.GetMoney();
        if (money < amount)
        {
            Debug.Log($"Not enough money. Have: {money} need {amount}");
            Disable();
            ShakePayButton();
            return;
        }

        // Apply payment
        money -= amount;
        PlayerState.SetMoney(money);                 // pushes MoneyChanged to all binders
        PlayerPrefs.SetInt("bill" + id, 1);

        // Reduce effect if bill was overdue
        // effectAmount computed in Refresh(); if not overdue it will be 0.
        if (!string.IsNullOrEmpty(effect) && effectAmount > 0)
        {
            int currentEffect = PlayerPrefs.GetInt(effect, 0);
            int newEffect = Mathf.Max(0, currentEffect - effectAmount);
            PlayerPrefs.SetInt(effect, newEffect);
        }

        PlayerPrefs.Save();

        paid = true;

        if (bankAccount)
            bankAccount.text = money.ToString("Account Balance: $0.00");

        SetPaidVisualState();
        DisableOtherBillsIfNeeded(money);

        BillsSignals.RaiseBillPaid(id); // <- Electricity uses this
    }

    private void SetPaidVisualState()
    {
        if (!payButton) return;

        var label = payButton.GetComponentInChildren<TMP_Text>();
        if (label) label.text = "Paid";

        DimPayButton();

        payButton.interactable = false;

        if (dueUI) dueUI.text = "";
    }

    private void DisableOtherBillsIfNeeded(float money)
    {
        var currentBills = GameObject.FindGameObjectsWithTag("Bill");
        for (int i = 0; i < currentBills.Length; i++)
        {
            var b = currentBills[i].GetComponent<BillScript>();
            if (b == null) continue;

            if (!b.isPaid() && b.amount > money)
                b.Disable();
        }
    }

    static readonly Color SelectedRowColor = new Color(1f, 0.92f, 0.45f, 1f);
    Graphic _rowGraphic;
    Color _rowColor;
    bool _rowCaptured;

    /// <summary>Highlights this row (and pops the Pay button) while the gamepad/keyboard focus is on it.</summary>
    public void SetSelected(bool selected)
    {
        if (!_rowCaptured)
        {
            _rowGraphic = GetComponent<Graphic>();
            if (_rowGraphic) _rowColor = _rowGraphic.color;
            _rowCaptured = true;
        }
        if (_rowGraphic)
            Tween.Tint(_rowGraphic, selected ? SelectedRowColor : _rowColor, 0.08f, Tween.Ease.Linear);
        if (payButton)
            Tween.Scale(payButton.transform, Vector3.one * (selected ? 1.12f : 1f), 0.1f, Tween.Ease.OutQuad);
    }

    static readonly Color DimColor = new Color(0.4f, 0.502f, 0.463f, 0.243f);
    Color? _payColor;
    Vector3 _payOrigin;
    bool _shaking;

    // "Can't do that": quick damped side-to-side wiggle of the Pay button.
    void ShakePayButton()
    {
        if (!payButton) return;
        var t = payButton.transform;
        if (!_shaking) _payOrigin = t.localPosition;
        _shaking = true;
        Tween.Run(payButton, "shake", 0.35f, v =>
        {
            float offset = Mathf.Sin(v * Mathf.PI * 6f) * (1f - v) * 12f;
            t.localPosition = _payOrigin + new Vector3(offset, 0f, 0f);
        }, Tween.Ease.Linear, 0f, () => { t.localPosition = _payOrigin; _shaking = false; });
    }

    void RestorePayButton()
    {
        var graphic = payButton.targetGraphic;
        if (graphic != null && _payColor.HasValue)
            Tween.Tint(graphic, _payColor.Value, 1f / 6f, Tween.Ease.Linear);
    }

    // Paid: the button greys out and fades (was the Pay Button controller's Disable Pay Button clip).
    void DimPayButton()
    {
        AnimUtil.DisableAnimator(payButton.gameObject);
        var graphic = payButton.targetGraphic;
        if (graphic != null)
        {
            if (!_payColor.HasValue) _payColor = graphic.color;
            Tween.Tint(graphic, DimColor, 1f / 6f, Tween.Ease.Linear);
        }
    }
}
