using UnityEngine;

public class Electricity : MonoBehaviour
{
    public Animator lights;
    public int electricityBillId = 0;

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
    }
}