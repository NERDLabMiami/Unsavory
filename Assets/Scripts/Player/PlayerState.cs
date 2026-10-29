using UnityEngine;

public static class PlayerState
{
    private const string PREF_MONEY = "money";
    private const string PREF_PILLS = "pills";
    private const string PREF_PILLS_TAKEN_TODAY = "pills taken today";
    private const string PREF_PILL_BOOST = "pill boost";
    private const string PREF_HOURLY_RATE = "hourly rate";
    private const string PREF_EARNED_WAGES = "earned wages";
    private const string PREF_HEALTH = "health";
    private const string PREF_LAST_PAYCHECK = "last paycheck";
    private const string PREF_WAGES_TOTAL = "wages total";

    public static float GetHourlyRate() => PlayerPrefs.GetFloat(PREF_HOURLY_RATE, 8.05f);

    public static float GetMoney() => PlayerPrefs.GetFloat(PREF_MONEY, 0f);

    public static void SetMoney(float value, bool save = true)
    {
        PlayerPrefs.SetFloat(PREF_MONEY, value);
        if (save) PlayerPrefs.Save();
        PlayerSignals.RaiseMoneyChanged(value);
    }

    public static int GetPills() => PlayerPrefs.GetInt(PREF_PILLS, 0);

    public static void SetPills(int value, bool save = true)
    {
        PlayerPrefs.SetInt(PREF_PILLS, value);
        if (save) PlayerPrefs.Save();
        PlayerSignals.RaisePillsChanged(value);
    }

    // Pills only last the day: the count caps the daily dose, the boost is added to the shift's effective health (NoseWipeScript).
    public static int GetPillsTakenToday() => PlayerPrefs.GetInt(PREF_PILLS_TAKEN_TODAY, 0);

    public static float GetPillBoost() => PlayerPrefs.GetFloat(PREF_PILL_BOOST, 0f);

    public static void RecordPillTaken(float boost, bool save = true)
    {
        PlayerPrefs.SetInt(PREF_PILLS_TAKEN_TODAY, GetPillsTakenToday() + 1);
        PlayerPrefs.SetFloat(PREF_PILL_BOOST, GetPillBoost() + boost);
        if (save) PlayerPrefs.Save();
    }

    public static void ResetPillsToday()
    {
        PlayerPrefs.SetInt(PREF_PILLS_TAKEN_TODAY, 0);
        PlayerPrefs.SetFloat(PREF_PILL_BOOST, 0f);
    }

    public static float GetEarnedWages() => PlayerPrefs.GetFloat(PREF_EARNED_WAGES, 0f);

    public static void SetEarnedWages(float value, bool save = true)
    {
        PlayerPrefs.SetFloat(PREF_EARNED_WAGES, value);
        if (save) PlayerPrefs.Save();
        PlayerSignals.RaiseEarnedWagesChanged(value);
    }

    public static float GetLastPaycheck() => PlayerPrefs.GetFloat(PREF_LAST_PAYCHECK, 0f);

    public static float GetWagesTotal() => PlayerPrefs.GetFloat(PREF_WAGES_TOTAL, 0f);

    /// <summary>Called when a paycheck is cashed out: remembers it and adds it to the career total.</summary>
    public static void RecordPaycheck(float paycheck)
    {
        PlayerPrefs.SetFloat(PREF_LAST_PAYCHECK, paycheck);
        PlayerPrefs.SetFloat(PREF_WAGES_TOTAL, GetWagesTotal() + paycheck);
    }

    public static void ResetPaychecks()
    {
        PlayerPrefs.SetFloat(PREF_LAST_PAYCHECK, 0f);
        PlayerPrefs.SetFloat(PREF_WAGES_TOTAL, 0f);
    }

    public static float GetHealth() => PlayerPrefs.GetFloat(PREF_HEALTH, 60f);

    public static void SetHealth(float value, bool save = true)
    {
        PlayerPrefs.SetFloat(PREF_HEALTH, value);
        if (save) PlayerPrefs.Save();
        PlayerSignals.RaiseHealthChanged(value);
    }

    public static float PotentialWagesHours(float hoursWorked) => GetHourlyRate() * hoursWorked;

    // Adds to earned wages accumulator
    public static float AddWagesHours(float hoursWorked)
    {
        float wages = GetEarnedWages();
        float dayWage = PotentialWagesHours(hoursWorked);
        wages += dayWage;
        SetEarnedWages(wages);
        return dayWage;
    }

    public static void SetCateringEventFlag()
    {
        PlayerPrefs.SetInt("catering", 1);
        PlayerPrefs.Save();
    }

    public static void EnsureDefaultsExist()
    {
        if (!PlayerPrefs.HasKey(PREF_PILLS)) PlayerPrefs.SetInt(PREF_PILLS, 0);
        if (!PlayerPrefs.HasKey(PREF_MONEY)) PlayerPrefs.SetFloat(PREF_MONEY, 0f);
        if (!PlayerPrefs.HasKey(PREF_HEALTH)) PlayerPrefs.SetFloat(PREF_HEALTH, 60f);
        if (!PlayerPrefs.HasKey(PREF_HOURLY_RATE)) PlayerPrefs.SetFloat(PREF_HOURLY_RATE, 8.05f);
        if (!PlayerPrefs.HasKey(PREF_EARNED_WAGES)) PlayerPrefs.SetFloat(PREF_EARNED_WAGES, 0f);
        PlayerPrefs.Save();
    }
}
