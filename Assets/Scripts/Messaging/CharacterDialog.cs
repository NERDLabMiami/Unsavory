using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System;
using System.Collections;

public class CharacterDialog : MonoBehaviour
{
    [Header("Data")]
    public TextAsset dialog;
    public string key;

    [Header("UI")]
    public Text dialogBox;
    public GameObject advanceButton;
    public GameObject homeButton;
    public GameObject retryButton;
    public GameObject quitButton;
    public GameObject trays;
    public GameObject inGameGui;

    [Header("Optional legacy side effects (turn OFF when flow owns timing)")]
    public bool legacyTriggerCameraPrepOnComplete = false;
    public bool legacyShowInGameGuiOnComplete = false;

    public event Action OnDialogueComplete;

    [Header("Auto advance")]
    [Tooltip("Lines move on by themselves (Confirm skips ahead) and the Advance button stays hidden. Used for the intro.")]
    public bool autoAdvance;

    private Coroutine _auto;

    void Update()
    {
        // Banter by gamepad A / Enter. If the button is selected, the UI's own Submit already presses it.
        if (!autoAdvance && advanceButton && advanceButton.activeInHierarchy &&
            GameInput.Instance != null && GameInput.Instance.ConfirmPressed &&
            (UnityEngine.EventSystems.EventSystem.current == null ||
             UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != advanceButton))
        {
            advanceScript();
            return;
        }

        if (autoAdvance && _auto != null && GameInput.Instance != null && GameInput.Instance.ConfirmPressed)
        {
            StopCoroutine(_auto);
            _auto = null;
            advanceScript();
        }
    }

    private void ScheduleAutoAdvance()
    {
        if (_auto != null) { StopCoroutine(_auto); _auto = null; }
        if (autoAdvance) _auto = StartCoroutine(AutoAdvance());
    }

    private IEnumerator AutoAdvance()
    {
        string line = dialogBox ? dialogBox.text : "";
        // The first line also has to wait for the boss to slide in.
        float wait = Mathf.Clamp(1.2f + line.Length * 0.04f, 2f, 6f) + (dialogIndex == 0 ? 1.5f : 0f);
        yield return new WaitForSecondsRealtime(wait);
        _auto = null;
        advanceScript();
    }

    private int dialogIndex = 0;
    private int selectedIndex;
    private JSONNode jsonDialog;

    public void reappear() { reappear(true); }

    public void reappear(bool panCamera)
    {
        SetBossUIVisible(true);
        var anim = GetComponent<Animator>();
        if (anim) anim.SetTrigger("reappear");

        if (panCamera && Camera.main)
        {
            var camAnim = Camera.main.GetComponent<Animator>();
            if (camAnim) camAnim.SetTrigger("Pots");
        }
    }

    public void advanceScript()
    {
        if (jsonDialog == null || string.IsNullOrEmpty(key)) return;

        // Are we at the end?
        if (dialogIndex >= jsonDialog[key][selectedIndex].Count - 1)
        {
            // Hide boss UI immediately
            SetBossUIVisible(false);

            // Hide boss
            var childAnim = GetComponentInChildren<Animator>();
            if (childAnim) childAnim.SetTrigger("disappear");

            // Legacy behavior (make explicit & optional)
            if (legacyShowInGameGuiOnComplete && inGameGui) inGameGui.SetActive(true);

            if (legacyTriggerCameraPrepOnComplete && Camera.main)
            {
                var camAnim = Camera.main.GetComponent<Animator>();
                if (camAnim) camAnim.SetTrigger("Prep");
            }

            if (_auto != null) { StopCoroutine(_auto); _auto = null; }

            // NEW: tell the world dialogue finished
            OnDialogueComplete?.Invoke();
        }
        else
        {
            var childAnim = GetComponentInChildren<Animator>();
            if (childAnim) childAnim.SetTrigger("new text");
            dialogIndex++;
            updateSpeechBubble();
            ScheduleAutoAdvance();
        }
    }

    private void SetBossUIVisible(bool visible)
    {
        if (advanceButton) advanceButton.SetActive(visible && !autoAdvance);
    }

    private void SetFailOptionsVisible(bool visible)
    {
        if (homeButton) homeButton.SetActive(visible);
        if (retryButton) retryButton.SetActive(visible);
        if (quitButton) quitButton.SetActive(visible);
    }

    public void updateSpeechBubble()
    {
        if (jsonDialog == null || string.IsNullOrEmpty(key) || dialogBox == null) return;

        string response_key = key + "_response";
        dialogBox.text = jsonDialog[key][selectedIndex][dialogIndex];

        if (advanceButton)
        {
            Text responseText = advanceButton.GetComponentInChildren<Text>();
            if (responseText) responseText.text = jsonDialog[response_key][selectedIndex][dialogIndex];
        }
    }

    public void changeSpeechKey(string _key)
    {
        if (dialog == null) return;

        jsonDialog = JSON.Parse(dialog.ToString());
        key = _key;
        dialogIndex = 0;

        if (jsonDialog == null || jsonDialog[key] == null || jsonDialog[key].Count <= 0) return;

        selectedIndex = UnityEngine.Random.Range(0, jsonDialog[key].Count);

        SetBossUIVisible(true);

        if (dialogBox)
            dialogBox.text = jsonDialog[key][selectedIndex][dialogIndex];

        string response_key = _key + "_response";

        // Your original code sets BOTH home + advance button texts (odd but harmless)
        if (homeButton)
        {
            Text responseText = homeButton.GetComponentInChildren<Text>();
            if (responseText) responseText.text = jsonDialog[response_key][selectedIndex][dialogIndex];
        }

        if (advanceButton)
        {
            Text advanceText = advanceButton.GetComponentInChildren<Text>();
            if (advanceText) advanceText.text = jsonDialog[response_key][selectedIndex][dialogIndex];
        }

        ScheduleAutoAdvance();
    }
}
