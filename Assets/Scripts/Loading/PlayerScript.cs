using UnityEngine;
using UnityEngine.UI;

public class PlayerScript : MonoBehaviour
{
    [Header("Optional UI")]
    public Text moneyUI;
    public Text pillsUI;

    [Header("Reset Behavior (debug)")]
    public bool reset = false;

    private const string PREF_MONEY = "money";
    private const string PREF_PILLS = "pills";
    private const string PREF_HOURLY_RATE = "hourly rate";
    private const string PREF_EARNED_WAGES = "earned wages";
    private const string PREF_CURRENT_LEVEL = "current level";

    // ----------------------------
    // Static API (scene-independent)
    // ----------------------------

    public static float GetHourlyRate() => PlayerPrefs.GetFloat(PREF_HOURLY_RATE, 8.05f);

    public static float GetMoney() => PlayerPrefs.GetFloat(PREF_MONEY, 0f);

    public static void SetMoney(float value)
    {
        PlayerPrefs.SetFloat(PREF_MONEY, value);
        PlayerPrefs.Save();
    }

    public static int GetPills() => PlayerPrefs.GetInt(PREF_PILLS, 0);

    public static void SetPills(int value)
    {
        PlayerPrefs.SetInt(PREF_PILLS, value);
        PlayerPrefs.Save();
    }

    public static float PotentialWagesHours(float hoursWorked) => GetHourlyRate() * hoursWorked;

    /// <summary>
    /// Adds to "earned wages" (paycheck accumulator). ReturnHomeScript cashes this out.
    /// </summary>
    public static float AddWagesHours(float hoursWorked)
    {
        float earnedWages = PlayerPrefs.GetFloat(PREF_EARNED_WAGES, 0f);
        float dayWage = PotentialWagesHours(hoursWorked);

        earnedWages += dayWage;
        PlayerPrefs.SetFloat(PREF_EARNED_WAGES, earnedWages);
        PlayerPrefs.Save();

        return dayWage;
    }

    public static void SetCateringEventFlag()
    {
        PlayerPrefs.SetInt("catering", 1);
        PlayerPrefs.Save();
    }

    // ----------------------------
    // Instance (UI view + legacy)
    // ----------------------------

    private void Start()
    {
        EnsureDefaultsExist();

        // Your legacy debug reset behavior
        if (!PlayerPrefs.HasKey(PREF_CURRENT_LEVEL) || reset)
        {
            // Reset player data (health + money)
            CustomFunctionScript.resetPlayerData(60, GetMoney());
            PlayerPrefs.Save();
        }

        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void EnsureDefaultsExist()
    {
        if (!PlayerPrefs.HasKey(PREF_PILLS))
            PlayerPrefs.SetInt(PREF_PILLS, 0);

        if (!PlayerPrefs.HasKey(PREF_MONEY))
            PlayerPrefs.SetFloat(PREF_MONEY, 0f);

        PlayerPrefs.Save();
    }

    public void RefreshUI()
    {
        if (moneyUI) moneyUI.text = "$" + GetMoney().ToString("0");
        if (pillsUI) pillsUI.text = GetPills().ToString();
    }

    // Preserve your existing instance method signatures (so old scripts don't break):
    public float potentialWages(float timeWorkedHours) => PotentialWagesHours(timeWorkedHours);
    public float addWages(float timeWorkedHours) => AddWagesHours(timeWorkedHours);
    public void cateringEvent() => SetCateringEventFlag();
}
