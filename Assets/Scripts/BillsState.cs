using UnityEngine;
using SimpleJSON;

public static class BillsState
{
    public static bool IsBillPaid(int id) => PlayerPrefs.GetInt("bill" + id, 0) == 1;

    public static string GetEffect(int id)
    {
        var billsText = Resources.Load<TextAsset>("bills");
        if (billsText == null) return "";
        JSONNode bills = JSON.Parse(billsText.ToString());
        if (bills == null || bills["bills"] == null) return "";
        return bills["bills"][id]["effect"].Value;
    }

    /// <summary>True while any bill carrying this effect is overdue and unpaid.</summary>
    public static bool IsEffectActive(string effect)
    {
        var billsText = Resources.Load<TextAsset>("bills");
        if (billsText == null) return false;
        JSONNode bills = JSON.Parse(billsText.ToString());
        if (bills == null || bills["bills"] == null) return false;
        for (int i = 0; i < bills["bills"].Count; i++)
            if (bills["bills"][i]["effect"].Value == effect && IsBillOverdue(i)) return true;
        return false;
    }

    /// <summary>An overdue Cable & Phone bill kills the phone: bills can no longer be paid.</summary>
    public static bool IsPhoneDead() => IsEffectActive("phone effect");

    public static bool IsBillOverdue(int id)
    {
        var billsText = Resources.Load<TextAsset>("bills");
        if (billsText == null) return false;

        JSONNode bills = JSON.Parse(billsText.ToString());
        if (bills == null || bills["bills"] == null) return false;

        int due = bills["bills"][id]["due"].AsInt;
        int day = PlayerPrefs.GetInt("current level", 0);

        // overdue means day > due, AND unpaid
        return (day > due) && !IsBillPaid(id);
    }
}