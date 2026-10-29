using System;
using System.Collections.Generic;

namespace Unsavory.Toasts
{
    // Load-time checks. Every message names the rule ID (or the array path) so a bad entry is easy to find.
    public static class NarrativeValidator
    {
        static readonly string[] EntryKinds = { "home", "work" };
        static readonly string[] HoverKeys = { "phone", "bed", "door", "pills", "fruit_bowl" };
        static readonly string[] ModalKeys = { "bed", "door", "pills" };
        static readonly string[] AfterKeys = { "skip_shift", "buy_pills", "take_pill" };

        public static List<string> Validate(NarrativeDocument doc)
        {
            var errors = new List<string>();
            if (doc == null) { errors.Add("No narrative document"); return errors; }
            errors.AddRange(doc.ParseErrors);
            if (doc.SchemaVersion != 3) errors.Add("schema_version must be 3 (found " + doc.SchemaVersion + ")");

            var ids = new HashSet<string>();

            foreach (var kind in EntryKinds)
            {
                EntryChannels channels;
                if (!doc.SceneEntry.TryGetValue(kind, out channels)) { errors.Add("scene_entry." + kind + " is missing"); continue; }
                CheckRules(channels.Special, "scene_entry." + kind + ".special", false, ids, errors);
                CheckRules(channels.World, "scene_entry." + kind + ".world", false, ids, errors);
                CheckRules(channels.State, "scene_entry." + kind + ".state", false, ids, errors);
            }

            CheckGroups(doc.Hover, "hover", HoverKeys, false, ids, errors);
            CheckGroups(doc.Modals, "modals", ModalKeys, true, ids, errors);
            CheckGroups(doc.AfterAction, "after_action", AfterKeys, false, ids, errors);
            return errors;
        }

        static void CheckGroups(Dictionary<string, List<Rule>> groups, string path, string[] required, bool modal, HashSet<string> ids, List<string> errors)
        {
            foreach (var key in required)
            {
                List<Rule> rules;
                if (!groups.TryGetValue(key, out rules)) { errors.Add(path + "." + key + " is missing"); continue; }
                CheckRules(rules, path + "." + key, modal, ids, errors);
            }
        }

        static void CheckRules(List<Rule> rules, string path, bool modal, HashSet<string> ids, List<string> errors)
        {
            for (int i = 0; i < rules.Count; i++)
            {
                var rule = rules[i];
                string where = string.IsNullOrEmpty(rule.Id) ? path + "[" + i + "]" : path + " rule '" + rule.Id + "'";

                if (string.IsNullOrEmpty(rule.Id)) errors.Add(where + ": missing id");
                else if (!ids.Add(rule.Id)) errors.Add(where + ": duplicate id");

                CheckWhen(rule.When, where + " when", errors);
                if (rule.Once != null && rule.Once != "run") errors.Add(where + ": once must be \"run\" (found '" + rule.Once + "')");
                if (!rule.HasMessage) errors.Add(where + ": needs text or lines");
                if (rule.Lines != null)
                    for (int l = 0; l < rule.Lines.Count; l++)
                        if (string.IsNullOrEmpty(rule.Lines[l])) errors.Add(where + ": lines[" + l + "] is empty");
                CheckPlaceholders(rule.Text, where, errors);
                if (rule.Lines != null) foreach (var line in rule.Lines) CheckPlaceholders(line, where, errors);

                if (modal) CheckModal(rule, where, errors);
                else if (rule.Choices != null) errors.Add(where + ": choices are only valid on modals");
            }
        }

        static void CheckModal(Rule rule, string where, List<string> errors)
        {
            if (string.IsNullOrEmpty(rule.Title)) errors.Add(where + ": modal needs a title");
            if (string.IsNullOrEmpty(rule.Text)) errors.Add(where + ": modal needs text");
            if (rule.Choices == null || rule.Choices.Count == 0) { errors.Add(where + ": modal needs choices"); return; }

            bool canCancel = false;
            for (int c = 0; c < rule.Choices.Count; c++)
            {
                var choice = rule.Choices[c];
                string at = where + " choices[" + c + "]";
                if (string.IsNullOrEmpty(choice.Label)) errors.Add(at + ": missing label");
                if (string.IsNullOrEmpty(choice.Action)) errors.Add(at + ": missing action");
                else if (!NarrativeActions.IsKnown(choice.Action)) errors.Add(at + ": unknown action '" + choice.Action + "'");
                else if (choice.Action == NarrativeActions.Cancel) canCancel = true;
                CheckWhen(choice.EnabledWhen, at + " enabled_when", errors);
            }
            if (!canCancel) errors.Add(where + ": modal has no cancel choice (focus could be trapped)");
        }

        static void CheckWhen(Dictionary<string, string> when, string where, List<string> errors)
        {
            if (when == null) return;
            foreach (var kv in when)
            {
                string op, operand;
                if (!RuleEvaluator.TryParseCondition(kv.Value, out op, out operand))
                    errors.Add(where + ": malformed condition '" + kv.Key + "': '" + kv.Value + "'");
            }
        }

        static void CheckPlaceholders(string text, string where, List<string> errors)
        {
            if (string.IsNullOrEmpty(text)) return;
            int open = 0;
            foreach (char c in text)
            {
                if (c == '{') open++;
                else if (c == '}') open--;
                if (open < 0 || open > 1) { errors.Add(where + ": unbalanced braces in '" + text + "'"); return; }
            }
            if (open != 0) errors.Add(where + ": unbalanced braces in '" + text + "'");
        }
    }
}
