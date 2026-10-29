using UnityEngine;
using UnityEngine.UI;

public class DebugScript : MonoBehaviour {
	public Text healthText;
	public Slider healthSlider;
	public Text cashText;
	public Slider cashSlider;
	public Text pillText;
	public Slider pillSlider;
	public Text wageText;
	public Slider wageSlider;
	public Text firedText;
	public Slider firedSlider;
	public Slider daySlider;
	public Text dayText;

	// Use this for initialization
	void Start () {
		RefreshFromPrefs();
	}

	// Shows the saved values. SetValueWithoutNotify so merely opening this scene (or a value outside a
	// slider's range) doesn't write back through the onValueChanged handlers and change the save.
	public void RefreshFromPrefs() {
		SetSlider(healthSlider, healthText, PlayerPrefs.GetFloat("health"));
		SetSlider(cashSlider, cashText, PlayerPrefs.GetFloat("money"), "$");
		SetSlider(pillSlider, pillText, PlayerPrefs.GetInt("pills"));
		SetSlider(wageSlider, wageText, PlayerPrefs.GetFloat("hourly rate", 8.05f));
		SetSlider(firedSlider, firedText, PlayerPrefs.GetInt("max warnings"));
		SetSlider(daySlider, dayText, PlayerPrefs.GetInt("current level", 0));
	}

	void SetSlider(Slider slider, Text label, float value, string prefix = "") {
		if (slider != null) slider.SetValueWithoutNotify(value);
		if (label != null) label.text = prefix + value.ToString();
	}

	/// <summary>Reset button: wipe the player data like a fresh save, then show the new values.</summary>
	public void ResetAndRefresh() {
		CustomFunctionScript.resetPlayerData(25, 0);
		PlayerPrefs.Save();
		RefreshFromPrefs();
	}

	// Update is called once per frame
	void Update () {
	
	}

	public void setDay(float d) {
		dayText.text = d.ToString();
		PlayerPrefs.SetInt("current level", (int)d);
	}

	public void setHealth(float h) {
		healthText.text = h.ToString();
		PlayerPrefs.SetFloat("health", h);
	}

	public void setCash(float c) {
		cashText.text = "$" + c.ToString();
		PlayerPrefs.SetFloat("money", c);
	}

	public void setPills(float p) {
		pillText.text = p.ToString();
		PlayerPrefs.SetInt("pills", (int)p);
	}

	public void setWages(float w) {
		wageText.text = w.ToString();
		PlayerPrefs.SetFloat("hourly rate", w);
	}

	public void setFiredAttempts(float f) {
		firedText.text = f.ToString();
		PlayerPrefs.SetInt("max warnings", (int) f);
	}
}
