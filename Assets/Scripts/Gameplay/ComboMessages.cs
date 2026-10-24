using UnityEngine;
using SimpleJSON;

// Reads Resources/combo.txt:
//   { "streak": { "2": "Nice work!", ... }, "fallbackFrom": 5, "fallback": "{n} IN A ROW!",
//     "breakMin": 3, "broken": [ "Consistency is key!", ... ] }
// "streak" holds the message for an exact count of orders in a row. A count with no entry shows "fallback"
// ({n} is the count) from "fallbackFrom" up, else nothing. Losing a streak of at least "breakMin" shows a random "broken" line.
public static class ComboMessages
{
	static JSONNode _data;
	static int _lastBroken = -1;

	static JSONNode Data() {
		if (_data != null) return _data;
		var file = Resources.Load<TextAsset>("combo");
		_data = file != null ? JSON.Parse(file.ToString()) : null;
		return _data;
	}

	/// <summary>The message for reaching this many orders in a row, or null.</summary>
	public static string ForStreak(int count) {
		var data = Data();
		if (data == null) return null;
		string text = data["streak"][count.ToString()].Value;
		if (text == "" && data["fallbackFrom"].Value != "" && count >= data["fallbackFrom"].AsInt) text = data["fallback"].Value;
		return text == "" ? null : text.Replace("{n}", count.ToString());
	}

	/// <summary>The message for losing a streak of this length, or null when it was too short to mention.</summary>
	public static string ForBroken(int count) {
		var data = Data();
		if (data == null || count < Mathf.Max(1, data["breakMin"].AsInt)) return null;
		var lines = data["broken"];
		if (lines == null || lines.Count <= 0) return null;

		// Don't repeat the line just shown.
		int i = Random.Range(0, lines.Count);
		if (lines.Count > 1 && i == _lastBroken) i = (i + 1) % lines.Count;
		_lastBroken = i;
		return lines[i].Value;
	}
}
