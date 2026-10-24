using UnityEngine;
using System.Collections.Generic;

// Fills {token}s in mail and toasts. Every DialogueContext stat is a token ({day}, {missed_bills}, {wages_paid}, ...),
// plus {metro} (the active metro's name), {min_wage} (its minimum hourly wage) and {bill_<cola key>} for what each bill costs there ({bill_rent}, {bill_groceries}, ...).
// Money stats print as "$0.00"; the rest are rounded to whole numbers. Unknown tokens are left as written.
public static class TokenFormatter
{
    static readonly HashSet<string> Currency = new HashSet<string> { "money", "wages_paid", "wages_total" };

    public static string Apply(string text, Dictionary<string, float> context = null)
    {
        if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0) return text;

        if (context == null) context = DialogueContext.Build();
        foreach (var kv in context)
            text = text.Replace("{" + kv.Key + "}", Format(kv.Key, kv.Value));

        text = text.Replace("{metro}", ColaData.CurrentMetroName());
        text = text.Replace("{min_wage}", Money(ColaData.CurrentMinimumWage()));

        var bills = BillsState.Bills();
        if (bills != null)
            for (int i = 0; i < bills.Count; i++)
            {
                string key = BillsState.ColaKey(i);
                if (key != "") text = text.Replace("{bill_" + key + "}", Money(BillsState.GetAmount(i)));
            }
        return text;
    }

    static string Format(string key, float value) => Currency.Contains(key) ? Money(value) : Mathf.RoundToInt(value).ToString();

    static string Money(float value) => "$" + value.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture);
}
