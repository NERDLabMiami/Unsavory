using UnityEngine;
using SimpleJSON;

public static class BillsState
{
    static JSONNode _bills;

    /// <summary>The "bills" array of Resources/bills.txt (parsed once), or null.</summary>
    public static JSONNode Bills()
    {
        if (_bills != null) return _bills;
        var billsText = Resources.Load<TextAsset>("bills");
        if (billsText == null) return null;
        _bills = JSON.Parse(billsText.ToString())?["bills"];
        return _bills;
    }

    public static bool IsBillPaid(int id) => PlayerPrefs.GetInt("bill" + id, 0) == 1;

    public static string GetEffect(int id)
    {
        var bills = Bills();
        if (bills == null) return "";
        return bills[id]["effect"].Value;
    }

    /// <summary>The bill's cola.json key (bills.txt "cola"), or "" when it has none.</summary>
    public static string ColaKey(int id)
    {
        var bills = Bills();
        return bills != null ? bills[id]["cola"].Value : "";
    }

    /// <summary>False when the active metro has no such bill (cola.json null override); it is hidden and never counts as missed.</summary>
    public static bool IsAvailable(int id) => ColaData.IsAvailable(ColaKey(id));

    /// <summary>What the bill costs: cola.json (metro override, else national baseline), falling back to bills.txt "amount".</summary>
    public static float GetAmount(int id)
    {
        float amount;
        if (ColaData.TryGetBill(ColaKey(id), out amount)) return amount;
        var bills = Bills();
        return bills != null ? bills[id]["amount"].AsFloat : 0f;
    }

    /// <summary>True while any bill carrying this effect is overdue and unpaid.</summary>
    public static bool IsEffectActive(string effect)
    {
        var bills = Bills();
        if (bills == null) return false;
        for (int i = 0; i < bills.Count; i++)
            if (bills[i]["effect"].Value == effect && IsBillOverdue(i)) return true;
        return false;
    }

    /// <summary>An overdue Cable & Phone bill kills the phone: bills can no longer be paid.</summary>
    public static bool IsPhoneDead() => IsEffectActive("phone effect");

    /// <summary>Index of the Cable & Phone bill (the one carrying the phone effect), or -1.</summary>
    public static int PhoneBillId()
    {
        var bills = Bills();
        if (bills == null) return -1;
        for (int i = 0; i < bills.Count; i++)
            if (bills[i]["effect"].Value == "phone effect" && IsAvailable(i)) return i;
        return -1;
    }

    /// <summary>Pays the phone bill if it's unpaid and affordable, clearing the phone effect it caused. False if nothing was paid.</summary>
    public static bool PayPhoneBill()
    {
        int id = PhoneBillId();
        if (id < 0 || IsBillPaid(id)) return false;

        float amount = Mathf.Round(GetAmount(id));
        float money = PlayerState.GetMoney();
        if (money < amount) return false;

        PlayerState.SetMoney(money - amount);
        PlayerPrefs.SetInt("bill" + id, 1);

        int daysOverdue = PlayerPrefs.GetInt("current level", 0) - Bills()[id]["due"].AsInt;
        if (daysOverdue > 0)
            PlayerPrefs.SetInt("phone effect", Mathf.Max(0, PlayerPrefs.GetInt("phone effect", 0) - daysOverdue));
        PlayerPrefs.Save();

        BillsSignals.RaiseBillPaid(id);
        return true;
    }

    public static bool IsBillOverdue(int id)
    {
        var bills = Bills();
        if (bills == null || !IsAvailable(id)) return false;
        if (bills[id]["type"].Value == "buy") return false; // purchases have no due date

        int due = bills[id]["due"].AsInt;
        int day = PlayerPrefs.GetInt("current level", 0);

        // overdue means day > due, AND unpaid
        return (day > due) && !IsBillPaid(id);
    }

    /// <summary>Bills (not purchases) due today or earlier and still unpaid.</summary>
    public static int DueCount()
    {
        var bills = Bills();
        if (bills == null) return 0;
        int day = PlayerPrefs.GetInt("current level", 0);
        int n = 0;
        for (int i = 0; i < bills.Count; i++)
            if (bills[i]["type"].Value != "buy" && IsAvailable(i) && !IsBillPaid(i) && day >= bills[i]["due"].AsInt) n++;
        return n;
    }

    /// <summary>Bills (not purchases) past their due day and still unpaid.</summary>
    public static int MissedCount()
    {
        var bills = Bills();
        if (bills == null) return 0;
        int n = 0;
        for (int i = 0; i < bills.Count; i++)
            if (bills[i]["type"].Value != "buy" && IsBillOverdue(i)) n++;
        return n;
    }
}
