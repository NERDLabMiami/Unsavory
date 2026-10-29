using UnityEngine;

// How the player gets to work. The car is lost two ways: the car payment is missed (repossessed, which clears when the
// bill is paid), or the player drives with the car insurance overdue and crashes (wrecked for the rest of the run).
public static class CommuteState
{
    const string WreckedPref = "car wrecked";
    const string CrashPref = "crash commute";

    public static bool CarWrecked { get { return PlayerPrefs.GetInt(WreckedPref, 0) == 1; } }
    public static bool CarRepossessed { get { return BillsState.IsEffectActive("late effect"); } }
    public static bool InsuranceLapsed { get { return BillsState.IsEffectActive("insurance effect"); } }

    // Whether the car can be driven at all today.
    public static bool CarOk { get { return !CarRepossessed && !CarWrecked; } }

    // This morning's commute ended in the crash (consumed by the work entry that follows).
    public static bool CrashedCommute { get { return PlayerPrefs.GetInt(CrashPref, 0) == 1; } }
    public static void ClearCrashedCommute() { PlayerPrefs.DeleteKey(CrashPref); }

    // Late arrival: the car payment effect, or no car because it was wrecked (the commute by transit takes longer).
    public static int LateEffect { get { return Mathf.Max(PlayerPrefs.GetInt("late effect", 0), CarWrecked ? 1 : 0); } }

    // Driving with the insurance overdue ends in an accident that wrecks the car. Returns true when that happened;
    // the player still gets to work, by public transit.
    public static bool Drive()
    {
        if (!InsuranceLapsed) return false;
        PlayerPrefs.SetInt(WreckedPref, 1);
        PlayerPrefs.SetInt(CrashPref, 1);
        PlayerPrefs.Save();
        return true;
    }

    public static void ResetRun()
    {
        PlayerPrefs.DeleteKey(WreckedPref);
        PlayerPrefs.DeleteKey(CrashPref);
    }
}
