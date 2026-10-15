using UnityEngine;

public class ShiftEndReporter : MonoBehaviour
{
    public void EndShiftAndGoHome()
    {
        CareerFlowManager.Instance.LoadHome();
    }
}