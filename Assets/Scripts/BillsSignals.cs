using System;

public static class BillsSignals
{
    public static event Action<int> BillPaid;     // id
    public static event Action<int> BillUnpaid;   // id (future)
    public static event Action BillsRefreshed;    // optional "rebuild UI" hook

    public static void RaiseBillPaid(int id) => BillPaid?.Invoke(id);
    public static void RaiseBillUnpaid(int id) => BillUnpaid?.Invoke(id);
    public static void RaiseBillsRefreshed() => BillsRefreshed?.Invoke();
}