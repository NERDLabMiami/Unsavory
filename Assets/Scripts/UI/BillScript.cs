using UnityEngine;
using UnityEngine.UI;

public class BillScript : MonoBehaviour
{
    [Header("Bill Data")]
    public int id;
    public int amount;
    public string title;
    public string effect;
    public int dueDate = 0;

    [Header("UI")]
    public Text titleUI;
    public Text amountUI;
    public Text dueUI;
    public Button payButton;
    public Text bankAccount; // "Account Balance: $0.00"

    private bool paid = false;
    private bool newlyDisabled = false;
    private int effectAmount = 0;

    void Start()
    {
        PlayerState.EnsureDefaultsExist();
        paid = (PlayerPrefs.GetInt("bill" + id, 0) == 1);
        Refresh();
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
        if (!paid) Refresh();
    }

    public void Refresh()
    {
        float money = PlayerState.GetMoney();
        int currentDay = PlayerPrefs.GetInt("current level", 0);

        if (titleUI)  titleUI.text = title;
        if (amountUI) amountUI.text = "$" + amount.ToString("0");

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
                dueUI.text = "Due in " + (dueDate - currentDay) + " days";
                effectAmount = 0;
            }
            else if (dueDate == currentDay)
            {
                dueUI.text = "Due Today";
                effectAmount = 0;
            }
            else
            {
                dueUI.text = "OVERDUE!";
                effectAmount = currentDay - dueDate; // days overdue
            }
        }

        SetPayButtonEnabled(money >= amount);
    }

    private void SetPayButtonEnabled(bool enabled)
    {
        if (!payButton) return;

        // Important: use interactable; enabled is for component enable/disable
        payButton.interactable = enabled;

        if (!enabled)
            Disable();
        else
            newlyDisabled = false;
    }

    public bool isPaid() => paid;

    public void Disable()
    {
        if (!payButton) return;

        if (!newlyDisabled)
        {
            newlyDisabled = true;
            var anim = payButton.gameObject.GetComponent<Animator>();
            if (anim) anim.SetTrigger("disable");
        }

        payButton.interactable = false;
    }

    public void payBill()
    {
        if (paid) return;

        float money = PlayerState.GetMoney();
        if (money < amount)
        {
            Debug.Log($"Not enough money. Have: {money} need {amount}");
            Disable();
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

        var label = payButton.GetComponentInChildren<Text>();
        if (label) label.text = "Paid";

        var anim = payButton.gameObject.GetComponent<Animator>();
        if (anim) anim.SetTrigger("disable");

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
}
