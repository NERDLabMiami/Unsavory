using UnityEngine;
using UnityEngine.UI;

public class ConfirmPanel : MonoBehaviour
{
    [Header("UI")]
    public GameObject root;          // panel root
    public Text titleText;
    public Text bodyText;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Gate Lock")]
    public string lockReason = "HOME_CONFIRM";

    private HomeInteractable _pending;

    void Awake()
    {
        if (confirmButton) confirmButton.onClick.AddListener(Confirm);
        if (cancelButton) cancelButton.onClick.AddListener(Close);
        if (root) root.SetActive(false);
    }

    public void Open(HomeInteractable pending)
    {
        _pending = pending;

        bool goToWork = pending.action == HomeInteractable.HomeAction.GoToWork;
        bool runEnded = CareerFlowManager.Instance.RunEnded;

        if (titleText) titleText.text = runEnded ? "Head out?" : goToWork ? "Go to work?" : "Stay home?";

        if (bodyText) bodyText.text = runEnded
            ? "There's nothing left for you here. Time to find something else."
            : goToWork
                ? "You'll lose a little health from the trip, but you can earn wages today."
                : "You'll recover health, but you won't earn wages unless you have paid sick days.";

        if (root) root.SetActive(true);

        if (GameplayGate.Instance != null)
            GameplayGate.Instance.Lock(lockReason);
    }

    public void Close()
    {
        if (root) root.SetActive(false);

        if (GameplayGate.Instance != null)
            GameplayGate.Instance.Unlock(lockReason);

        _pending = null;
    }

    private void Confirm()
    {
        var p = _pending;
        Close();
        if (p != null) p.ExecuteAction();
    }
}