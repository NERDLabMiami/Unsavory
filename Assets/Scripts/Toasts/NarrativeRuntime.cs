using System;
using System.Collections.Generic;
using UnityEngine;
using Unsavory.Toasts;

// Messages chosen for one event. Once-per-run IDs are recorded when the text is actually shown (MarkShown).
public sealed class NarrativeMessage
{
    public readonly List<string> Lines = new List<string>();
    public readonly List<string> OnceIds = new List<string>();

    public void MarkShown() { NarrativeRuntime.MarkShown(OnceIds); }
}

public sealed class ModalChoiceView
{
    public string Label;
    public string Action;
    public bool Enabled;
}

public sealed class ModalView
{
    public string RuleId;
    public string Title;
    public string Text;
    public readonly List<ModalChoiceView> Choices = new List<ModalChoiceView>();
}

// What the player has learned from boss dialogue and mail ("reveals" in mail.txt / boss dialogue.txt).
// Never inferred from the day number.
public static class NarrativeKnowledge
{
    public const string Acquisition = "acquisition";
    public const string NewVision = "new_vision";

    static string Key(string tag) { return "knows " + tag; }
    public static void Learn(string tag) { if (!string.IsNullOrEmpty(tag)) PlayerPrefs.SetInt(Key(tag), 1); }
    public static bool Knows(string tag) { return PlayerPrefs.GetInt(Key(tag), 0) == 1; }
    public static void Forget() { PlayerPrefs.DeleteKey(Key(Acquisition)); PlayerPrefs.DeleteKey(Key(NewVision)); }
}

// The one place the pill price and rules live; the Drug Panel and the narrative pills modal both go through it.
public static class PillsService
{
    public enum Result { Ok, NoMoney, NoPills, DailyLimit, NotSick }

    public static float Price = 5f;
    public static float PillBoost = 3f;
    public static int MaxPerDay = 3;

    public static string PriceLabel { get { return "$" + Price.ToString("0.##"); } }
    public static bool CanAfford { get { return PlayerState.GetMoney() >= Price; } }

    // The scene's BuyDrugsScript carries the designer-set values; its panel is inactive until opened, so look it up directly.
    public static void SyncFromScene()
    {
        var script = UnityEngine.Object.FindAnyObjectByType<BuyDrugsScript>(FindObjectsInactive.Include);
        if (script == null) return;
        Price = script.costOfDrugs;
        PillBoost = script.pillBoost;
        MaxPerDay = script.maxPillsPerDay;
    }

    public static Result Buy()
    {
        float money = PlayerState.GetMoney();
        if (money < Price) return Result.NoMoney;
        PlayerState.SetMoney(money - Price);
        PlayerState.SetPills(PlayerState.GetPills() + 1);
        return Result.Ok;
    }

    public static Result Take()
    {
        int pills = PlayerState.GetPills();
        if (pills <= 0) return Result.NoPills;
        if (PlayerState.GetPillsTakenToday() >= MaxPerDay) return Result.DailyLimit;
        // Healthy enough not to sneeze: keep the pill.
        if (PlayerState.GetHealth() >= NoseWipeScript.SickHealth) return Result.NotSick;
        PlayerState.SetPills(pills - 1);
        PlayerState.RecordPillTaken(PillBoost);
        return Result.Ok;
    }

    public static string FailureText(Result result)
    {
        switch (result)
        {
            case Result.NoMoney: return "You don't have enough money";
            case Result.NoPills: return "You don't have any pills to take";
            case Result.DailyLimit: return "You've taken enough for today";
            case Result.NotSick: return "You're not sick enough to need it";
            default: return null;
        }
    }
}

// Loads toasts.txt (schema v3), builds the narrative context from authoritative game state, and answers
// entry / hover / modal / after-action lookups. Display and gameplay stay with the callers.
public static class NarrativeRuntime
{
    const string PlayedPref = "toasts played";       // "|rule id|" tokens for once:"run" rules
    const string EntryPref = "toast entry";          // pending entry event ("home" / "work"), set by a real transition
    const string SkippedPref = "skipped prev shift"; // the last transition was a skipped workday

    static NarrativeDocument _doc;
    static bool _loaded;

