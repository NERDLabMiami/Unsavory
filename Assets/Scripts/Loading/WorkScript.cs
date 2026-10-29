using UnityEngine;

public class WorkScript : MonoBehaviour
{
    private const string PREF_HEALTH = "health";
    private const string PREF_USING_PAID_SICK_DAYS = "using paid sick days";
    private const string PREF_PAID_SICK_DAYS = "paid sick days";

    [Header("Health Effects")]
    public float decreaseHealthMinimum = 0.1f;
    public float decreaseHealthMaximum = 0.2f;
    public float increaseHealthMinimum = 1.0f;
    public float increaseHealthMaximum = 2.0f;

    private float GetHealth()
    {
        return PlayerPrefs.GetFloat(PREF_HEALTH, 60f);
    }

    private void SetHealth(float value)
    {
        PlayerPrefs.SetFloat(PREF_HEALTH, value);
        PlayerPrefs.Save();
    }

    public void DecreaseHealth()
    {
        float health = GetHealth();
        float delta = Random.Range(decreaseHealthMinimum, decreaseHealthMaximum); // min, max
        SetHealth(health - delta);
    }

    public void IncreaseHealth()
    {
        float health = GetHealth();
        float delta = Random.Range(increaseHealthMinimum, increaseHealthMaximum); // min, max
        SetHealth(health + delta);
    }

    /// <summary>
    /// Home door -> go to work.
    /// </summary>
    public void gotoWork()
    {
        // The run is over (laid off or evicted): the door leads out of the game, not to a shift.
        if (CareerFlowManager.Instance.RunEnded)
        {
            CareerFlowManager.Instance.LoadCredits();
            return;
        }
        if (CareerFlowManager.Instance.EvictIfRentOverdue()) return;
        DecreaseHealth();
        CareerFlowManager.Instance.BeginShift();
    }

    /// <summary>
    /// Bed -> call in sick / stay home.
    /// </summary>
    public void stayHome()
    {
        if (CareerFlowManager.Instance.RunEnded) return;
        if (CareerFlowManager.Instance.EvictIfRentOverdue()) return;
        TryUsePaidSickDay();
        CareerStats.SickDayTaken();
        IncreaseHealth();
        // The next morning's state line acknowledges the skipped workday (a toast shown now would vanish with this scene).
        NarrativeRuntime.SetSkippedPreviousShift();
        CareerFlowManager.Instance.LoadHome();
    }

    private void TryUsePaidSickDay()
    {
        if (!PlayerPrefs.HasKey(PREF_USING_PAID_SICK_DAYS))
            return;

        int paidSickDays = PlayerPrefs.GetInt(PREF_PAID_SICK_DAYS, 0);
        if (paidSickDays <= 0)
            return;

        paidSickDays--;
        PlayerPrefs.SetInt(PREF_PAID_SICK_DAYS, paidSickDays);
        PlayerPrefs.Save();

        // Sick day wage credit without needing a Player object in the scene
        PlayerState.SetEarnedWages(PlayerState.GetEarnedWages() + 8f * PlayerState.GetHourlyRate());
    }
}
