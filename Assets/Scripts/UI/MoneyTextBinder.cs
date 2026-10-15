using UnityEngine;
using UnityEngine.UI;

public class MoneyTextBinder : MonoBehaviour
{
    public Text moneyText;
    public string prefix = "$";
    public string format = "0";

    void OnEnable()
    {
        PlayerSignals.MoneyChanged += Handle;
        Handle(PlayerState.GetMoney());
    }

    void OnDisable()
    {
        PlayerSignals.MoneyChanged -= Handle;
    }

    private void Handle(float money)
    {
        if (moneyText) moneyText.text = prefix + money.ToString(format);
    }
}