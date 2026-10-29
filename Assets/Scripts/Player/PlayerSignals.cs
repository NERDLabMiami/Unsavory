using System;

public static class PlayerSignals
{
    // Fire when values change so UI can refresh immediately.
    public static event Action<float> MoneyChanged;
    public static event Action<int> PillsChanged;
    public static event Action<float> EarnedWagesChanged;
    public static event Action<float> HealthChanged;
    public static event Action<int> TrustChanged;
    public static event Action<int> FruitChanged;

    public static void RaiseMoneyChanged(float money) => MoneyChanged?.Invoke(money);
    public static void RaisePillsChanged(int pills) => PillsChanged?.Invoke(pills);
    public static void RaiseEarnedWagesChanged(float wages) => EarnedWagesChanged?.Invoke(wages);
    public static void RaiseHealthChanged(float health) => HealthChanged?.Invoke(health);
    public static void RaiseTrustChanged(int trust) => TrustChanged?.Invoke(trust);
    public static void RaiseFruitChanged(int fruit) => FruitChanged?.Invoke(fruit);
}