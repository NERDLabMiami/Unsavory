using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;

// The boss dialogue. Say(key, onFinished) plays one speech from boss dialogue.txt: the panel pops in and the lines play in order.
// A boss line steps on by itself after a minimum wait (Confirm cuts the wait short); a player line sits on the Advance
// button and waits for the player. After the last line the boss fades and onFinished runs.
// Where the player goes next (countdown, Home, main menu) is up to the caller.
//
// Data: { "key": [ { "when": { "late": ">0" }, "weight": 1, "once": false, "lines": [ { "who": "boss"|"player", "text": "...", "wait": 3, "shot": "Name" } ] } ] }
// Text may hold {name} tokens, filled from the vars passed to Say (e.g. "You earned {wages}.").
// A player line may instead offer choices: { "who": "player", "choices": [ { "id": "retry", "text": "Try again" }, { "id": "leave", "text": "End shift" } ] }.
// The first choice sits on the Advance button, the second on Choice Button. Picking one ends the speech and passes its id to onFinished.
// Which variant plays is decided by DialogueSelector from the conditions in "when" (see DialogueContext).
public class CharacterDialog : MonoBehaviour
{
    [Header("Data")]
    public TextAsset dialog;
    public string key;

    [Header("UI")]
    public Text dialogBox;
    public GameObject advanceButton;
    /// <summary>Second answer for a line with "choices" (the first is on advanceButton). Its click is hooked up here.</summary>
    public GameObject choiceButton;

    /// <summary>The boss's exit fade; yield on it to wait until he is gone. Null if he never left.</summary>
    public Tween HideTween { get; private set; }

    const float InputDelay = 0.4f;
    const float ReplyLead = 0.35f; // boss line -> player reply: just enough to see the order

    JSONNode _lines;
    int _line;
    bool _active;
    Action<string> _onFinished;
    Dictionary<string, string> _vars;
    string _pickedId;
    Coroutine _wait;
    // A press that opens the dialogue (e.g. Continue on the shift-complete panel) must not also advance it.
    float _inputReadyAt;
    // True while a pop is playing (the player's reply leaving, or the boss swapping to a follow-up): input is ignored.
    bool _busy;
    Vector3 _buttonRest;
    bool _haveButtonRest;
    Vector3 _choiceRest;

    void Awake()
    {
        if (choiceButton)
        {
            var b = choiceButton.GetComponent<Button>();
            if (b) b.onClick.AddListener(ChooseSecond);
            _choiceRest = AnimUtil.RestScale(choiceButton.transform);
            choiceButton.SetActive(false);
        }
    }

    /// <summary>Starts a speech. If the key is missing the speech ends at once, so the caller's flow is never stranded.</summary>
    /// <param name="extra">Context values (on top of DialogueContext.Build) that "when" conditions can test.</param>
    public void Say(string speechKey, Action onFinished = null, Dictionary<string, float> extra = null)
    {
        Say(speechKey, onFinished == null ? (Action<string>)null : id => onFinished(), extra);
    }

    /// <summary>As above; onFinished gets the id of the choice the player picked (null if the speech had none), and vars fill {name} tokens in the text.</summary>
    public void Say(string speechKey, Action<string> onFinished, Dictionary<string, float> extra = null, Dictionary<string, string> vars = null)
    {
        key = speechKey;
        _onFinished = onFinished;
        _vars = vars;
        _pickedId = null;
        _inputReadyAt = Time.unscaledTime + InputDelay;
        _line = 0;
        _active = true;
        _busy = false;
        HideTween = null;
        StopWait();

        var json = dialog != null ? JSON.Parse(dialog.ToString()) : null;
        JSONNode variants = json != null ? json[key] : null;
        int variant = -1;
        if (variants != null && variants.Count > 0)
        {
            var context = DialogueContext.Build();
            if (extra != null) foreach (var kv in extra) context[kv.Key] = kv.Value;
            variant = DialogueSelector.Pick(key, variants, context);
        }
        if (variant < 0)
        {
            Debug.LogError($"[CharacterDialog] no dialogue for key '{key}'; skipping it.");
            Finish();
            return;
        }

        if (variants[variant]["once"].Value == "true") DialogueSelector.MarkPlayed(key, variant);
        _lines = variants[variant]["lines"];
        if (dialogBox) dialogBox.text = "";
        AnimUtil.Get<BossPanelAnim>(gameObject).Show();
        ShowLine();
    }

    /// <summary>Advance button (and Confirm): ends a boss line's wait, or answers as the player; after the last line the speech is over.</summary>
    public void advanceScript()
    {
        if (!_active || _busy || Time.unscaledTime < _inputReadyAt) return;
        if (CurrentHasChoices) _pickedId = ChoiceId(0);
        Next();
    }

    void ChooseSecond()
    {
        if (!_active || _busy || Time.unscaledTime < _inputReadyAt || !CurrentHasChoices) return;
        _pickedId = ChoiceId(1);
        Next();
    }

    void Next()
    {
        StopWait();
        if (CurrentIsPlayer && advanceButton && advanceButton.activeInHierarchy)
        {
            StartCoroutine(PlayerReply());
            return;
        }
        NextLine();
    }

    // The reply button lights up, then pops out; the boss answers (or the speech ends) once it is gone.
    IEnumerator PlayerReply()
    {
        _busy = true;
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es != null) es.SetSelectedGameObject(null);

