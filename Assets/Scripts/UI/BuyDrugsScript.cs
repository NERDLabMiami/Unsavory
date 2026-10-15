using UnityEngine;
using UnityEngine.UI;

public class BuyDrugsScript : MonoBehaviour
{
    public int costOfDrugs = 5;
    public Text canvasTitle;

    public float healthIncreasePerPill = 1.0f;
    public float maxHealthForBenefit = 30f;

    void OnEnable()
    {
        PlayerState.EnsureDefaultsExist();
        SetHealthMessage();
    }

    public void buy()
    {
        float money = PlayerState.GetMoney();
        if (money >= costOfDrugs)
        {
            PlayerState.SetMoney(money - costOfDrugs);
            PlayerState.SetPills(PlayerState.GetPills() + 1);

            if (canvasTitle) canvasTitle.text = "Cold Medicine Purchased";
        }
        else
        {
            if (canvasTitle) canvasTitle.text = "You don't have enough money";
        }
    }

    public void take()
    {
        int pills = PlayerState.GetPills();
        if (pills <= 0)
        {
            if (canvasTitle) canvasTitle.text = "You don't have any pills to take";
            return;
        }

        PlayerState.SetPills(pills - 1);

        float health = PlayerState.GetHealth();
        if (health < maxHealthForBenefit)
        {
            PlayerState.SetHealth(health + healthIncreasePerPill);
            if (canvasTitle) canvasTitle.text = "Feeling Slightly Better";
        }
        else
        {
            if (canvasTitle) canvasTitle.text = "Already feeling okay";
        }
    }

    public void SetHealthMessage()
    {
        float health = PlayerState.GetHealth();
        if (!canvasTitle) return;

        if (health > 29)      canvasTitle.text = "Feeling Good!";
        else if (health > 25) canvasTitle.text = "Feeling Okay";
        else if (health > 20) canvasTitle.text = "Feeling Less Than Stellar";
        else if (health > 15) canvasTitle.text = "Feeling Nauseous";
        else if (health > 10) canvasTitle.text = "Feeling Awful";
        else if (health > 5)  canvasTitle.text = "Feeling Horrible";
        else                  canvasTitle.text = "Definitely Sick";
    }
}
