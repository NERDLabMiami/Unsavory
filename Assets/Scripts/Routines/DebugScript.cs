using UnityEngine;
using System.Collections;
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
		healthSlider.value = PlayerPrefs.GetFloat("health");
		healthText.text = healthSlider.value.ToString();
		cashSlider.value = PlayerPrefs.GetFloat("money");
		cashText.text = cashSlider.value.ToString();	
		pillSlider.value = PlayerPrefs.GetInt("pills");
		pillText.text = pillSlider.value.ToString();
		wageSlider.value = PlayerPrefs.GetFloat ("hourly rate", 8.05f);
		wageText.text = wageSlider.value.ToString();
		firedSlider.value = PlayerPrefs.GetInt("max warnings");
		firedText.text = firedSlider.value.ToString();
		daySlider.value = PlayerPrefs.GetInt("current level", 0);
		dayText.text = daySlider.value.ToString();
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
