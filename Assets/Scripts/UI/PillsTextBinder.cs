using UnityEngine;
using UnityEngine.UI;

public class PillsTextBinder : MonoBehaviour
{
    public Text pillsText;

    void OnEnable()
    {
        PlayerSignals.PillsChanged += Handle;
        Handle(PlayerState.GetPills());
    }

    void OnDisable()
    {
        PlayerSignals.PillsChanged -= Handle;
    }

    private void Handle(int pills)
    {
        if (pillsText) pillsText.text = pills.ToString();
    }
}