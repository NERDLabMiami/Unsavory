using UnityEngine;
using SimpleJSON;

// Mail is an archive: {tokens} in mail.txt are resolved once, on the day a mail is delivered, and stored,
// so a day-5 paycheck mail still shows the day-5 numbers when it is opened on day 10.
// Mail without tokens is never stored. Mail with no stored copy (old saves) is resolved when read.
public static class MailArchive
{
    static JSONNode _mail;

    public static JSONNode Mail()
    {
        if (_mail != null) return _mail;
        var text = Resources.Load<TextAsset>("mail");
        _mail = text != null ? JSON.Parse(text.ToString())?["mail"] : null;
        return _mail;
    }

    static string SubjectKey(int i) => "mail subject " + i;
    static string BodyKey(int i) => "mail body " + i;
    static bool HasTokens(JSONNode m) => m["subject"].Value.IndexOf('{') >= 0 || m["body"].Value.IndexOf('{') >= 0;

    /// <summary>How a run ended ("layoff", "eviction"), or "" while it is still going. Ending mail ("ending" in mail.txt) is delivered when it matches.</summary>
    public const string RunEndKey = "run end";

    public static string RunEnd() => PlayerPrefs.GetString(RunEndKey, "");

    /// <summary>Whether mail <paramref name="i"/> is in the inbox: unlocked by the day, or, for ending mail, by how the run ended.</summary>
    public static bool IsAvailable(int i)
    {
        var mail = Mail();
        if (mail == null) return false;
        string ending = mail[i]["ending"].Value;
        if (ending.Length > 0) return ending == RunEnd();
        return mail[i]["unlockDay"].AsInt <= PlayerPrefs.GetInt("current level", 0);
    }

    /// <summary>Mail in the inbox that hasn't been opened yet.</summary>
    public static int UnreadCount()
    {
        var mail = Mail();
        if (mail == null) return 0;
        int n = 0;
        for (int i = 0; i < mail.Count; i++)
            if (IsAvailable(i) && PlayerPrefs.GetInt("mail" + i, 0) == 0) n++;
        return n;
    }

    /// <summary>The day shown on a mail: its unlock day, or for ending mail the day the run ended.</summary>
    public static int Day(int i)
    {
        var mail = Mail();
        if (mail == null) return 0;
        return mail[i]["ending"].Value.Length > 0 ? PlayerPrefs.GetInt("current level", 0) : mail[i]["unlockDay"].AsInt;
    }

    /// <summary>Freezes the tokens of every mail now in the inbox that hasn't been frozen yet. Call when the day advances or the run ends.</summary>
    public static void DeliverDue()
    {
        var mail = Mail();
        if (mail == null) return;

        var context = DialogueContext.Build();
        bool changed = false;
        for (int i = 0; i < mail.Count; i++)
        {
            if (!IsAvailable(i) || PlayerPrefs.HasKey(BodyKey(i)) || !HasTokens(mail[i])) continue;
            PlayerPrefs.SetString(SubjectKey(i), TokenFormatter.Apply(mail[i]["subject"].Value, context));
            PlayerPrefs.SetString(BodyKey(i), TokenFormatter.Apply(mail[i]["body"].Value, context));
            changed = true;
        }
        if (changed) PlayerPrefs.Save();
    }

    public static string Subject(int i) => Stored(SubjectKey(i), "subject", i);

    public static string Body(int i) => Stored(BodyKey(i), "body", i);

    static string Stored(string key, string field, int i)
    {
        if (PlayerPrefs.HasKey(key)) return PlayerPrefs.GetString(key);
        var mail = Mail();
        return mail != null ? TokenFormatter.Apply(mail[i][field].Value) : "";
    }

    /// <summary>New career: forget what was delivered and what was read.</summary>
    public static void Reset()
    {
        PlayerPrefs.DeleteKey(RunEndKey);
        var mail = Mail();
        if (mail == null) return;
        for (int i = 0; i < mail.Count; i++)
        {
            PlayerPrefs.DeleteKey(SubjectKey(i));
            PlayerPrefs.DeleteKey(BodyKey(i));
            PlayerPrefs.DeleteKey("mail" + i);
        }
    }
}
