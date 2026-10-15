using UnityEngine;

public class HomeInteractable : MonoBehaviour, IWorldInteractable
{
    public enum HomeAction
    {
        GoToWork,
        StayHome,
        OpenBills,
        OpenPills,
        OpenLetter
    }

    public HomeAction action;

    [Header("Targets")]
    public WorkScript workScript;          // short-term: keep your existing stayHome/gotoWork logic
    public GameObject billsModalRoot;      // set active to open
    public GameObject pillsModalRoot;      // set active to open
    public GameObject letterModalRoot;
    [Header("Confirm UI")]
    public ConfirmPanel confirmPanel; // assign in inspector

    public void Interact()
    {
        if (GameplayGate.Instance != null && !GameplayGate.Instance.InteractionEnabled)
            return;

        switch (action)
        {
            case HomeAction.GoToWork:
            case HomeAction.StayHome:
                if (confirmPanel != null)
                    confirmPanel.Open(this);     // modal confirm
                else
                    ExecuteAction();             // fallback
                break;

            case HomeAction.OpenBills:
                if (billsModalRoot) billsModalRoot.SetActive(true);
                break;

            case HomeAction.OpenPills:
                if (pillsModalRoot) pillsModalRoot.SetActive(true);
                break;

            case HomeAction.OpenLetter:
                if (letterModalRoot) letterModalRoot.SetActive(true);
                break;
        }
    }

    public void ExecuteAction()
    {
        switch (action)
        {
            case HomeAction.GoToWork:
                if (workScript != null) workScript.gotoWork();
                else CareerFlowManager.Instance.BeginShift();
                break;

            case HomeAction.StayHome:
                if (workScript != null) workScript.stayHome();
                else CareerFlowManager.Instance.LoadHome();
                break;
        }
    }

}