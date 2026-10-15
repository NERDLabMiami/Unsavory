using UnityEngine;

public class HomeCareerController : MonoBehaviour
{
    [Tooltip("Optional: If you have an existing Player/Stats script, drag it here for stay-home side effects.")]
    public MonoBehaviour optionalStayHomeLogic; // placeholder hook

    public void GoToWork()
    {
        CareerFlowManager.Instance.BeginShift();
    }

    public void StayHome()
    {
        // If you have existing logic, call it here (or keep it in your existing WorkScript for now).
        // Example:
        // optionalStayHomeLogic?.SendMessage("ApplyStayHomeEffects", SendMessageOptions.DontRequireReceiver);

        CareerFlowManager.Instance.LoadHome();
    }
}