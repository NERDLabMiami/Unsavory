using UnityEngine;
using SimpleJSON;

public static class BillsState
{
    public static bool IsBillPaid(int id) => PlayerPrefs.GetInt("bill" + id, 0) == 1;

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