    // Rent is due today. Computed each Home load (not persisted), so Home publishes it here for hover/modal lookups.
    public static bool RentPending;

    static void HookDiagnostics()
    {
        Diagnostics.Warn = Debug.LogWarning;
        Diagnostics.Error = Debug.LogError;
        Diagnostics.LogMissingKeys = Debug.isDebugBuild || Application.isEditor;
    }

    public static NarrativeDocument Document()
    {
        if (_loaded) return _doc;
        _loaded = true;
        HookDiagnostics();
        var file = Resources.Load<TextAsset>("toasts");
        if (file == null) { Debug.LogError("[Narrative] Resources/toasts.txt is missing"); return null; }
        var doc = NarrativeParser.Parse(file.text);
        var errors = NarrativeValidator.Validate(doc);
        foreach (var error in errors) Debug.LogError("[Narrative] " + error);
        // A broken file shows nothing rather than half the story.
        _doc = errors.Count == 0 ? doc : null;
        return _doc;
    }

    // ---- Run lifecycle ----

    // New Game: forget what was shown, the pending entry and the handoff, and what the player had learned.
    public static void ResetRun()
    {
        PlayerPrefs.DeleteKey(PlayedPref);
        PlayerPrefs.DeleteKey(EntryPref);
        PlayerPrefs.DeleteKey(SkippedPref);
        NarrativeKnowledge.Forget();
        CommuteState.ResetRun();
        RentPending = false;
    }

    public static void MarkShown(IList<string> ids)
    {
        if (ids == null || ids.Count == 0) return;
        string played = PlayerPrefs.GetString(PlayedPref, "");
        foreach (var id in ids) if (played.IndexOf(Token(id), StringComparison.Ordinal) < 0) played += Token(id);
        PlayerPrefs.SetString(PlayedPref, played);
        PlayerPrefs.Save();
    }

    static string Token(string id) { return "|" + id + "|"; }
    static bool IsShown(string id) { return PlayerPrefs.GetString(PlayedPref, "").IndexOf(Token(id), StringComparison.Ordinal) >= 0; }

    // A real transition (arriving home, reporting to work) leaves a marker; the entry toast consumes it,
    // so reloads, Continue and retries of the same scene stay quiet.
    public static void MarkEntry(string kind) { PlayerPrefs.SetString(EntryPref, kind); }

    public static bool ConsumeEntry(string kind)
    {
        if (PlayerPrefs.GetString(EntryPref, "") != kind) return false;
        PlayerPrefs.DeleteKey(EntryPref);
        return true;
    }

    public static void SetSkippedPreviousShift() { PlayerPrefs.SetInt(SkippedPref, 1); }
    public static void ClearSkippedPreviousShift() { PlayerPrefs.DeleteKey(SkippedPref); }

    // ---- Context ----

    public static int RunEndCode()
    {
        switch (MailArchive.RunEnd())
        {
            case "layoff": return 1;
            case "eviction": return 2;
            default: return 0;
        }
    }

    // Read-only snapshot of the game as it is right now. `extra` lets a caller add or override transient facts.
    public static NarrativeContext Context(Action<NarrativeContext> extra = null)
    {
        var c = new NarrativeContext();
        c.Set("day", PlayerPrefs.GetInt("current level", 1));
        c.Set("run_end", RunEndCode());
        c.Set("first_visit", !PlayerPrefs.HasKey("welcomed home"));
        c.Set("rent_pending", RentPending);

        float money = PlayerState.GetMoney();
        c.Set("health", PlayerState.GetHealth());
        c.Set("money", money);
        c.Set("pills", PlayerState.GetPills());
        c.Set("pill_price", PillsService.PriceLabel);
        c.Set("can_afford_pills", money >= PillsService.Price);

        c.Set("missed_bills", BillsState.MissedCount());
        c.Set("bill_due", BillsState.DueCount());
        c.Set("unread_mail", MailArchive.UnreadCount());
        c.Set("phone_service", !BillsState.IsPhoneDead());

        // The car is out of reach once it is repossessed (car payment missed) or wrecked (crashed with the insurance overdue).
        // An overdue insurance bill alone does not take the car away: the crash is what does.
        bool carOk = CommuteState.CarOk;
        c.Set("car_ok", carOk);
        if (!carOk) c.Set("car_unavailable_reason", CommuteState.CarRepossessed ? "payment" : "accident");
        c.Set("insurance_lapsed", CommuteState.InsuranceLapsed);
        c.Set("crashed_commute", CommuteState.CrashedCommute);
        c.Set("late", CommuteState.LateEffect);

        c.Set("fruit_count", PlayerPrefs.GetInt("fruit", 5));
        c.Set("tutorial_done", PlayerPrefs.HasKey("tutorial"));
        c.Set("catering", PlayerPrefs.HasKey("catering"));

        c.Set("acquisition_known", NarrativeKnowledge.Knows(NarrativeKnowledge.Acquisition));
        c.Set("new_vision_known", NarrativeKnowledge.Knows(NarrativeKnowledge.NewVision));
        c.Set("skipped_previous_shift", PlayerPrefs.GetInt(SkippedPref, 0) == 1);

        if (extra != null) extra(c);
        return c;
    }

