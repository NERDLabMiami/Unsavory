using System;
using System.Collections.Generic;
using SimpleJSON;

namespace Unsavory.Toasts
{
    // Reads the v2 JSON into plain objects. Shape problems are recorded in ParseErrors instead of throwing.
    public static class NarrativeParser
    {
        static readonly string[] EntryKinds = { "home", "work" };

        public static NarrativeDocument Parse(string json)
        {
            var doc = new NarrativeDocument();
            JSONNode root;
            try { root = JSON.Parse(json); }
            catch (Exception e) { doc.ParseErrors.Add("Narrative JSON could not be parsed: " + e.Message); return doc; }

            var rootObj = root as JSONClass;
            if (rootObj == null) { doc.ParseErrors.Add("Narrative JSON root must be an object"); return doc; }

            doc.SchemaVersion = rootObj["schema_version"].AsInt;

            var entry = rootObj["scene_entry"] as JSONClass;
            if (entry == null) doc.ParseErrors.Add("scene_entry must be an object");
            else
            {
                foreach (var kind in EntryKinds)
                {
                    var slot = entry[kind] as JSONClass;
                    if (slot == null) { doc.ParseErrors.Add("scene_entry." + kind + " must be an object"); continue; }
                    doc.SceneEntry[kind] = new EntryChannels
                    {
                        Special = Rules(slot["special"], "scene_entry." + kind + ".special", doc),
                        World = Rules(slot["world"], "scene_entry." + kind + ".world", doc),
                        State = Rules(slot["state"], "scene_entry." + kind + ".state", doc),
                    };
                }
            }

            Groups(rootObj["hover"], "hover", doc.Hover, doc);
            Groups(rootObj["modals"], "modals", doc.Modals, doc);
            Groups(rootObj["after_action"], "after_action", doc.AfterAction, doc);
            return doc;
        }

        static void Groups(JSONNode node, string path, Dictionary<string, List<Rule>> into, NarrativeDocument doc)
        {
            var obj = node as JSONClass;
            if (obj == null) { doc.ParseErrors.Add(path + " must be an object"); return; }
            foreach (KeyValuePair<string, JSONNode> kv in obj)
                into[kv.Key] = Rules(kv.Value, path + "." + kv.Key, doc);
        }

        static List<Rule> Rules(JSONNode node, string path, NarrativeDocument doc)
        {
            var list = new List<Rule>();
            var array = node as JSONArray;
            if (array == null) { doc.ParseErrors.Add(path + " must be an array"); return list; }
            for (int i = 0; i < array.Count; i++)
            {
                var obj = array[i] as JSONClass;
                if (obj == null) { doc.ParseErrors.Add(path + "[" + i + "] must be an object"); continue; }
                list.Add(ToRule(obj, path + "[" + i + "]", doc));
            }
            return list;
        }

        static Rule ToRule(JSONClass obj, string path, NarrativeDocument doc)
        {
            var rule = new Rule
            {
                Id = Str(obj["id"]),
                When = Conditions(obj["when"], path + ".when", doc),
                Once = Str(obj["once"]),
                Text = Str(obj["text"]),
                Title = Str(obj["title"]),
            };

            var lines = obj["lines"] as JSONArray;
            if (lines != null)
            {
                rule.Lines = new List<string>();
                for (int i = 0; i < lines.Count; i++) rule.Lines.Add(lines[i].Value);
            }
            else if (!string.IsNullOrEmpty(obj["lines"].Value))
                doc.ParseErrors.Add(path + ".lines must be an array");

            var choices = obj["choices"] as JSONArray;
            if (choices != null)
            {
                rule.Choices = new List<Choice>();
                for (int i = 0; i < choices.Count; i++)
                {
                    var c = choices[i] as JSONClass;
                    if (c == null) { doc.ParseErrors.Add(path + ".choices[" + i + "] must be an object"); continue; }
                    rule.Choices.Add(new Choice
                    {
                        Label = Str(c["label"]),
                        Action = Str(c["action"]),
                        EnabledWhen = Conditions(c["enabled_when"], path + ".choices[" + i + "].enabled_when", doc),
                    });
                }
            }
            return rule;
        }

        static Dictionary<string, string> Conditions(JSONNode node, string path, NarrativeDocument doc)
        {
            var obj = node as JSONClass;
            if (obj == null) return null;
            var map = new Dictionary<string, string>();
            foreach (KeyValuePair<string, JSONNode> kv in obj) map[kv.Key] = kv.Value.Value;
            return map;
        }

        // SimpleJSON's lazy creator reports a missing key as an empty value, so null means absent.
        static string Str(JSONNode node)
        {
            if (node == null) return null;
            var v = node.Value;
            return string.IsNullOrEmpty(v) ? null : v;
        }
    }
}
