using UnityEngine;
using SimpleJSON;
using System.Collections.Generic;

// What the boss can react to. Variants in boss dialogue.txt name these in "when", e.g. { "late": ">0", "health": "<30" }.
// To add a trigger: write a PlayerPrefs value and list it in Build(), or pass it as the extra context of CharacterDialog.Say.
public static class DialogueContext
{
    public static Dictionary<string, float> Build()
    {
        return new Dictionary<string, float>
        {
            { "health", PlayerPrefs.GetFloat("health", 0) },
            { "late", CommuteState.LateEffect },
            { "electricity", PlayerPrefs.GetInt("electricity effect", 0) },
            { "phone", PlayerPrefs.GetInt("phone effect", 0) },
            { "overtime", PlayerPrefs.GetInt("overtime effect", 0) },
            { "insurance", PlayerPrefs.GetInt("insurance effect", 0) },
            { "day", PlayerPrefs.GetInt("current level", 1) },
            { "warnings", PlayerPrefs.GetInt("warnings", 0) },
            { "retries", TacoLedger.Trust },   // older name for trust_tacos
            { "sneezes", PlayerPrefs.GetInt("sneezes", 0) },
            // Good will tacos: trust_tacos are spendable retries; balance is earned - missed, standing its sign (-1, 0, +1).
            { "trust_tacos", TacoLedger.Trust },
            { "tacos_earned", TacoLedger.Earned },
            { "tacos_missed", TacoLedger.Missed },
            { "tacos_spent", TacoLedger.Spent },
            { "taco_balance", TacoLedger.Balance },
            { "taco_standing", TacoLedger.Standing },
            { "was_trusted", TacoLedger.WasTrusted ? 1 : 0 },
            // Consecutive earned (+) or missed (-) tacos.
            { "performance_streak", TacoLedger.Streak },
            // Career totals (see CareerStats).
            { "orders_served", CareerStats.OrdersServed },
            { "orders_missed", CareerStats.OrdersMissed },
            { "shifts_worked", CareerStats.ShiftsWorked },
            { "perfect_days", CareerStats.PerfectDays },
            { "sick_days", CareerStats.SickDays },
            { "best_day", CareerStats.BestDay },
            { "first_satisfactory", PlayerPrefs.GetInt("satisfactory played", 0) == 0 ? 1 : 0 },
            // Bills past their due day and still unpaid (the "<name> effect" counters above are what they cause).
            { "missed_bills", BillsState.MissedCount() },
            // Bills due today or earlier and unpaid, unopened mail, and whether the car bill is paid up (0 = lost the car).
            { "bill_due", BillsState.DueCount() },
            { "unread_mail", MailArchive.UnreadCount() },
            { "car_ok", CommuteState.CarOk ? 1 : 0 },
            { "money",PlayerPrefs.GetFloat("money", 0) },
            { "pills", PlayerPrefs.GetInt("pills", 0) },
            // Career wages: the last paycheck cashed out and the sum of all of them.
            { "wages_paid", PlayerState.GetLastPaycheck() },
            { "wages_total", PlayerState.GetWagesTotal() },
        };
    }
}

// Chooses which variant of a speech plays. A variant's "when" must all hold; the highest "priority" (default 0) wins, then the
// most specific (most conditions), and ties are broken at random by "weight". Story beats use priority so a crowd comment never
// pre-empts them. "once" variants are skipped after they have played.
public static class DialogueSelector
{
    public static int Pick(string key, JSONNode variants, Dictionary<string, float> context, System.Func<int, bool> alreadyPlayed = null)
    {
        int bestPriority = int.MinValue;
        int bestScore = -1;
        var candidates = new List<int>();
        for (int i = 0; i < variants.Count; i++)
        {
            var v = variants[i];
            if (v["lines"].Count <= 0 && v["text"].Value == "") continue;   // toasts carry "text" instead of "lines"
            if (v["once"].Value == "true" && (alreadyPlayed != null ? alreadyPlayed(i) : HasPlayed(key, i))) continue;
            if (!Matches(v["when"], context)) continue;

            int priority = v["priority"].AsInt;
            int score = v["when"].Count;
            if (priority > bestPriority || (priority == bestPriority && score > bestScore))
            {
                bestPriority = priority; bestScore = score; candidates.Clear();
            }
            if (priority == bestPriority && score == bestScore) candidates.Add(i);
        }
        if (candidates.Count == 0) return -1;

        float total = 0f;
        foreach (int i in candidates) total += Weight(variants[i]);
        float roll = Random.value * total;
        foreach (int i in candidates)
        {
            roll -= Weight(variants[i]);
            if (roll <= 0f) return i;
        }
        return candidates[candidates.Count - 1];
    }

    public static void MarkPlayed(string key, int variant) { PlayerPrefs.SetInt(PlayedKey(key, variant), 1); }
    static bool HasPlayed(string key, int variant) { return PlayerPrefs.GetInt(PlayedKey(key, variant), 0) == 1; }
    static string PlayedKey(string key, int variant) { return "dialog:" + key + ":" + variant; }

    static float Weight(JSONNode v) { return v["weight"].Value != "" ? Mathf.Max(0f, v["weight"].AsFloat) : 1f; }

    static bool Matches(JSONNode when, Dictionary<string, float> context)
    {
        if (when.Count <= 0) return true;
        foreach (System.Collections.Generic.KeyValuePair<string, JSONNode> kv in when.AsObject)
        {
            float have;
            // A condition on something the game doesn't track yet can't hold.
            if (!context.TryGetValue(kv.Key, out have) || !Compare(have, kv.Value.Value)) return false;
        }
        return true;
    }

    // ">0", ">=30", "<5", "<=5", "==2", "!=2"; a bare number means ==.
    static bool Compare(float have, string expr)
    {
        expr = expr.Trim();
        string op = "==";
        foreach (var candidate in new[] { ">=", "<=", "==", "!=", ">", "<" })
            if (expr.StartsWith(candidate)) { op = candidate; expr = expr.Substring(candidate.Length); break; }

        float want;
        if (!float.TryParse(expr.Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out want))
        {
            Debug.LogError($"[DialogueSelector] can't read condition '{op}{expr}'.");
            return false;
        }
        switch (op)
        {
            case ">=": return have >= want;
            case "<=": return have <= want;
            case ">": return have > want;
            case "<": return have < want;
            case "!=": return !Mathf.Approximately(have, want);
            default: return Mathf.Approximately(have, want);
        }
    }
}
