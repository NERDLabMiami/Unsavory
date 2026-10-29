using System;
using System.Collections.Generic;
using System.Globalization;

namespace Unsavory.Toasts
{
    // Development diagnostics. The Unity adapter points Warn/Error at Debug.LogWarning/LogError.
    public static class Diagnostics
    {
        public static Action<string> Warn;
        public static Action<string> Error;
        // Logs a nonmatch caused by a context key that was never provided (as opposed to a false comparison).
        public static bool LogMissingKeys;

        internal static void W(string message) { if (Warn != null) Warn(message); }
        internal static void E(string message) { if (Error != null) Error(message); }
    }

    // Read-only snapshot of what the narrative can react to. Values are numbers or strings; absent keys are absent, never zero.
    public class NarrativeContext
    {
        readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        public NarrativeContext Set(string key, double value) { _values[key] = value; return this; }
        public NarrativeContext Set(string key, bool value) { _values[key] = value ? 1d : 0d; return this; }
        public NarrativeContext Set(string key, string value) { if (value != null) _values[key] = value; return this; }
        public bool TryGet(string key, out object value) { return _values.TryGetValue(key, out value); }
        public bool Has(string key) { return _values.ContainsKey(key); }
        public IEnumerable<string> Keys { get { return _values.Keys; } }

        internal static string Format(object value)
        {
            if (value is string) return (string)value;
            double d = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            return Math.Abs(d - Math.Round(d)) < 1e-4
                ? ((long)Math.Round(d)).ToString(CultureInfo.InvariantCulture)
                : d.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }

    public class Choice
    {
        public string Label;
        public string Action;
        public Dictionary<string, string> EnabledWhen;
    }

    // One entry of any rule array: scene entry slot, hover, modal, or after-action.
    public class Rule
    {
        public string Id;
        public Dictionary<string, string> When;
        public string Once;
        public string Text;
        public List<string> Lines;
        public string Title;
        public List<Choice> Choices;

        public bool HasMessage { get { return !string.IsNullOrEmpty(Text) || (Lines != null && Lines.Count > 0); } }
        public bool IsModal { get { return Choices != null; } }
    }

    public class EntryChannels
    {
        public List<Rule> Special = new List<Rule>();
        public List<Rule> World = new List<Rule>();
        public List<Rule> State = new List<Rule>();
    }

    public class NarrativeDocument
    {
        public int SchemaVersion;
        public Dictionary<string, EntryChannels> SceneEntry = new Dictionary<string, EntryChannels>();
        public Dictionary<string, List<Rule>> Hover = new Dictionary<string, List<Rule>>();
        public Dictionary<string, List<Rule>> Modals = new Dictionary<string, List<Rule>>();
        public Dictionary<string, List<Rule>> AfterAction = new Dictionary<string, List<Rule>>();
        // Problems found while reading the JSON (wrong shapes), reported alongside the validator's.
        public List<string> ParseErrors = new List<string>();
    }

    public static class NarrativeActions
    {
        public const string SkipShift = "skip_shift";
        public const string DriveToWork = "drive_to_work";
        public const string TransitToWork = "transit_to_work";
        public const string BuyPills = "buy_pills";
        public const string TakePill = "take_pill";
        public const string OpenPhone = "open_phone";
        public const string Cancel = "cancel";

        public static readonly string[] All =
        {
            SkipShift, DriveToWork, TransitToWork, BuyPills, TakePill, OpenPhone, Cancel,
        };

        public static bool IsKnown(string action) { return Array.IndexOf(All, action) >= 0; }
    }
}
