using UnityEngine;

public class Electricity : MonoBehaviour
{
    public Animator lights;
    public int electricityBillId = 0;

    public static bool LightsOff { get; private set; }

    private bool lightsOff = false;

    void OnEnable()
    {
        BillsSignals.BillPaid += OnBillPaid;
        BillsSignals.BillsRefreshed += Refresh;
        Refresh();
    }

    void OnDisable()
    {
        BillsSignals.BillPaid -= OnBillPaid;
        BillsSignals.BillsRefreshed -= Refresh;
        LightsOff = false;
    }

    private void OnBillPaid(int id)
    {
        if (id == electricityBillId)
            Refresh();
    }

    public void Refresh()
    {
        bool overdue = BillsState.IsBillOverdue(electricityBillId);

        if (!lightsOff && overdue)
        {
            if (lights) lights.SetTrigger("lights off");
            lightsOff = true;
        }
        else if (lightsOff && !overdue)
        {
            if (lights) lights.SetTrigger("lights on");
            lightsOff = false;
        }
        else return;

        LightsOff = lightsOff;
        HomeInteractable.ApplyDim(lightsOff);
    }
}