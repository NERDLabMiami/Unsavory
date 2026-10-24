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
// A boss line may set his expression: "face": "angry", "view": "straight"|"side" (sprite Resources/Boss/boss-<face>-<view>).
// Both stick until a later line changes them; view defaults to straight, and with no face the original portrait shows.
// If a face has no sprite in the requested view, the other view is used.
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
    // Completes once the portrait has slid in and the bubble has landed; answers and the first line's timer wait for it.
    Tween _showTween;
    // The boss's current expression; a line that names no face/view keeps the last one.
    string _face = "", _view = "straight";

    void Awake()
    {
        if (choiceButton)
        {
            var b = choiceButton.GetComponent<Button>();
            if (b) b.onClick.AddListener(ChooseSecond);
            _choiceRest = AnimUtil.RestScale(choiceButton.transform);
            choiceButton.SetActive(false);
        }
        // ModalFocus would rebuild the buttons' navigation from screen positions every frame; RevealAnswers links the two itself.
        var modal = advanceButton ? advanceButton.GetComponentInParent<ModalFocus>() : null;
        if (modal) modal.manualNavigation = true;
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
        _face = "";
        _view = "straight";
        AnimUtil.Get<BossPanelAnim>(gameObject).SetFace(null);
        _showTween = AnimUtil.Get<BossPanelAnim>(gameObject).Show();
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
                string text2 = null;
                if (CurrentHasChoices)
                {
                    text = Fill(node["choices"][0]["text"].Value);
                    // Only offer the second choice if the speech wrote one.
                    if (choiceButton && node["choices"].Count > 1) text2 = Fill(node["choices"][1]["text"].Value);
                }
                StartCoroutine(RevealAnswers(_line, text, text2));
            }
            return;
        }

        if (advanceButton) advanceButton.SetActive(false);
        float wait = node["wait"].Value != "" ? node["wait"].AsFloat : -1f;
        if (node["face"].Value != "") _face = node["face"].Value;
        if (node["view"].Value != "") _view = node["view"].Value;
        Sprite face = ResolveFace();
        if (_line == 0)
        {
            if (face) AnimUtil.Get<BossPanelAnim>(gameObject).SetFace(face);
            if (dialogBox) dialogBox.text = text;
            _wait = StartCoroutine(WaitThenNext(text, wait));
            return;
        }

        // A follow-up: the old panel pops out and comes back with the new words.
        _busy = true;
        AnimUtil.Get<BossPanelAnim>(gameObject).Swap(
            () =>
            {
                if (face) AnimUtil.Get<BossPanelAnim>(gameObject).SetFace(face);
                if (dialogBox) dialogBox.text = text;
            },
            () =>
            {
                _busy = false;
                if (_active) _wait = StartCoroutine(WaitThenNext(text, wait));
            });
    }

    // The player's answers appear after the bubble: the first (bottom) button pops, then the second above it.
    IEnumerator RevealAnswers(int line, string text, string text2)
    {
        if (_showTween != null) yield return _showTween;
        if (!_active || _line != line) yield break;

        var label = advanceButton.GetComponentInChildren<Text>();
        if (label) SetLabel(label, text);
        advanceButton.SetActive(true);
        BossPanelAnim.PopIn(advanceButton.transform, _buttonRest, 0.4f, 0.05f);
        if (text2 != null)
        {
            var label2 = choiceButton.GetComponentInChildren<Text>();
            if (label2) SetLabel(label2, text2);
            choiceButton.SetActive(true);
            BossPanelAnim.PopIn(choiceButton.transform, _choiceRest, 0.4f, 0.2f);
        }
        LinkAnswers(text2 != null);
        // Selected = highlighted, and Submit presses it.
        var es = UnityEngine.EventSystems.EventSystem.current;
        if (es != null) es.SetSelectedGameObject(advanceButton);
    }

    // The second answer sits above the first: up from Advance reaches it, down from it comes back.
    void LinkAnswers(bool both)
    {
        var advance = advanceButton.GetComponent<Selectable>();
        var choice = choiceButton ? choiceButton.GetComponent<Selectable>() : null;
        if (advance) advance.navigation = both && choice
            ? new Navigation { mode = Navigation.Mode.Explicit, selectOnUp = choice }
            : new Navigation { mode = Navigation.Mode.None };
        if (choice) choice.navigation = both
            ? new Navigation { mode = Navigation.Mode.Explicit, selectOnDown = advance }
            : new Navigation { mode = Navigation.Mode.None };
    }

    const string IconName = "Inline Icon";

    // A "{taco}" in an answer is drawn as the trust taco image: room is left in the text and the icon is parked there.
    void SetLabel(Text label, string text)
    {
        var old = label.transform.Find(IconName);
        if (old) Destroy(old.gameObject);
        int at = text.IndexOf("{taco}");
        var sprite = TrustTacoHud.FullSprite;
        if (at < 0 || sprite == null || label.font == null)
        {
            label.text = text.Replace("{taco}", "1 taco");
            return;
        }

        float size = label.fontSize * 1.1f;
        label.font.RequestCharactersInTexture("\u00A0", label.fontSize, label.fontStyle);
        CharacterInfo info;
        float advance = label.font.GetCharacterInfo('\u00A0', out info, label.fontSize, label.fontStyle) ? info.advance : label.fontSize * 0.3f;
        int count = Mathf.Max(1, Mathf.CeilToInt(size / Mathf.Max(1f, advance)));
        label.text = text.Substring(0, at) + new string('\u00A0', count) + text.Substring(at + "{taco}".Length);

        var rect = label.rectTransform.rect;
        label.cachedTextGenerator.Populate(label.text, label.GetGenerationSettings(rect.size));
        var chars = label.cachedTextGenerator.characters;
        if (at >= chars.Count) return;
        float x = chars[at].cursorPos.x / label.pixelsPerUnit;

        var go = new GameObject(IconName, typeof(RectTransform), typeof(Image));
        var rt = (RectTransform)go.transform;
        rt.SetParent(label.transform, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(size, size);
        rt.anchoredPosition = new Vector2(x, -rect.height * 0.5f);
        var image = go.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }

    // Resources/Boss/boss-<face>-<view>; if that view doesn't exist for the face, the other one is tried. Null keeps the current sprite.
    Sprite ResolveFace()
    {
        if (_face == "") return null;
        string other = _view == "side" ? "straight" : "side";
        var sprite = Resources.Load<Sprite>($"Boss/boss-{_face}-{_view}") ?? Resources.Load<Sprite>($"Boss/boss-{_face}-{other}");
        if (sprite == null) Debug.LogWarning($"[CharacterDialog] no boss sprite for face '{_face}' (view '{_view}').");
        return sprite;
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
        if (_line == 0 && _showTween != null) yield return _showTween;
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
