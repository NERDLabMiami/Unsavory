using UnityEngine;

// Career totals for mail and the boss (see DialogueContext). All state lives in PlayerPrefs and is wiped with the rest
// of the career by resetPlayerData. Catering and the tutorial are not part of the career, so they are not counted.
public static class CareerStats
{
    public static int OrdersServed { get { return PlayerPrefs.GetInt("orders_served", 0); } }
    public static int OrdersMissed { get { return PlayerPrefs.GetInt("orders_missed", 0); } }
    public static int ShiftsWorked { get { return PlayerPrefs.GetInt("shifts_worked", 0); } }
    public static int PerfectDays { get { return PlayerPrefs.GetInt("perfect_days", 0); } }
    public static int SickDays { get { return PlayerPrefs.GetInt("sick_days", 0); } }
    // Most orders served in a single finished shift.
    public static int BestDay { get { return PlayerPrefs.GetInt("best_day", 0); } }

    static bool Counting { get { return !PlayerPrefs.HasKey("catering"); } }

    public static void OrderServed()
    {
        if (!Counting) return;
        PlayerPrefs.SetInt("orders_served", OrdersServed + 1);
        PlayerPrefs.SetInt("orders_served_today", PlayerPrefs.GetInt("orders_served_today", 0) + 1);
    }

    public static void OrderMissed()
    {
        if (Counting) PlayerPrefs.SetInt("orders_missed", OrdersMissed + 1);
    }

    /// <summary>A shift is over. <paramref name="completed"/> is a full day; <paramref name="perfect"/> a flawless one.</summary>
    public static void ShiftEnded(bool completed, bool perfect)
    {
        if (!Counting) return;
        int today = PlayerPrefs.GetInt("orders_served_today", 0);
        PlayerPrefs.DeleteKey("orders_served_today");
        if (!completed) return;
        PlayerPrefs.SetInt("shifts_worked", ShiftsWorked + 1);
        if (perfect) PlayerPrefs.SetInt("perfect_days", PerfectDays + 1);
        if (today > BestDay) PlayerPrefs.SetInt("best_day", today);
    }

    public static void SickDayTaken()
    {
        PlayerPrefs.SetInt("sick_days", SickDays + 1);
    }
}