        var button = advanceButton.transform;
        var graphic = advanceButton.GetComponent<Graphic>();
        Color baseColor = graphic != null ? graphic.color : Color.white;
        if (graphic != null)
        {
            yield return Tween.Tint(graphic, Color.Lerp(baseColor, new Color(1f, 0.92f, 0.3f, baseColor.a), 0.7f), 0.1f, Tween.Ease.OutQuad);
            yield return Tween.Delay(0.08f);
        }
        if (choiceButton && choiceButton.activeSelf) BossPanelAnim.PopOut(choiceButton.transform);
        yield return BossPanelAnim.PopOut(button);

        advanceButton.SetActive(false);
        if (choiceButton) { choiceButton.SetActive(false); choiceButton.transform.localScale = _choiceRest; }
        button.localScale = _buttonRest;
        if (graphic != null) { Tween.Cancel(graphic, "color"); graphic.color = baseColor; }
        _busy = false;
        NextLine();
    }

    void NextLine()
    {
        if (_pickedId != null || _line >= _lines.Count - 1)
        {
            Finish();
            return;
        }

        _line++;
        ShowLine();
    }

    void Finish()
    {
        _active = false;
        _busy = false;
        StopWait();
        if (advanceButton) advanceButton.SetActive(false);
        if (choiceButton) choiceButton.SetActive(false);
        HideTween = AnimUtil.Get<BossPanelAnim>(gameObject).Hide();

        var done = _onFinished;
        _onFinished = null;
        if (done != null) done(_pickedId);
    }

    bool CurrentIsPlayer { get { return _lines[_line]["who"].Value == "player"; } }
    bool CurrentHasChoices { get { return CurrentIsPlayer && _lines[_line]["choices"].Count > 0; } }
    string ChoiceId(int i) { var id = _lines[_line]["choices"][i]["id"].Value; return id != "" ? id : i.ToString(); }

    // Fills {name} tokens from the vars given to Say.
    string Fill(string text)
    {
        if (_vars == null || string.IsNullOrEmpty(text)) return text;
        foreach (var kv in _vars) text = text.Replace("{" + kv.Key + "}", kv.Value);
        return text;
    }

    void ShowLine()
    {
        var node = _lines[_line];
        string text = Fill(ApplyShotTag(node["shot"].Value, node["text"].Value, out string shot));
        if (shot != "") CameraDirector.Go(shot);

        if (CurrentIsPlayer)
        {
            // The player's reply sits on the Advance button; the boss's last words stay on the panel above it.
            if (advanceButton)
            {
                if (!_haveButtonRest) { _buttonRest = AnimUtil.RestScale(advanceButton.transform); _haveButtonRest = true; }
                var label = advanceButton.GetComponentInChildren<Text>();
                if (CurrentHasChoices)
                {
                    text = Fill(node["choices"][0]["text"].Value);
                    // Only offer the second choice if the speech wrote one.
                    if (choiceButton && node["choices"].Count > 1)
                    {
                        var label2 = choiceButton.GetComponentInChildren<Text>();
                        if (label2) label2.text = Fill(node["choices"][1]["text"].Value);
                        choiceButton.SetActive(true);
                        BossPanelAnim.PopIn(choiceButton.transform, _choiceRest, 0.4f, 0.2f);
                    }
                }
                if (label) label.text = text;
                advanceButton.SetActive(true);
                BossPanelAnim.PopIn(advanceButton.transform, _buttonRest, 0.4f, 0.1f);
                // Selected = highlighted, and Submit presses it.
                var es = UnityEngine.EventSystems.EventSystem.current;
                if (es != null) es.SetSelectedGameObject(advanceButton);
            }
            return;
        }

        if (advanceButton) advanceButton.SetActive(false);
        float wait = node["wait"].Value != "" ? node["wait"].AsFloat : -1f;
        if (_line == 0)
        {
            if (dialogBox) dialogBox.text = text;
            _wait = StartCoroutine(WaitThenNext(text, wait));
            return;
        }

        // A follow-up: the old panel pops out and comes back with the new words.
        _busy = true;
        AnimUtil.Get<BossPanelAnim>(gameObject).Swap(
            () => { if (dialogBox) dialogBox.text = text; },
            () =>
            {
                _busy = false;
                if (_active) _wait = StartCoroutine(WaitThenNext(text, wait));
            });
    }

    void Update()
    {
        if (!_active || _busy || Time.unscaledTime < _inputReadyAt) return;
        var input = GameInput.Instance;
        if (input == null || !input.ConfirmPressed) return;

        // Confirm ends a boss line's wait. On a player line, if the button is selected the UI's own Submit already presses it.
        if (!CurrentIsPlayer) { Next(); return; }
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (advanceButton && advanceButton.activeInHierarchy && (es == null ||
            (es.currentSelectedGameObject != advanceButton && es.currentSelectedGameObject != choiceButton)))
            advanceScript();
    }

    IEnumerator WaitThenNext(string text, float wait)
    {
        bool replyNext = _line + 1 < _lines.Count && _lines[_line + 1]["who"].Value == "player";
        if (wait < 0f && replyNext) wait = ReplyLead;
        else if (wait < 0f) wait = Mathf.Clamp(1.2f + text.Length * 0.04f, 2f, 6f);
        yield return new WaitForSecondsRealtime(wait);
        _wait = null;
        if (_active && !_busy) Next();
    }

    void StopWait()
    {
        if (_wait != null) { StopCoroutine(_wait); _wait = null; }
    }

    // A line's camera shot is its "shot" field. The old "@ShotName|text" prefix still works.
    static string ApplyShotTag(string field, string text, out string shot)
    {
        shot = field;
        if (!string.IsNullOrEmpty(text) && text[0] == '@')
        {
            int bar = text.IndexOf('|');
            if (bar >= 2)
            {
                if (shot == "") shot = text.Substring(1, bar - 1);
                return text.Substring(bar + 1);
            }
        }
        return text;
    }
}
