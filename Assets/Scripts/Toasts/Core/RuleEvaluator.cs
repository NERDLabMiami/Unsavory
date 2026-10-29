using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Unsavory.Toasts
{
    // Deterministic, ordered rule matching shared by every v2 rule array. No randomness, no scoring, no expression evaluation.
    public static class RuleEvaluator
    {
        static readonly string[] Operators = { ">=", "<=", "==", "!=", ">", "<" };

        // Splits "==1" / ">=11" / "before_shift" into operator and operand. A bare operand means equality.
        public static bool TryParseCondition(string expr, out string op, out string operand)
        {
            op = "=="; operand = null;
            if (expr == null) return false;
            string s = expr.Trim();
            if (s.Length == 0) return false;
            foreach (var candidate in Operators)
            {
                if (s.StartsWith(candidate, StringComparison.Ordinal))
                {
                    op = candidate;
                    s = s.Substring(candidate.Length).Trim();
                    break;
                }
            }
            if (s.Length == 0) return false;
            operand = s;
            double ignored;
            bool numeric = double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out ignored);
            // Strings only support equality; ordering a string against a string has no meaning in this data.
            return numeric || op == "==" || op == "!=";
        }

        public static bool Compare(string expr, object actual)
        {
            string op, operand;
            if (!TryParseCondition(expr, out op, out operand))
            {
                Diagnostics.E("Narrative: malformed condition '" + expr + "'");
                return false;
            }

            double want;
            if (double.TryParse(operand, NumberStyles.Float, CultureInfo.InvariantCulture, out want))
            {
                if (actual is string) return false;
                double have;
                try { have = Convert.ToDouble(actual, CultureInfo.InvariantCulture); }
                catch (Exception) { return false; }
                switch (op)
                {
                    case "==": return Math.Abs(have - want) < 1e-4;
                    case "!=": return Math.Abs(have - want) >= 1e-4;
                    case ">=": return have >= want;
                    case "<=": return have <= want;
                    case ">": return have > want;
                    case "<": return have < want;
                }
                return false;
            }

            // Exact string equality, e.g. phase "before_shift" or car_unavailable_reason "insurance".
            var text = actual as string;
            if (text == null) return false;
            bool equal = string.Equals(text, operand, StringComparison.Ordinal);
            return op == "==" ? equal : !equal;
        }

        // All predicates ANDed. A missing context key is a nonmatch (so "==0" never matches an unknown value).
        public static bool Matches(Dictionary<string, string> when, NarrativeContext ctx, string ruleId = null)
        {
            if (when == null || when.Count == 0) return true;
            foreach (var kv in when)
            {
                object actual;
                if (ctx == null || !ctx.TryGet(kv.Key, out actual))
                {
                    if (Diagnostics.LogMissingKeys)
                        Diagnostics.W("Narrative: rule '" + ruleId + "' needs context key '" + kv.Key + "', which is unavailable");
                    return false;
                }
                if (!Compare(kv.Value, actual)) return false;
            }
            return true;
        }

        public static bool IsEnabled(Choice choice, NarrativeContext ctx, string ruleId = null)
        {
            return choice == null || Matches(choice.EnabledWhen, ctx, ruleId);
        }

        // First matching rule in file order. A "once: run" rule that was already displayed is skipped.
        public static Rule First(IList<Rule> rules, NarrativeContext ctx, Func<string, bool> alreadyShown = null)
        {
            if (rules == null) return null;
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                if (rule.Once == "run" && alreadyShown != null && alreadyShown(rule.Id)) continue;
                if (Matches(rule.When, ctx, rule.Id)) return rule;
            }
            return null;
        }

        // A special rule is exclusive. Otherwise at most one world rule, then at most one state rule.
        public static List<Rule> SelectEntry(EntryChannels channels, NarrativeContext ctx, Func<string, bool> alreadyShown = null)
        {
            var picked = new List<Rule>();
            if (channels == null) return picked;
            var special = First(channels.Special, ctx, alreadyShown);
            if (special != null) { picked.Add(special); return picked; }
            var world = First(channels.World, ctx, alreadyShown);
            if (world != null) picked.Add(world);
            var state = First(channels.State, ctx, alreadyShown);
            if (state != null) picked.Add(state);
            return picked;
        }

        // "{name}" placeholders only; anything else is left alone. Unresolved names stay visible and warn.
        public static string Interpolate(string text, NarrativeContext ctx, string ruleId = null)
        {
            if (string.IsNullOrEmpty(text) || text.IndexOf('{') < 0) return text;
            var sb = new StringBuilder(text.Length + 8);
            int i = 0;
            while (i < text.Length)
            {
                char c = text[i];
                if (c == '{')
                {
                    int close = text.IndexOf('}', i + 1);
                    if (close > i + 1)
                    {
                        string name = text.Substring(i + 1, close - i - 1);
                        object value;
                        if (IsIdentifier(name) && ctx != null && ctx.TryGet(name, out value))
                        {
                            sb.Append(NarrativeContext.Format(value));
                            i = close + 1;
                            continue;
                        }
                        if (IsIdentifier(name))
                            Diagnostics.W("Narrative: rule '" + ruleId + "' has unresolved placeholder {" + name + "}");
                    }
                }
                sb.Append(c);
                i++;
            }
            return sb.ToString();
        }

        static bool IsIdentifier(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            for (int i = 0; i < name.Length; i++)
            {
                char c = name[i];
                if (!(char.IsLetterOrDigit(c) || c == '_')) return false;
            }
            return true;
        }

        // The rule's messages in order, interpolated. Empty messages are dropped.
        public static List<string> Messages(Rule rule, NarrativeContext ctx)
        {
            var result = new List<string>();
            if (rule == null) return result;
            if (rule.Lines != null && rule.Lines.Count > 0)
            {
                foreach (var line in rule.Lines)
                {
                    var text = Interpolate(line, ctx, rule.Id);
                    if (!string.IsNullOrEmpty(text)) result.Add(text);
                }
            }
            else
            {
                var text = Interpolate(rule.Text, ctx, rule.Id);
                if (!string.IsNullOrEmpty(text)) result.Add(text);
            }
            return result;
        }
    }
}
