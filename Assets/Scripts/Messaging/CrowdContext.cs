using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;

// Who is coming in today and how many, for the boss's return speeches. Each levels.txt entry lists the crowds it can draw
// ("crowds": ["students", "office"]); the boss highlights one. Size comes from the timing levers: orders / "waiting time".
// Variants test the crowd with a flag ({ "crowd_hospital": "==1" }) and the size with { "crowd_size": ">=3" }; text can use {crowd} and {crowd_size}.
public static class CrowdContext
{
    // The crowds levels.txt may name, with how the boss says each one.
    static readonly Dictionary<string, string> Names = new Dictionary<string, string>
    {
        { "students", "students" },
        { "families", "families" },
        { "office", "office workers" },
        { "health", "health-conscious regulars" },
        { "construction", "construction workers" },
        { "hospital", "hospital staff" },
    };

    // Orders per waiting-time unit below which the crowd is size 1, 2, 3; anything above is 4.
    static readonly float[] SizeBands = { 2f, 3.5f, 5.5f };
    static readonly string[] SizeWords = { "quiet", "steady", "busy", "packed" };

    public struct Crowd { public string name; public int size; }

    public static Crowd Pick(int level)
    {
        var levels = JSON.Parse(Resources.Load<TextAsset>("levels").ToString())["levels"];
        var entry = levels[Mathf.Clamp(CareerFlowManager.LevelIndex(level), 0, levels.Count - 1)];

        var crowd = new Crowd { name = "", size = Size(entry["waiting time"].AsFloat, entry["orders"].AsInt) };
        var options = entry["crowds"].AsArray;
        if (options.Count > 0) crowd.name = options[Random.Range(0, options.Count)].Value;
        return crowd;
    }

    public static int Size(float waitingTime, int orders)
    {
        float rush = orders / Mathf.Max(waitingTime, 0.1f);
        int size = 1;
        foreach (float band in SizeBands) if (rush >= band) size++;
        return size;
    }

    public static Dictionary<string, float> Context(Crowd crowd)
    {
        var context = new Dictionary<string, float> { { "crowd_size", crowd.size } };
        foreach (string name in Names.Keys) context["crowd_" + name] = name == crowd.name ? 1 : 0;
        return context;
    }

    public static Dictionary<string, string> Vars(Crowd crowd)
    {
        string name;
        return new Dictionary<string, string>
        {
            { "crowd", Names.TryGetValue(crowd.name, out name) ? name : "customers" },
            { "crowd_size", SizeWords[Mathf.Clamp(crowd.size, 1, SizeWords.Length) - 1] },
        };
    }
}