    // ---- Lookups ----

    // Scene entry ("home" / "work"): a special rule alone, else the world line then the state line.
    public static NarrativeMessage PickEntry(string kind, Action<NarrativeContext> extra = null)
    {
        var doc = Document();
        EntryChannels channels;
        if (doc == null || !doc.SceneEntry.TryGetValue(kind, out channels)) return null;

        var ctx = Context(extra);
        var message = new NarrativeMessage();
        foreach (var rule in RuleEvaluator.SelectEntry(channels, ctx, IsShown))
        {
            message.Lines.AddRange(RuleEvaluator.Messages(rule, ctx));
            if (rule.Once == "run") message.OnceIds.Add(rule.Id);
        }
        return message.Lines.Count > 0 ? message : null;
    }

    // Hover text for "phone" / "bed" / "door" / "pills" / "fruit_bowl". Never has side effects.
    public static string PickHover(string objectKey)
    {
        var doc = Document();
        List<Rule> rules;
        if (doc == null || !doc.Hover.TryGetValue(objectKey, out rules)) return null;
        var ctx = Context();
        var lines = RuleEvaluator.Messages(RuleEvaluator.First(rules, ctx), ctx);
        return lines.Count > 0 ? lines[0] : null;
    }

    public static NarrativeMessage PickAfter(string action)
    {
        var doc = Document();
        List<Rule> rules;
        if (doc == null || !doc.AfterAction.TryGetValue(action, out rules)) return null;
        var ctx = Context();
        var rule = RuleEvaluator.First(rules, ctx, IsShown);
        if (rule == null) return null;
        var message = new NarrativeMessage();
        message.Lines.AddRange(RuleEvaluator.Messages(rule, ctx));
        if (rule.Once == "run") message.OnceIds.Add(rule.Id);
        return message.Lines.Count > 0 ? message : null;
    }

    // The modal for "bed" / "door" / "pills", with each choice's enabled state evaluated now.
    public static ModalView PickModal(string objectKey)
    {
        var doc = Document();
        List<Rule> rules;
        if (doc == null || !doc.Modals.TryGetValue(objectKey, out rules)) return null;
        var ctx = Context();
        var rule = RuleEvaluator.First(rules, ctx);
        if (rule == null) return null;

        var view = new ModalView
        {
            RuleId = rule.Id,
            Title = RuleEvaluator.Interpolate(rule.Title, ctx, rule.Id),
            Text = RuleEvaluator.Interpolate(rule.Text, ctx, rule.Id),
        };
        foreach (var choice in rule.Choices)
            view.Choices.Add(new ModalChoiceView
            {
                Label = choice.Label,
                Action = choice.Action,
                Enabled = RuleEvaluator.IsEnabled(choice, ctx, rule.Id),
            });
        return view;
    }

    // Commit-time recheck: is this action still enabled in the modal that matches the game right now?
    public static bool IsActionEnabled(string objectKey, string action)
    {
        var view = PickModal(objectKey);
        if (view == null) return false;
        foreach (var choice in view.Choices)
            if (choice.Action == action) return choice.Enabled;
        return false;
    }
}
