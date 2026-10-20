using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Sits on the Pay button and forwards select/deselect to its BillScript row.</summary>
public class BillSelectionRelay : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public BillScript bill;
    public void OnSelect(BaseEventData e) { if (bill) bill.SetSelected(true); }
    public void OnDeselect(BaseEventData e) { if (bill) bill.SetSelected(false); }
}
