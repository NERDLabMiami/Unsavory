using UnityEngine;

// Good will taco bookkeeping the boss reacts to (see DialogueContext). All state lives in PlayerPrefs.
// trust_tacos are the spendable retries: none at the start of a career, one per earned taco, spent on a retry.
public static class TacoLedger
{
    public const int MaxTrust = 3;

    public static int Trust { get { return PlayerPrefs.GetInt("trust_tacos", 0); } }
    public static int Earned { get { return Get("tacos_earned", "tacos earned"); } }
    public static int Missed { get { return Get("tacos_missed", "tacos missed"); } }
    public static int Spent { get { return PlayerPrefs.GetInt("tacos_spent", 0); } }
    public static int Balance { get { return Earned - Missed; } }
    public static int Standing { get { return Balance == 0 ? 0 : (Balance > 0 ? 1 : -1); } }
    public static bool WasTrusted { get { return PlayerPrefs.GetInt("was_trusted", 0) == 1; } }
    public static int Streak { get { return PlayerPrefs.GetInt("performance_streak", 0); } }

    // Older saves counted under keys with spaces.
    static int Get(string key, string legacy) { return PlayerPrefs.GetInt(key, PlayerPrefs.GetInt(legacy, 0)); }

    // A day that could have earned a taco is over. Call before the boss speaks so the dialogue sees the new totals;
    // was_trusted is judged on the standing from before this result, so "you used to be good" only follows a later slip.
    public static void RecordResult(bool earned)
    {
        if (Standing > 0) PlayerPrefs.SetInt("was_trusted", 1);
        string tally = earned ? "tacos_earned" : "tacos_missed";
        PlayerPrefs.SetInt(tally, Get(tally, earned ? "tacos earned" : "tacos missed") + 1);
        // Keep the other counter on the new key too, so the legacy fallback can't go stale.
        string other = earned ? "tacos_missed" : "tacos_earned";
        PlayerPrefs.SetInt(other, Get(other, earned ? "tacos missed" : "tacos earned"));

        int streak = Streak;
        PlayerPrefs.SetInt("performance_streak", earned ? (streak > 0 ? streak + 1 : 1) : (streak < 0 ? streak - 1 : -1));
        if (earned) AddTrust(1);
    }

    public static void AddTrust(int n)
    {
        SetTrust(Trust + n);
    }

    public static bool SpendTrust()
    {
        if (Trust <= 0) return false;
        SetTrust(Trust - 1);
        PlayerPrefs.SetInt("tacos_spent", Spent + 1);
        return true;
    }

    public static void Reset()
    {
        foreach (var k in new[] { "tacos_earned", "tacos_missed", "tacos earned", "tacos missed", "tacos_spent", "was_trusted", "performance_streak" })
            PlayerPrefs.DeleteKey(k);
        SetTrust(0);
    }

    static void SetTrust(int value)
    {
        PlayerPrefs.SetInt("trust_tacos", Mathf.Clamp(value, 0, MaxTrust));
        PlayerSignals.RaiseTrustChanged(Trust);
    }
}
