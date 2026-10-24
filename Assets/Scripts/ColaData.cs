using UnityEngine;
using SimpleJSON;

// Cost-of-living data from Resources/cola.json: national baselineBills plus per-metro billOverrides.
// The active metro is the "metro" pref (an id such as "nyc"); empty or unknown means the national baseline.
// A null override means that bill doesn't exist in the metro (no car in NYC). SimpleJSON reads a JSON null as the string "null".
public static class ColaData
{
    const string MetroPref = "metro";
    static JSONNode _root;

    static JSONNode Root()
    {
        if (_root != null) return _root;
        var text = Resources.Load<TextAsset>("cola");
        _root = text != null ? JSON.Parse(text.ToString()) : null;
        return _root;
    }

    public static string CurrentMetroId() => PlayerPrefs.GetString(MetroPref, "");

    static JSONNode CurrentMetro()
    {
        var root = Root();
        string id = CurrentMetroId();
        if (root == null || id == "") return null;
        var metros = root["metros"];
        for (int i = 0; i < metros.Count; i++)
            if (metros[i]["id"].Value == id) return metros[i];
        return null;
    }

    /// <summary>Display name of the active metro, or "the national average" when none is set.</summary>
    public static string CurrentMetroName()
    {
        var metro = CurrentMetro();
        return metro != null ? metro["name"].Value : "the national average";
    }

    /// <summary>Active metro's minimumHourlyWage, else nationalMinimumHourlyWage. 0 when cola.json has neither.</summary>
    public static float CurrentMinimumWage()
    {
        var root = Root();
        if (root == null) return 0f;

        var metro = CurrentMetro();
        if (metro != null)
        {
            var wage = metro["minimumHourlyWage"];
            if (wage != null && wage.Value != "null" && wage.Value != "") return wage.AsFloat;
        }

        var national = root["nationalMinimumHourlyWage"];
        return national != null && national.Value != "null" ? national.AsFloat : 0f;
    }

    /// <summary>False when the metro explicitly has no such bill (null override).</summary>
    public static bool IsAvailable(string colaKey)
    {
        if (colaKey == "") return true;
        var metro = CurrentMetro();
        if (metro == null) return true;
        var over = metro["billOverrides"][colaKey];
        return !(over != null && over.Value == "null");
    }

    /// <summary>The metro override when it has one, else the national baseline. False when cola.json has neither.</summary>
    public static bool TryGetBill(string colaKey, out float amount)
    {
        amount = 0f;
        var root = Root();
        if (root == null || colaKey == "") return false;

        var metro = CurrentMetro();
        if (metro != null)
        {
            var over = metro["billOverrides"][colaKey];
            if (over != null && over.Value != "null" && over.Value != "") { amount = over.AsFloat; return true; }
        }

        var baseline = root["baselineBills"][colaKey];
        if (baseline == null || baseline.Value == "" || baseline.Value == "null") return false;
        amount = baseline.AsFloat;
        return true;
    }
}
